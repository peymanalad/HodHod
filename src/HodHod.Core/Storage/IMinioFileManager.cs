using System.Collections.Generic;
using System;
using System.IO;
using System.Threading.Tasks;
using Abp.Dependency;

namespace HodHod.Storage;

public interface IMinioFileManager : ITransientDependency
{
    Task UploadAsync(string objectName, byte[] content);

    Task UploadStreamAsync(
        string objectName,
        Stream content,
        string contentType,
        bool compress = false);
    Task UploadManyAsync(IEnumerable<(string Name, Stream Stream, string ContentType)> files, bool compress = false);

    Task<byte[]> DownloadAsync(string objectName);

    Task<Stream> DownloadStreamAsync(string objectName, long? offset = null, long? length = null);

    Task ProcessStreamAsync(string objectName, Func<Stream, Task> processor, long? offset = null, long? length = null);

    Task<string> GetPresignedGetUrlAsync(string objectName, int expirySeconds);

    Task<string> GetPresignedPutUrlAsync(string objectName, int expirySeconds, string contentType);
}