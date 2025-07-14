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
using HodHod.Storage.FileValidator;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.IO.Extensions;
using Abp.UI;
using Abp.Web.Models;
using Microsoft.AspNetCore.Mvc;
using HodHod.DemoUiComponents.Dto;
using HodHod.Storage;
using HodHod.Storage.FileValidator;

namespace HodHod.FileUploads;

[AbpAllowAnonymous]
public class FileUploadAppService : HodHodAppServiceBase, IFileUploadAppService

{

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

    public async Task<List<FileUploads.Dto.UploadFileOutput>> UploadFiles()
    {
        var files = _httpContextAccessor.HttpContext?.Request?.Form?.Files;
        if (files == null || files.Count == 0)
        {
            throw new UserFriendlyException(L("فایلی برای ارسال انتخاب نشده! لطفا\u064b ابتدا یک فایل انتخاب کنید."));
        }

        var outputs = new List<FileUploads.Dto.UploadFileOutput>();

        foreach (var file in files)
        {
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(ext))
            {
                //throw new UserFriendlyException("Unsupported file type: " + ext);
                throw new UserFriendlyException("نوع فایل نامعتبر است!");

            }

            byte[] bytes;
            using (var stream = file.OpenReadStream())
            {
                bytes = stream.GetAllBytes();
            }

            var token = Guid.NewGuid().ToString("N");
            _tempFileCacheManager.SetFile(token, new TempFileInfo(file.FileName, file.ContentType, bytes));

            outputs.Add(new FileUploads.Dto.UploadFileOutput
            {
                Token = token,
                FileName = file.FileName
            });
        }

        return outputs;
    }
}