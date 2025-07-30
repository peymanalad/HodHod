using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO.Compression;
using Abp.Dependency;
using Minio;
using Minio.DataModel.Args;
using Minio.DataModel;
using System.Security.AccessControl;
using Twilio.TwiML.Voice;

namespace HodHod.Storage;

public class MinioFileManager : IMinioFileManager, ISingletonDependency
{
    private readonly IMinioClient _client;
    private readonly string _bucket;
    private bool _bucketChecked;

    public MinioFileManager()
    {
        var endpoint = Environment.GetEnvironmentVariable("MINIO_ENDPOINT") ?? "localhost:9000";
        var access = Environment.GetEnvironmentVariable("MINIO_ACCESS_KEY") ?? "minioadmin";
        var secret = Environment.GetEnvironmentVariable("MINIO_SECRET_KEY") ?? "minioadmin";
        _bucket = Environment.GetEnvironmentVariable("MINIO_BUCKET") ?? "hodhod";

        _client = new MinioClient()
            .WithEndpoint(endpoint)
            .WithCredentials(access, secret)
            .Build();
    }

    private async System.Threading.Tasks.Task EnsureBucketAsync()
    {
        if (_bucketChecked) return;
        var exists = await _client.BucketExistsAsync(new BucketExistsArgs().WithBucket(_bucket));
        if (!exists)
        {
            await _client.MakeBucketAsync(new MakeBucketArgs().WithBucket(_bucket));
        }
        _bucketChecked = true;
    }

    public async System.Threading.Tasks.Task UploadAsync(string objectName, byte[] content)
    {
        using var ms = new MemoryStream(content);
        await UploadStreamAsync(objectName, ms, "application/octet-stream");
    }

    public async System.Threading.Tasks.Task UploadStreamAsync(string objectName, System.IO.Stream content, string contentType, bool compress = false)
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
        MemoryStream? compressed = null;

        if (compress)
        {
            compressed = new MemoryStream();
            using (var gzip = new GZipStream(compressed, CompressionLevel.Optimal, leaveOpen: true))
            {
                await preparedStream.CopyToAsync(gzip);
            }
            compressed.Position = 0;
            uploadStream = compressed;
            if (preparedStream != content)
            {
                await preparedStream.DisposeAsync();
            }
        }

        var args = new PutObjectArgs()
            .WithBucket(_bucket)
            .WithObject(objectName)
            .WithContentType(contentType)
            .WithStreamData(uploadStream)
            .WithObjectSize(uploadStream.Length);

        if (compress)
        {
            args.WithHeaders(new Dictionary<string, string> { ["Content-Encoding"] = "gzip" });
        }

        await _client.PutObjectAsync(args);

        if (compress && compressed != null)
        {
            await uploadStream.DisposeAsync();
        }
        else if (!compress && preparedStream != content)
        {
            await preparedStream.DisposeAsync();
        }
    }

    public async System.Threading.Tasks.Task UploadManyAsync(
        IEnumerable<(string Name, System.IO.Stream Stream, string ContentType)> files, bool compress = false)
    {
        var tasks = new List<System.Threading.Tasks.Task>();
        foreach (var file in files)
        {
            tasks.Add(UploadStreamAsync(file.Name, file.Stream, file.ContentType, compress));
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

        await _client.GetObjectAsync(args.WithCallbackStream(stream => stream.CopyTo(ms)));
        ms.Position = 0;

        // If data looks like gzip, decompress before returning
        if (ms.Length >= 3)
        {
            int b1 = ms.ReadByte();
            int b2 = ms.ReadByte();
            int b3 = ms.ReadByte();
            ms.Position = 0;
            if (b1 == 0x1f && b2 == 0x8b && b3 == 0x08)
            {
                var decompressed = new MemoryStream();
                using (var gzip = new GZipStream(ms, CompressionMode.Decompress))
                {
                    await gzip.CopyToAsync(decompressed);
                }
                decompressed.Position = 0;
                return decompressed;
            }
        }

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
        var req = new PresignedGetObjectArgs()
            .WithBucket(_bucket)
            .WithObject(objectName)
            .WithExpiry(expirySeconds);
        return await _client.PresignedGetObjectAsync(req);
    }

    public async Task<string> GetPresignedPutUrlAsync(string objectName, int expirySeconds, string contentType)
    {
        await EnsureBucketAsync();
        var req = new PresignedPutObjectArgs()
            .WithBucket(_bucket)
            .WithObject(objectName)
            .WithExpiry(expirySeconds)
            .WithHeaders(new Dictionary<string, string> { ["Content-Type"] = contentType });
        return await _client.PresignedPutObjectAsync(req);
    }
}