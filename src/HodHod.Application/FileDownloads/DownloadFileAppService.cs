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
using FFMpegCore;
using Minio.Exceptions;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing;

namespace HodHod.FileDownloads;

[AbpAllowAnonymous]
public class DownloadFileAppService : HodHodAppServiceBase, IDownloadFileAppService
{
    private readonly IRepository<ReportFile, Guid> _reportFileRepository;
    private readonly ITempFileCacheManager _tempFileCacheManager;
    //private readonly IAppFolders _appFolders;
    private readonly IMimeTypeMap _mimeTypeMap;
    private readonly IMinioFileManager _minioFileManager;

    public DownloadFileAppService(
        IRepository<ReportFile, Guid> reportFileRepository,
        ITempFileCacheManager tempFileCacheManager,
        //IAppFolders appFolders,
        IMimeTypeMap mimeTypeMap,
        IMinioFileManager minioFileManager)
    {
        _reportFileRepository = reportFileRepository;
        _tempFileCacheManager = tempFileCacheManager;
        //_appFolders = appFolders;
        _mimeTypeMap = mimeTypeMap;
        _minioFileManager = minioFileManager;
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

        //var physicalPath = Path.Combine(_appFolders.ReportFilesFolder, file.FilePath);
        //if (!File.Exists(physicalPath))
        //var bytes = await _minioFileManager.DownloadAsync(file.FilePath);
        await using var stream = await _minioFileManager.DownloadStreamAsync(file.FilePath);
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms);
        var bytes = ms.ToArray();
        if (bytes == null || bytes.Length == 0)
        {
            return null;
        }

        //var bytes = await File.ReadAllBytesAsync(physicalPath);
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

        //var physicalPath = Path.Combine(_appFolders.ReportFilesFolder, file.FilePath);
        //if (!File.Exists(physicalPath))
        //var bytes = await _minioFileManager.DownloadAsync(file.FilePath);
        await using var stream = await _minioFileManager.DownloadStreamAsync(file.FilePath);
        using var msStream = new MemoryStream();
        await stream.CopyToAsync(msStream);
        var bytes = msStream.ToArray();
        if (bytes == null || bytes.Length == 0)
        {
            return null;
        }

        var mimeType = _mimeTypeMap.GetMimeType(file.FileName);
        if (mimeType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            //using var image = System.Drawing.Image.FromFile(physicalPath);
            using var image = System.Drawing.Image.FromStream(new MemoryStream(bytes));
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
            //var bytes = ms.ToArray();
            bytes = ms.ToArray();
            var dto = new FileDto(file.FileName, MimeTypeNames.ImageJpeg)
            {
                Base64Data = $"data:{MimeTypeNames.ImageJpeg};base64,{Convert.ToBase64String(bytes)}"
            };

            return dto;
        }
        else if (mimeType.StartsWith("video/", StringComparison.OrdinalIgnoreCase))
        {
            var tempInput = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}_{file.FileName}");
            await File.WriteAllBytesAsync(tempInput, bytes);
            var tempOutput = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.png");

            try
            {
                //var success = await FFMpegCore.FFMpeg.SnapshotAsync(physicalPath, tempOutput, null, TimeSpan.FromSeconds(1));
                var success = await FFMpegCore.FFMpeg.SnapshotAsync(tempInput, tempOutput, null, TimeSpan.FromSeconds(1));
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
                bytes = ms.ToArray();

                return new FileDto(file.FileName, MimeTypeNames.ImageJpeg)
                {
                    Base64Data = $"data:{MimeTypeNames.ImageJpeg};base64,{Convert.ToBase64String(bytes)}"
                };
            }
            finally
            {
                try
                {
                    if (File.Exists(tempInput))
                        File.Delete(tempInput);
                }
                catch { }
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

        //var physicalPath = Path.Combine(_appFolders.ReportFilesFolder, file.FilePath);
        //if (!File.Exists(physicalPath))
        //var bytes = await _minioFileManager.DownloadAsync(file.FilePath);
        await using var stream = await _minioFileManager.DownloadStreamAsync(file.FilePath);
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms);
        var bytes = ms.ToArray();
        if (bytes == null || bytes.Length == 0)
        {
            return null;
        }

        //var bytes = await File.ReadAllBytesAsync(physicalPath);
        var mimeType = _mimeTypeMap.GetMimeType(file.FileName);
        var dto = new FileDto(file.FileName, mimeType)
        {
            Base64Data = $"data:{mimeType};base64,{Convert.ToBase64String(bytes)}"
        };
        _tempFileCacheManager.SetFile(dto.FileToken, new TempFileInfo(file.FileName, mimeType, bytes));
        return dto;
    }

    public async Task<string> GetReportFilePresignedUrl(Guid reportFileId, int expirySeconds = 3600)
    {
        var file = await _reportFileRepository.FirstOrDefaultAsync(reportFileId);
        if (file == null)
        {
            return string.Empty;
        }

        return await _minioFileManager.GetPresignedGetUrlAsync(file.FilePath, expirySeconds);
    }



    public async Task<List<FileDto>> GetReportFilesData(Guid reportId)
    {
        var files = await _reportFileRepository.GetAll()
            .Where(f => f.ReportId == reportId)
            .ToListAsync();

        var result = new List<FileDto>();
        //foreach (var file in files)
        var tasks = files.Select(async file =>
        {
            //var bytes = await _minioFileManager.DownloadAsync(file.FilePath);
            await using var stream = await _minioFileManager.DownloadStreamAsync(file.FilePath);
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            var bytes = ms.ToArray();
            if (bytes == null || bytes.Length == 0)
            {
                //continue;
                return null;
            }
            var mimeType = _mimeTypeMap.GetMimeType(file.FileName);
            var dto = new FileDto(file.FileName, mimeType)
            {
                Base64Data = $"data:{mimeType};base64,{Convert.ToBase64String(bytes)}",
                Id = file.Id
            };
            _tempFileCacheManager.SetFile(dto.FileToken, new TempFileInfo(file.FileName, mimeType, bytes));
            //    result.Add(dto);
            //}
            return dto;
        });

        var downloaded = await Task.WhenAll(tasks);
        result.AddRange(downloaded.Where(d => d != null));

        return result;
    }

    public async Task<List<ReportFilePreviewDto>> GetReportFilePreviews(Guid reportId)
    {
        var files = await _reportFileRepository.GetAll()
            .Where(f => f.ReportId == reportId)
            .ToListAsync();

        var result = new List<ReportFilePreviewDto>();

        foreach (var file in files)
        {
            var mimeType = _mimeTypeMap.GetMimeType(file.FileName);
            string fileType;
            if (mimeType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                fileType = "image";
            }
            else if (mimeType.StartsWith("video/", StringComparison.OrdinalIgnoreCase))
            {
                fileType = "video";
            }
            else if (mimeType.StartsWith("audio/", StringComparison.OrdinalIgnoreCase))
            {
                fileType = "audio";
            }
            else
            {
                fileType = "document";
            }

            var originalUrl = await _minioFileManager.GetPresignedGetUrlAsync(file.FilePath, 3600);
            string previewUrl = null;

            if (fileType == "image" || fileType == "video")
            {
                var previewObject = fileType == "image"
                    ? $"thumbnails/{file.ReportId}/{Path.ChangeExtension(file.FileName, "jpg")}" :
                    $"previews/{file.ReportId}/{Path.ChangeExtension(file.FileName, "jpg")}";

                var exists = true;
                try
                {
                    await _minioFileManager.ProcessStreamAsync(previewObject, _ => Task.CompletedTask, 0, 1);
                }
                catch (MinioException)
                {
                    exists = false;
                }

                if (!exists)
                {
                    if (fileType == "image")
                    {
                        await using var stream = await _minioFileManager.DownloadStreamAsync(file.FilePath);
                        using var image = Image.FromStream(stream);
                        const int max = 200;
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

                        using var thumbnail = new Bitmap(width, height);
                        using (var g = System.Drawing.Graphics.FromImage(thumbnail))
                        {
                            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            g.DrawImage(image, 0, 0, width, height);
                        }

                        using var ms = new MemoryStream();
                        thumbnail.Save(ms, ImageFormat.Jpeg);
                        ms.Position = 0;
                        await _minioFileManager.UploadStreamAsync(previewObject, ms, MimeTypeNames.ImageJpeg);
                    }
                    else if (fileType == "video")
                    {
                        var tempInput = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}_{file.FileName}");
                        await using (var original = await _minioFileManager.DownloadStreamAsync(file.FilePath))
                        await using (var fs = File.Create(tempInput))
                        {
                            await original.CopyToAsync(fs);
                        }

                        var tempOutput = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.png");
                        try
                        {
                            var success = await FFMpeg.SnapshotAsync(tempInput, tempOutput, null, TimeSpan.FromSeconds(1));
                            if (!success || !File.Exists(tempOutput))
                                throw new Exception("Snapshot creation failed.");

                            using var image = Image.FromFile(tempOutput);
                            const int max = 200;
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

                            using var thumbnail = new Bitmap(width, height);
                            using (var g = System.Drawing.Graphics.FromImage(thumbnail))
                            {
                                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                                g.DrawImage(image, 0, 0, width, height);
                            }

                            using var ms = new MemoryStream();
                            thumbnail.Save(ms, ImageFormat.Jpeg);
                            ms.Position = 0;
                            await _minioFileManager.UploadStreamAsync(previewObject, ms, MimeTypeNames.ImageJpeg);
                        }
                        finally
                        {
                            try
                            {
                                if (File.Exists(tempInput)) File.Delete(tempInput);
                            }
                            catch { }
                            try
                            {
                                if (File.Exists(tempOutput)) File.Delete(tempOutput);
                            }
                            catch { }
                        }
                    }
                }

                previewUrl = await _minioFileManager.GetPresignedGetUrlAsync(previewObject, 3600);
            }

            result.Add(new ReportFilePreviewDto
            {
                FileName = file.FileName,
                FileType = fileType,
                OriginalUrl = originalUrl,
                PreviewUrl = previewUrl
            });
        }

        return result;
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