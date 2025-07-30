using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using HodHod.Dto;
using HodHod.Reports.Dto;

namespace HodHod.FileDownloads;

public interface IDownloadFileAppService : IApplicationService
{
    Task<List<FileDto>> GetReportFiles(Guid reportId);
    Task<FileDto> GetReportFile(Guid reportFileId);
    Task<FileDto> GetReportFileData(Guid reportFileId);
    Task<FileDto> GetReportFileSnapshot(Guid reportFileId);
    Task<string> GetReportFilePresignedUrl(Guid reportFileId, int expirySeconds = 3600);
    Task<ReportFileCountsDto> GetReportFileCounts(Guid reportId);
    Task<List<Guid>> GetReportFileIds(Guid reportId);
}