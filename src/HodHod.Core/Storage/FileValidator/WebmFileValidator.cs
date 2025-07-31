using Abp.Dependency;
using System.Collections.Generic;
using System.IO;

namespace HodHod.Storage.FileValidator;

public class WebmFileValidator : BaseFileValidator, ITransientDependency
{
    protected override HashSet<string> AllowedExtensions => new() { ".webm" };

    protected override HashSet<string> AllowedMimeTypes => new()
    {
        "audio/webm",
        "video/webm"
    };

    protected override Dictionary<string, List<byte[]>> AllowedFileSignatures => new()
    {
        { ".webm", new() { new byte[] { 0x1A, 0x45, 0xDF, 0xA3 } } }
    };

    public override bool CanValidate(IFileValidateInput file)
    {
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
        {
            return false;
        }

        var contentType = file.ContentType.ToLowerInvariant();
        if (contentType == "video/webm")
        {
            contentType = "audio/webm";
        }

        return AllowedMimeTypes.Contains(contentType);
    }

}