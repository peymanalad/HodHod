using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Abp.Authorization;
using Abp.AspNetZeroCore.Net;
using Abp.Domain.Repositories;
using HodHod.Dto;
using HodHod.Storage;
using Abp.MimeTypes;
using HodHod.Reports;
using Microsoft.EntityFrameworkCore;

namespace HodHod.FileDownloads;

[AbpAllowAnonymous]
public class DownloadFileAppService : HodHodAppServiceBase, IDownloadFileAppService
{
    private readonly IRepository<ReportFile, Guid> _reportFileRepository;
    private readonly ITempFileCacheManager _tempFileCacheManager;
    private readonly IAppFolders _appFolders;
    private readonly IMimeTypeMap _mimeTypeMap;

    public DownloadFileAppService(
        IRepository<ReportFile, Guid> reportFileRepository,
        ITempFileCacheManager tempFileCacheManager,
        IAppFolders appFolders,
        IMimeTypeMap mimeTypeMap)
    {
        _reportFileRepository = reportFileRepository;
        _tempFileCacheManager = tempFileCacheManager;
        _appFolders = appFolders;
        _mimeTypeMap = mimeTypeMap;
    }

    public async Task<List<FileDto>> GetReportFiles(Guid reportId)
    {
        var files = await _reportFileRepository.GetAll()
            .Where(f => f.ReportId == reportId)
            .ToListAsync();

        var result = new List<FileDto>();
        foreach (var file in files)
        {
            var physicalPath = Path.Combine(_appFolders.ReportFilesFolder, file.FilePath);
            if (!File.Exists(physicalPath))
            {
                continue;
            }

            var bytes = await File.ReadAllBytesAsync(physicalPath);
            var mimeType = _mimeTypeMap.GetMimeType(file.FileName);
            var dto = new FileDto(file.FileName, mimeType);
            _tempFileCacheManager.SetFile(dto.FileToken, new TempFileInfo(file.FileName, mimeType, bytes));
            result.Add(dto);
        }

        return result;
    }
}