using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using HodHod.Dto;

namespace HodHod.FileDownloads;

public interface IDownloadFileAppService : IApplicationService
{
    Task<List<FileDto>> GetReportFiles(Guid reportId);
}