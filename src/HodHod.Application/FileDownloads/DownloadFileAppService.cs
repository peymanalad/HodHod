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
using HodHod.Reports.Dto;
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
            var mimeType = _mimeTypeMap.GetMimeType(file.FileName);
            var dto = new FileDto(file.FileName, mimeType)
            {
                Id = file.Id
            };
            result.Add(dto);
        }

        return result;
    }

    public async Task<List<Guid>> GetReportFileIds(Guid reportId)
    {
        return await _reportFileRepository.GetAll()
            .Where(f => f.ReportId == reportId)
            .Select(f => f.Id)
            .ToListAsync();
    }

    public async Task<FileDto> GetReportFile(Guid reportFileId)
    {
        var file = await _reportFileRepository.FirstOrDefaultAsync(reportFileId);
        if (file == null)
        {
            return null;
        }

        var physicalPath = Path.Combine(_appFolders.ReportFilesFolder, file.FilePath);
        if (!File.Exists(physicalPath))
        {
            return null;
        }

        var bytes = await File.ReadAllBytesAsync(physicalPath);
        var mimeType = _mimeTypeMap.GetMimeType(file.FileName);
        var dto = new FileDto(file.FileName, mimeType)
        {
            Id = file.Id
        };
        _tempFileCacheManager.SetFile(dto.FileToken, new TempFileInfo(file.FileName, mimeType, bytes));
        return dto;
    }

    public async Task<FileDto> GetReportFileSnapshot(Guid reportFileId)
    {
        var file = await _reportFileRepository.FirstOrDefaultAsync(reportFileId);
        if (file == null)
        {
            return null;
        }

        var physicalPath = Path.Combine(_appFolders.ReportFilesFolder, file.FilePath);
        if (!File.Exists(physicalPath))
        {
            return null;
        }

        var mimeType = _mimeTypeMap.GetMimeType(file.FileName);
        if (mimeType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            using var image = System.Drawing.Image.FromFile(physicalPath);
            const int max = 150;
            var width = image.Width;
            var height = image.Height;
            if (width > height)
            {
                if (width > max)
                {
                    height = height * max / width;
                    width = max;
                }
            }
            else
            {
                if (height > max)
                {
                    width = width * max / height;
                    height = max;
                }
            }

            using var thumbnail = new System.Drawing.Bitmap(width, height);
            using (var g = System.Drawing.Graphics.FromImage(thumbnail))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(image, 0, 0, width, height);
            }

            using var ms = new MemoryStream();
            thumbnail.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            var bytes = ms.ToArray();
            var dto = new FileDto(file.FileName, MimeTypeNames.ImageJpeg)
            {
                Base64Data = $"data:{MimeTypeNames.ImageJpeg};base64,{Convert.ToBase64String(bytes)}"
            };

            return dto;
        }
        else if (mimeType.StartsWith("video/", StringComparison.OrdinalIgnoreCase))
        {
            var tempOutput = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.png");

            try
            {
                var success = await FFMpegCore.FFMpeg.SnapshotAsync(physicalPath, tempOutput, null, TimeSpan.FromSeconds(1));
                if (!success || !File.Exists(tempOutput))
                    throw new Exception("Snapshot creation failed.");

                using var image = System.Drawing.Image.FromFile(tempOutput);
                const int max = 150;
                var width = image.Width;
                var height = image.Height;

                if (width > height)
                {
                    if (width > max)
                    {
                        height = height * max / width;
                        width = max;
                    }
                }
                else
                {
                    if (height > max)
                    {
                        width = width * max / height;
                        height = max;
                    }
                }

                using var thumbnail = new System.Drawing.Bitmap(width, height);
                using (var g = System.Drawing.Graphics.FromImage(thumbnail))
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.DrawImage(image, 0, 0, width, height);
                }

                using var ms = new MemoryStream();
                thumbnail.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                var bytes = ms.ToArray();

                return new FileDto(file.FileName, MimeTypeNames.ImageJpeg)
                {
                    Base64Data = $"data:{MimeTypeNames.ImageJpeg};base64,{Convert.ToBase64String(bytes)}"
                };
            }
            finally
            {
                try
                {
                    if (File.Exists(tempOutput))
                        File.Delete(tempOutput);
                }
                catch { /* Ignore */ }
            }
        }

        return null;
    }

    public async Task<FileDto> GetReportFileData(Guid reportFileId)
    {
        var file = await _reportFileRepository.FirstOrDefaultAsync(reportFileId);
        if (file == null)
        {
            return null;
        }

        var physicalPath = Path.Combine(_appFolders.ReportFilesFolder, file.FilePath);
        if (!File.Exists(physicalPath))
        {
            return null;
        }

        var bytes = await File.ReadAllBytesAsync(physicalPath);
        var mimeType = _mimeTypeMap.GetMimeType(file.FileName);
        var dto = new FileDto(file.FileName, mimeType)
        {
            Base64Data = $"data:{mimeType};base64,{Convert.ToBase64String(bytes)}"
        };
        _tempFileCacheManager.SetFile(dto.FileToken, new TempFileInfo(file.FileName, mimeType, bytes));
        return dto;
    }

    public async Task<ReportFileCountsDto> GetReportFileCounts(Guid reportId)
    {
        var files = await _reportFileRepository.GetAll()
            .Where(f => f.ReportId == reportId)
            .ToListAsync();

        var counts = new ReportFileCountsDto();
        foreach (var file in files)
        {
            var mimeType = _mimeTypeMap.GetMimeType(file.FileName);
            if (mimeType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                counts.ImageCount++;
            }
            else if (mimeType.StartsWith("video/", StringComparison.OrdinalIgnoreCase))
            {
                counts.VideoCount++;
            }
            else if (mimeType.StartsWith("audio/", StringComparison.OrdinalIgnoreCase))
            {
                counts.VoiceCount++;
            }
            else
            {
                counts.DocumentCount++;
            }
        }

        //return result;
        return counts;
    }
}