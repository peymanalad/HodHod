﻿using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using Abp.IO.Extensions;
using Abp.UI;
using Abp.Web.Models;
using Microsoft.AspNetCore.Mvc;
using HodHod.Authorization;
using HodHod.Storage;
using HodHod.Web.Controllers;

namespace HodHod.Web.Areas.App.Controllers;

[Area("App")]
[AbpMvcAuthorize(AppPermissions.Pages_DemoUiComponents)]
public class DemoUiComponentsController : HodHodControllerBase
{
    private readonly IBinaryObjectManager _binaryObjectManager;

    public DemoUiComponentsController(IBinaryObjectManager binaryObjectManager)
    {
        _binaryObjectManager = binaryObjectManager;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public async Task<JsonResult> UploadFile(string defaultFileUploadTextInput)
    {
        try
        {
            var file = Request.Form.Files.First();

            //Check input
            if (file == null)
            {
                throw new UserFriendlyException(L("File_Empty_Error"));
            }

            if (file.Length > 1048576) //1MB
            {
                throw new UserFriendlyException(L("File_SizeLimit_Error"));
            }

            byte[] fileBytes;
            using (var stream = file.OpenReadStream())
            {
                fileBytes = stream.GetAllBytes();
            }

            var fileObject = new BinaryObject(AbpSession.TenantId, fileBytes, $"Demo ui, uploaded file {DateTime.UtcNow}");
            await _binaryObjectManager.SaveAsync(fileObject);

            return Json(new AjaxResponse(new
            {
                id = fileObject.Id,
                contentType = file.ContentType,
                defaultFileUploadTextInput = string.IsNullOrEmpty(defaultFileUploadTextInput) ? file.FileName : defaultFileUploadTextInput
            }));
        }
        catch (UserFriendlyException ex)
        {
            return Json(new AjaxResponse(new ErrorInfo(ex.Message)));
        }
    }

    public async Task<ActionResult> GetFile(Guid id, string contentType)
    {
        var fileObject = await _binaryObjectManager.GetOrNullAsync(id);
        if (fileObject == null)
        {
            return StatusCode((int)HttpStatusCode.NotFound);
        }

        return File(fileObject.Bytes, contentType);
    }
}

