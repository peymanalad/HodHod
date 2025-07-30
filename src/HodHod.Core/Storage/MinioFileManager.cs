using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Abp.Dependency;
using Minio;
using Minio.DataModel.Args;
using Minio.DataModel;
using System.Security.AccessControl;
using Twilio.TwiML.Voice;
using System.Linq;
using Minio.Exceptions;

namespace HodHod.Storage;

public class MinioFileManager : IMinioFileManager, ISingletonDependency
{
    private readonly IMinioClient _client;
    private readonly string _bucket;
    private bool _bucketChecked;

    public MinioFileManager()
    {
        var endpoint = Environment.GetEnvironmentVariable("MINIO_ENDPOINT");
        var access = Environment.GetEnvironmentVariable("MINIO_ACCESS_KEY");
        var secret = Environment.GetEnvironmentVariable("MINIO_SECRET_KEY");
        _bucket = Environment.GetEnvironmentVariable("MINIO_BUCKET");

        var useSslEnv = Environment.GetEnvironmentVariable("MINIO_USE_SSL");
        bool useSsl = false;

        if (!string.IsNullOrWhiteSpace(endpoint))
        {
            if (endpoint.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                useSsl = true;
                endpoint = endpoint.Substring("https://".Length);
            }
            else if (endpoint.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            {
                endpoint = endpoint.Substring("http://".Length);
            }
        }

        if (!string.IsNullOrWhiteSpace(useSslEnv))
        {
            if (bool.TryParse(useSslEnv, out var parsedBool))
            {
                useSsl = parsedBool;
            }
            else if (int.TryParse(useSslEnv, out var parsedInt))
            {
                useSsl = parsedInt != 0;
            }
        }

        var builder = new MinioClient()
                .WithEndpoint(endpoint)
                .WithCredentials(access, secret);

        if (useSsl)
        {
            builder = builder.WithSSL();
        }

        _client = builder.Build();
    }

    private async System.Threading.Tasks.Task EnsureBucketAsync()
    {
        if (_bucketChecked) return;
        try
        {
            var exists = await _client.BucketExistsAsync(new BucketExistsArgs().WithBucket(_bucket));
            if (!exists)
            {
                await _client.MakeBucketAsync(new MakeBucketArgs().WithBucket(_bucket));
            }
            _bucketChecked = true;
        }
        catch (MinioException ex)
        {
            throw new InvalidOperationException($"Failed to access MinIO bucket '{_bucket}': {ex.Message}", ex);
        }
    }

    public async System.Threading.Tasks.Task UploadAsync(string objectName, byte[] content)
    {
        using var ms = new MemoryStream(content);
        await UploadStreamAsync(objectName, ms, "application/octet-stream");
    }

    public async System.Threading.Tasks.Task UploadStreamAsync(string objectName, System.IO.Stream content, string contentType)
    {
        await EnsureBucketAsync();

        System.IO.Stream preparedStream = content;

        // MinIO requires a seekable stream with a known length. If the incoming
        // stream does not support seeking or Length, buffer it first.
        if (!content.CanSeek)
        {
            var buffer = new MemoryStream();
            await content.CopyToAsync(buffer);
            buffer.Position = 0;
            preparedStream = buffer;
        }

        System.IO.Stream uploadStream = preparedStream;

        var args = new PutObjectArgs()
            .WithBucket(_bucket)
            .WithObject(objectName)
            .WithContentType(contentType)
            .WithStreamData(uploadStream)
            .WithObjectSize(uploadStream.Length);

        try
        {
            await _client.PutObjectAsync(args);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        if (preparedStream != content)
        {
            await preparedStream.DisposeAsync();
        }
    }

    public async System.Threading.Tasks.Task UploadManyAsync(
        IEnumerable<(string Name, System.IO.Stream Stream, string ContentType)> files)
    {
        var tasks = new List<System.Threading.Tasks.Task>();
        foreach (var file in files)
        {
            tasks.Add(UploadStreamAsync(file.Name, file.Stream, file.ContentType));
        }

        await System.Threading.Tasks.Task.WhenAll(tasks);
    }

    public async Task<byte[]> DownloadAsync(string objectName)
    {
        await using var stream = await DownloadStreamAsync(objectName);
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms);
        return ms.ToArray();
    }

    public async Task<System.IO.Stream> DownloadStreamAsync(string objectName, long? offset = null, long? length = null)
    {
        await EnsureBucketAsync();
        var ms = new MemoryStream();

        var args = new GetObjectArgs()
            .WithBucket(_bucket)
            .WithObject(objectName);
        if (offset.HasValue || length.HasValue)
        {
            args.WithOffsetAndLength(offset ?? 0, length ?? -1);
        }

        ObjectStat? stat = null;
        try
        {
            stat = await _client.StatObjectAsync(
                new StatObjectArgs().WithBucket(_bucket).WithObject(objectName));
        }
        catch (MinioException)
        {
            // Object may not exist or metadata may be unavailable
        }

        await _client.GetObjectAsync(args.WithCallbackStream(stream => stream.CopyTo(ms)));
        ms.Position = 0;

        return ms;
    }
    public async System.Threading.Tasks.Task ProcessStreamAsync(string objectName, Func<System.IO.Stream, System.Threading.Tasks.Task> processor, long? offset = null, long? length = null)
    {
        await using var stream = await DownloadStreamAsync(objectName, offset, length);
        await processor(stream);
    }

    public async Task<string> GetPresignedGetUrlAsync(string objectName, int expirySeconds)
    {
        await EnsureBucketAsync();

        ObjectStat? stat = null;
        try
        {
            stat = await _client.StatObjectAsync(
                new StatObjectArgs().WithBucket(_bucket).WithObject(objectName));
        }
        catch (MinioException)
        {
            // Object might not exist yet; ignore
        }
        var expirySecondsStr = Environment.GetEnvironmentVariable("MINIO_LINK_EXPIRETION_SECONDS");

        if (!int.TryParse(expirySecondsStr, out var expireSec))
        {
            expireSec = 3600;
        }

        var req = new PresignedGetObjectArgs()
            .WithBucket(_bucket)
            .WithObject(objectName)
            .WithExpiry(expireSec);

        req.WithHeaders(new Dictionary<string, string>
        {

            ["response-content-type"] = stat.ContentType
        });

        return await _client.PresignedGetObjectAsync(req);
    }

    public async Task<string> GetPresignedPutUrlAsync(string objectName, int expirySeconds, string contentType)
    {
        await EnsureBucketAsync();
        var expirySecondsStr = Environment.GetEnvironmentVariable("MINIO_LINK_EXPIRETION_SECONDS");

        if (!int.TryParse(expirySecondsStr, out var expireSec))
        {
            expireSec = 3600;
        }
        var req = new PresignedPutObjectArgs()
            .WithBucket(_bucket)
            .WithObject(objectName)
            .WithExpiry(expireSec)
            .WithHeaders(new Dictionary<string, string> { ["Content-Type"] = contentType });
        return await _client.PresignedPutObjectAsync(req);
    }
}