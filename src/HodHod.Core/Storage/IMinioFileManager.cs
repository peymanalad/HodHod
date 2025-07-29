using System.Threading.Tasks;
using Abp.Dependency;

namespace HodHod.Storage;

public interface IMinioFileManager : ITransientDependency
{
    Task UploadAsync(string objectName, byte[] content);
    Task<byte[]> DownloadAsync(string objectName);
}