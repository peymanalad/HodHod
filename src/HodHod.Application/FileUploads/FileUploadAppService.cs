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
using Abp.Domain.Repositories;
using Abp.IO.Extensions;
using Abp.UI;
using Abp.Web.Models;
using HodHod.BlackLists;
using Microsoft.AspNetCore.Mvc;
using HodHod.DemoUiComponents.Dto;
using HodHod.Storage;
using HodHod.Storage.FileValidator;
using System.Linq;

namespace HodHod.FileUploads;

[AbpAllowAnonymous]
public class FileUploadAppService : HodHodAppServiceBase, IFileUploadAppService

{

    private static readonly HashSet<string> AllowedExtensions = new()
    {
        ".jpg", ".jpeg", ".png", ".webp", ".gif",
        ".mp4", ".mov", ".avi", ".webm",
        ".mp3", ".wav", ".ogg",
        ".docx", ".txt"/*, ".pdf"*/
    };

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITempFileCacheManager _tempFileCacheManager;
    private readonly IRepository<BlackListEntry, int> _blackListRepository;
    private readonly IMinioFileManager _minioFileManager;

    public FileUploadAppService(
        IHttpContextAccessor httpContextAccessor,
        ITempFileCacheManager tempFileCacheManager,
        IRepository<BlackListEntry, int> blackListRepository,
        IMinioFileManager minioFileManager)
    {
        _httpContextAccessor = httpContextAccessor;
        _tempFileCacheManager = tempFileCacheManager;
        _blackListRepository = blackListRepository;
        _minioFileManager = minioFileManager;
    }

    public async Task<List<FileUploads.Dto.UploadFileOutput>> UploadFiles([FromForm] IFormCollection form)
    {
        await CheckBlackListFromHeaderAsync();
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

    public async Task<string> GetPresignedUploadUrl(string fileName, string contentType, int expirySeconds = 3600)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
        {
            throw new UserFriendlyException("نوع فایل نامعتبر است!");
        }

        var uniqueName = Guid.NewGuid().ToString("N") + ext;
        return await _minioFileManager.GetPresignedPutUrlAsync(uniqueName, expirySeconds, contentType);
    }


    private async Task CheckBlackListFromHeaderAsync()
    {
        var phone = _httpContextAccessor.HttpContext?.Request?.Headers["X-PhoneNumber"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(phone))
        {
            return;
        }
        var normalized = PhoneNumberHelper.Normalize(phone);
        if (long.TryParse(normalized, out var digits))
        {
            if (await _blackListRepository.CountAsync(e => e.PhoneNumber == digits) > 0)
            {
                throw new UserFriendlyException(L("PhoneNumberBlackListed"));
            }
        }
    }
}