using Abp.Dependency;
using System.Collections.Generic;

namespace HodHod.Storage.FileValidator;

public class WebmFileValidator : BaseFileValidator, ITransientDependency
{
    protected override HashSet<string> AllowedExtensions => new() { ".webm" };

    protected override HashSet<string> AllowedMimeTypes => new()
    {
        "video/webm"
    };

    protected override Dictionary<string, List<byte[]>> AllowedFileSignatures => new()
    {
        { ".webm", new() { new byte[] { 0x1A, 0x45, 0xDF, 0xA3 } } }
    };
}