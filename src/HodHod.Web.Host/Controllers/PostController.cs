﻿using System;
using System.IO;
using System.Linq;
using Abp.IO.Extensions;
using Abp.UI;
using Abp.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using HodHod.Storage;

namespace Chamran.Deed.Web.Controllers
{
    [Authorize]
    public class PostsController : DeedControllerBase
    {
        private readonly ITempFileCacheManager _tempFileCacheManager;

        private const long MaxPostFileLength = 157286400; //150MB
        private const string MaxPostFileLengthUserFriendlyValue = "150MB"; //150MB
        private readonly string[] PostFileAllowedFileTypes =
        {
            ".jpg", ".jpeg", ".png", ".webp", ".gif",
            ".mp4", ".mov", ".avi", ".webm",
            ".mp3", ".wav", ".ogg",
            ".docx", ".txt", ".pdf"
        };
        //private readonly IPostMediaProcessorAppService _postMediaProcessorAppService;

        public PostsController(ITempFileCacheManager tempFileCacheManager/*, IPostMediaProcessorAppService iPostMediaProcessorAppService*/)
        {
            _tempFileCacheManager = tempFileCacheManager;
            //_postMediaProcessorAppService = iPostMediaProcessorAppService;
        }

        public FileUploadCacheOutput UploadPostFileFile()
        {
            try
            {
                //Check input
                if (Request.Form.Files.Count == 0)
                {
                    throw new UserFriendlyException(L("NoFileFoundError"));
                }

                var file = Request.Form.Files.First();
                if (file.Length > MaxPostFileLength)
                {
                    throw new UserFriendlyException(L("Warn_File_SizeLimit", MaxPostFileLengthUserFriendlyValue));
                }

                var fileType = Path.GetExtension(file.FileName).Substring(1);
                if (PostFileAllowedFileTypes != null && PostFileAllowedFileTypes.Length > 0 && !PostFileAllowedFileTypes.Contains(fileType))
                {
                    throw new UserFriendlyException(L("FileNotInAllowedFileTypes", PostFileAllowedFileTypes));
                }

                byte[] fileBytes;
                using (var stream = file.OpenReadStream())
                {
                    fileBytes = stream.GetAllBytes();
                }

                var fileToken = Guid.NewGuid().ToString("N");
                _tempFileCacheManager.SetFile(fileToken, new TempFileInfo(file.FileName, fileType, fileBytes));

                return new FileUploadCacheOutput(fileToken);
            }
            catch (UserFriendlyException ex)
            {
                return new FileUploadCacheOutput(new ErrorInfo(ex.Message));
            }
        }

        //[HttpPost("regenerate-thumbnails-previews")]
        //public async Task<IActionResult> Regenerate()
        //{
        //    await _postMediaProcessorAppService.RegenerateAllThumbnailsAndPreviewsAsync();
        //    return Ok("تمام فایل‌ها پردازش شدند.");
        //}
        //public string[] GetPostFileFileAllowedTypes()
        //{
        //    return PostFileAllowedFileTypes;
        //}

    }
}