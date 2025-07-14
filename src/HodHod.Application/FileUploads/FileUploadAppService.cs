using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Abp.Authorization;
using Abp.IO.Extensions;
using Abp.UI;
using Microsoft.AspNetCore.Http;
using HodHod.FileUploads.Dto;
using HodHod.Storage;

namespace HodHod.FileUploads;

[AbpAllowAnonymous]
public class FileUploadAppService : HodHodAppServiceBase, IFileUploadAppService
{
    private const int MaxFileCount = 5;
    private const long MaxFileSize = 20 * 1024 * 1024; //20 MB

    private static readonly HashSet<string> AllowedExtensions = new()
    {
        ".jpg", ".jpeg", ".png", ".webp", ".gif",
        ".mp4", ".mov", ".avi", ".webm",
        ".mp3", ".wav", ".ogg",
        ".docx", ".txt", ".pdf"
    };

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITempFileCacheManager _tempFileCacheManager;
    public FileUploadAppService(IHttpContextAccessor httpContextAccessor, ITempFileCacheManager tempFileCacheManager)
    {
        _httpContextAccessor = httpContextAccessor;
        _tempFileCacheManager = tempFileCacheManager;
    }

    public async Task<List<UploadFileOutput>> UploadFiles()
    {
        var files = _httpContextAccessor.HttpContext?.Request?.Form?.Files;
        if (files == null || files.Count == 0)
        {
            throw new UserFriendlyException(L("File_Empty_Error"));
        }

        if (files.Count > MaxFileCount)
        {
            throw new UserFriendlyException("Too many files uploaded. Max is " + MaxFileCount);
        }

        var outputs = new List<UploadFileOutput>();

        foreach (var file in files)
        {
            if (file.Length > MaxFileSize)
            {
                throw new UserFriendlyException(L("File_SizeLimit_Error"));
            }

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(ext))
            {
                throw new UserFriendlyException("Unsupported file type: " + ext);
            }

            byte[] bytes;
            using (var stream = file.OpenReadStream())
            {
                bytes = stream.GetAllBytes();
            }

            var token = Guid.NewGuid().ToString("N");
            _tempFileCacheManager.SetFile(token, new TempFileInfo(file.FileName, file.ContentType, bytes));

            outputs.Add(new UploadFileOutput
            {
                Token = token,
                FileName = file.FileName
            });
        }

        return outputs;
    }
}