using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using Abp.IO.Extensions;
using Abp.UI;
using Abp.Web.Models;
using Microsoft.AspNetCore.Mvc;
using HodHod.DemoUiComponents.Dto;
using HodHod.Storage;
using HodHod.Storage.FileValidator;

namespace HodHod.Web.Controllers;

[AbpMvcAuthorize]
public class DemoUiComponentsController : HodHodControllerBase
{
    private readonly IBinaryObjectManager _binaryObjectManager;
    private readonly IFileValidatorManager _fileValidatorManager;

    public DemoUiComponentsController(IBinaryObjectManager binaryObjectManager, IFileValidatorManager fileValidatorManager)
    {
        _binaryObjectManager = binaryObjectManager;
        _fileValidatorManager = fileValidatorManager;
    }

    [HttpPost]
    public async Task<JsonResult> UploadFiles()
    {
        try
        {
            var files = Request.Form.Files;

            //Check input
            if (files == null)
            {
                throw new UserFriendlyException(L("File_Empty_Error"));
            }

            List<UploadFileOutput> filesOutput = new List<UploadFileOutput>();

            foreach (var file in files)
            {
                if (file.Length > 20 * 1024 * 1024) //20MB
                {
                    throw new UserFriendlyException(L("File_SizeLimit_Error"));
                }

                // Validate the uploaded file to ensure it meets the allowed file type and signature requirements.
                var validationResult = _fileValidatorManager.ValidateAll(new FileValidateInput(file));

                if (!validationResult.Success)
                {
                    throw new UserFriendlyException($"Validation failed: {validationResult.Message}");
                }

                byte[] fileBytes;
                using (var stream = file.OpenReadStream())
                {
                    fileBytes = stream.GetAllBytes();
                }

                var fileObject = new BinaryObject(AbpSession.TenantId, fileBytes, $"Demo ui, uploaded file {DateTime.UtcNow}");
                await _binaryObjectManager.SaveAsync(fileObject);

                filesOutput.Add(new UploadFileOutput
                {
                    Id = fileObject.Id,
                    FileName = file.FileName
                });
            }

            return Json(new AjaxResponse(filesOutput));
        }
        catch (UserFriendlyException ex)
        {
            return Json(new AjaxResponse(new ErrorInfo(ex.Message)));
        }
    }
}

