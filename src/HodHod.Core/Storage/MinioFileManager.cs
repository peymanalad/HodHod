using System;
using System.IO;
using System.Threading.Tasks;
using Abp.Dependency;
using Minio;
using Minio.DataModel.Args;

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

    private async Task EnsureBucketAsync()
    {
        if (_bucketChecked) return;
        var exists = await _client.BucketExistsAsync(new BucketExistsArgs().WithBucket(_bucket));
        if (!exists)
        {
            await _client.MakeBucketAsync(new MakeBucketArgs().WithBucket(_bucket));
        }
        _bucketChecked = true;
    }

    public async Task UploadAsync(string objectName, byte[] content)
    {
        await EnsureBucketAsync();
        using var ms = new MemoryStream(content);
        await _client.PutObjectAsync(new PutObjectArgs()
            .WithBucket(_bucket)
            .WithObject(objectName)
            .WithStreamData(ms)
            .WithObjectSize(ms.Length));
    }

    public async Task<byte[]> DownloadAsync(string objectName)
    {
        await EnsureBucketAsync();
        using var ms = new MemoryStream();

        await _client.GetObjectAsync(
            new GetObjectArgs()
                .WithBucket(_bucket)
                .WithObject(objectName)
                .WithCallbackStream(stream => stream.CopyTo(ms))
        );

        return ms.ToArray();
    }

}