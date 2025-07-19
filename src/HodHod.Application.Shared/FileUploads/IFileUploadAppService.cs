using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using HodHod.FileUploads.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HodHod.FileUploads;

public interface IFileUploadAppService : IApplicationService
{
    Task<List<UploadFileOutput>> UploadFiles([FromForm] IFormCollection form);
}