using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using HodHod.FileUploads.Dto;

namespace HodHod.FileUploads;

public interface IFileUploadAppService : IApplicationService
{
    Task<List<UploadFileOutput>> UploadFiles();
}