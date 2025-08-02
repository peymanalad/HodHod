using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Entities;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.Runtime.Caching;
using Abp.Timing;
using Abp.UI;
using HodHod.Authorization.PasswordlessLogin;
using HodHod.Authorization.Roles;
using HodHod.Categories;
using HodHod.Geo;
using HodHod.Net.Sms;
using HodHod.Reports.Dto;
using HodHod.Storage;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;


using Task = System.Threading.Tasks.Task;
using HodHod.BlackLists;
using HodHod.Reports.Exporting;
using Microsoft.AspNetCore.Http;
using HodHod.Dto;
using Microsoft.AspNetCore.Mvc;

namespace HodHod.Reports;

[AbpAllowAnonymous]
public class ReportAppService : HodHodAppServiceBase, IReportAppService
{
    private readonly IRepository<Report, Guid> _reportRepository;
    private readonly IRepository<ReportFile, Guid> _reportFileRepository;
    private readonly IRepository<Category, int> _categoryRepository;
    private readonly IRepository<SubCategory, int> _subCategoryRepository;
    private readonly IRepository<PhoneReportLimit, int> _phoneReportLimitRepository;
    private readonly IRepository<Province, int> _provinceRepository;
    private readonly IRepository<City, int> _cityRepository;
    private readonly IRepository<ReportStar, Guid> _reportStarRepository;
    private readonly IRepository<BlackListEntry, int> _blackListRepository;
    //private readonly IBinaryObjectManager _binaryObjectManager;
    private readonly ITempFileCacheManager _tempFileCacheManager;
    private readonly IPasswordlessLoginManager _passwordlessLoginManager;
    //private readonly IAppFolders _appFolders;
    private readonly IMinioFileManager _minioFileManager;
    private readonly ISmsSender _smsSender;
    private readonly ICacheManager _cacheManager;
    private readonly ILocationAppService _locationAppService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IReportListExcelExporter _reportListExcelExporter;

    public ReportAppService(
        IRepository<Report, Guid> reportRepository,
        IRepository<ReportFile, Guid> reportFileRepository,
        //IBinaryObjectManager binaryObjectManager,
        ITempFileCacheManager tempFileCacheManager,
        //IPasswordlessLoginManager passwordlessLoginManager)
        IPasswordlessLoginManager passwordlessLoginManager,
        //IAppFolders appFolders,
        ISmsSender smsSender,
        ICacheManager cacheManager,
        IRepository<Category, int> categoryRepository,
        IRepository<SubCategory, int> subCategoryRepository,
        IRepository<PhoneReportLimit, int> phoneReportLimitRepository,
        IRepository<Province, int> provinceRepository,
        IRepository<City, int> cityRepository,
        ILocationAppService locationAppService,
        IRepository<ReportStar, Guid> reportStarRepository,
        IRepository<BlackListEntry, int> blackListRepository,
        IHttpContextAccessor httpContextAccessor,
        IMinioFileManager minioFileManager,
        IReportListExcelExporter reportListExcelExporter)
    {
        _reportRepository = reportRepository;
        _reportFileRepository = reportFileRepository;
        //_binaryObjectManager = binaryObjectManager;
        _tempFileCacheManager = tempFileCacheManager;
        _passwordlessLoginManager = passwordlessLoginManager;
        //_appFolders = appFolders;
        _smsSender = smsSender;
        _cacheManager = cacheManager;
        _categoryRepository = categoryRepository;
        _subCategoryRepository = subCategoryRepository;
        _phoneReportLimitRepository = phoneReportLimitRepository;
        _provinceRepository = provinceRepository;
        _cityRepository = cityRepository;
        _locationAppService = locationAppService;
        _reportStarRepository = reportStarRepository;
        _blackListRepository = blackListRepository;
        _httpContextAccessor = httpContextAccessor;
        _minioFileManager = minioFileManager;
        _reportListExcelExporter = reportListExcelExporter;
    }
    public async Task SendReportOtpAsync(SendReportOtpInput input)
    {
        await CheckBlackListFromHeaderAsync();
        await EnsureNotBlacklistedAsync(input.PhoneNumber);
        if (String.IsNullOrEmpty(input.PhoneNumber))
        {
            return;
        }

        var normalized = PhoneNumberHelper.Normalize(input.PhoneNumber);
        var phoneDigits = long.Parse(normalized);
        var limitEntity = await _phoneReportLimitRepository.FirstOrDefaultAsync(l => l.PhoneNumber == phoneDigits);
        var maxPerHour = limitEntity?.MaxReportsPerHour ?? PhoneReportLimitDefaults.MaxReportsPerHour;

        var limiterCache = _cacheManager.GetCache<string, OtpSendLimitCacheItem>(OtpSendLimitCacheItem.CacheName);
        var limiter = await limiterCache.GetOrDefaultAsync(input.PhoneNumber);
        var now = Clock.Now;
        if (limiter != null && limiter.WindowStart.AddHours(1) > now && limiter.Count >= maxPerHour)
        {
            throw new UserFriendlyException(L("تعداد دفعات ارسال کد زیاد شده است. لطفا\u064b کمی صبر کرده و دوباره تلاش کنید."));
        }

        if (limiter == null || limiter.WindowStart.AddHours(1) <= now)
        {
            limiter = new OtpSendLimitCacheItem(now, 0);
        }

        limiter.Count++;
        await limiterCache.SetAsync(input.PhoneNumber, limiter, absoluteExpireTime: new DateTimeOffset(limiter.WindowStart.AddHours(1)));

        var code = await _passwordlessLoginManager.GeneratePasswordlessLoginCode(
            AbpSession.TenantId,
            input.PhoneNumber);

        var message = string.Format(L("PasswordlessLogin_SmsMessage", HodHodConsts.ProductName, code));
        await _smsSender.SendAsync(input.PhoneNumber, message);

        // Do not actually send the SMS in this stage. The OTP is generated and
        // stored in cache for later verification but no SMS is dispatched.
        // var message = string.Format(L("PasswordlessLogin_SmsMessage", HodHodConsts.ProductName, code));
        // await _smsSender.SendAsync(input.PhoneNumber, message);
    }

    public async Task SubmitReport(CreateReportDto input)
    {
        await CheckBlackListFromHeaderAsync();
        await EnsureNotBlacklistedAsync(input.PhoneNumber);

        var normalized = PhoneNumberHelper.Normalize(input.PhoneNumber);
        await _passwordlessLoginManager.VerifyPasswordlessLoginCode(
            AbpSession.TenantId,
            input.PhoneNumber,
            input.OtpCode);

        var phoneDigits = long.Parse(normalized);
        var limit = await _phoneReportLimitRepository.FirstOrDefaultAsync(l => l.PhoneNumber == phoneDigits);

        var maxFileCount = limit?.MaxFileCount ?? PhoneReportLimitDefaults.MaxFileCount;
        var maxFileSize = limit?.MaxFileSizeInBytes ?? PhoneReportLimitDefaults.MaxFileSizeInBytes;
        var maxReportsPerHour = limit?.MaxReportsPerHour ?? PhoneReportLimitDefaults.MaxReportsPerHour;

        var recentCount = await _reportRepository.CountAsync(r => r.PhoneNumber == phoneDigits && r.CreationTime > Clock.Now.AddHours(-1));
        if (recentCount >= maxReportsPerHour)
            throw new UserFriendlyException("Report limit reached");

        if (input.FileTokens != null && input.FileTokens.Count > maxFileCount)
            throw new UserFriendlyException("Too many files");

        var category = await _categoryRepository.FirstOrDefaultAsync(c => c.PublicId == input.CategoryId)
            ?? throw new EntityNotFoundException("Category not found");

        var subCategory = await _subCategoryRepository.FirstOrDefaultAsync(s => s.PublicId == input.SubCategoryId)
            ?? throw new EntityNotFoundException("SubCategory not found");

        var loc = await _locationAppService.ReverseGeocodeAsync(input.Latitude, input.Longitude);

        var report = new Report
        {
            CategoryId = category.Id,
            SubCategoryId = subCategory.Id,
            UniqueId = $"HD-{Guid.NewGuid():N}".Substring(0, 8),
            Description = input.Description,
            Address = input.Address,
            Longitude = input.Longitude,
            Latitude = input.Latitude,
            PhoneNumber = phoneDigits,
            Province = loc?.Province,
            City = loc?.City,
            PersianCreationTime = PersianDateTimeHelper.ToCompactPersianNumber(Clock.Now),
            Status = ReportStatus.Unreviewed
        };

        await _reportRepository.InsertAsync(report);
        await CurrentUnitOfWork.SaveChangesAsync();

        if (input.FileTokens != null && input.FileTokens.Count > 0)
        {
            var uploads = new List<(string Name, Stream Stream, string ContentType)>();

            foreach (var token in input.FileTokens)
            {
                var info = _tempFileCacheManager.GetFileInfo(token);
                if (info == null)
                    continue;

                if (info.File.Length > maxFileSize)
                    throw new UserFriendlyException("File too large");

                var ext = Path.GetExtension(info.FileName);
                var uniqueName = Guid.NewGuid().ToString("N") + ext;
                var mime = info.FileType ?? "application/octet-stream";

                if (ext == ".webm")
                {
                    mime = "audio/webm";
                }

                uploads.Add((uniqueName, new MemoryStream(info.File), mime));

                _tempFileCacheManager.ClearFile(token);

                await _reportFileRepository.InsertAsync(new ReportFile
                {
                    ReportId = report.Id,
                    FileName = info.FileName,
                    FilePath = uniqueName
                });
            }

            if (uploads.Count > 0)
            {
                await _minioFileManager.UploadManyAsync(uploads);
            }
        }
    }

    [AbpAuthorize]
    public async Task<PagedResultDto<ReportDto>> GetReportsForAdminAsync(GetReportsInput input)
    {
        var user = await GetCurrentUserAsync();
        var roles = await UserManager.GetRolesAsync(user);

        if (!roles.Contains(StaticRoleNames.Host.Admin) &&
            !roles.Contains(StaticRoleNames.Host.SuperAdmin) &&
            !roles.Contains(StaticRoleNames.Host.ProvinceAdmin) &&
            !roles.Contains(StaticRoleNames.Host.CityAdmin))
        {
            throw new AbpAuthorizationException("Not authorized to view reports.");
        }

        input.Normalize();

        //var query = _reportRepository.GetAllIncluding(r => r.Files, r => r.Category, r => r.SubCategory);
        var query = _reportRepository.GetAllIncluding(r => r.Files, r => r.Category, r => r.SubCategory, r => r.Notes);
        if (roles.Contains(StaticRoleNames.Host.CityAdmin))
        {
            if (!string.IsNullOrEmpty(user.City))
            {
                query = query.Where(r => r.City == user.City);
            }
        }
        else if (roles.Contains(StaticRoleNames.Host.ProvinceAdmin))
        {
            if (!string.IsNullOrEmpty(user.Province))
            {
                query = query.Where(r => r.Province == user.Province);
            }
        }

        if (input.CategoryId.HasValue)
        {
            query = query.Where(r => r.Category.PublicId == input.CategoryId.Value);
        }

        if (input.SubCategoryId.HasValue)
        {
            query = query.Where(r => r.SubCategory.PublicId == input.SubCategoryId.Value);
        }

        if (input.StartDate.HasValue)
        {
            query = query.Where(r => r.CreationTime >= input.StartDate.Value);
        }

        if (input.EndDate.HasValue)
        {
            query = query.Where(r => r.CreationTime <= input.EndDate.Value);
        }

        if (!string.IsNullOrEmpty(input.Province))
        {
            query = query.Where(r => r.Province == input.Province);
        }

        if (!string.IsNullOrEmpty(input.City))
        {
            query = query.Where(r => r.City == input.City);
        }

        if (!string.IsNullOrWhiteSpace(input.PhoneNumber))
        {
            var normalized = PhoneNumberHelper.Normalize(input.PhoneNumber);
            if (!string.IsNullOrWhiteSpace(normalized))
            {
                var digits = long.Parse(normalized);
                query = query.Where(r => r.PhoneNumber == digits);
            }
        }


        if (!string.IsNullOrWhiteSpace(input.UniqueId))
        {
            query = query.Where(r => r.UniqueId.Contains(input.UniqueId));
        }

        if (input.Status.HasValue)
        {
            query = query.Where(r => r.Status == input.Status.Value);
        }

        if (input.IsArchived.HasValue)
        {
            query = query.Where(r => r.IsArchived == input.IsArchived.Value);
        }

        if (input.StartPersianCreationTime.HasValue)
        {
            query = query.Where(r => r.PersianCreationTime >= input.StartPersianCreationTime.Value);
        }

        if (input.EndPersianCreationTime.HasValue)
        {
            query = query.Where(r => r.PersianCreationTime <= input.EndPersianCreationTime.Value);
        }

        if (input.StartPersianCreationClock.HasValue)
        {
            var startClock = input.StartPersianCreationClock.Value;
            query = query.Where(r => (r.PersianCreationTime % 1000000) >= startClock);
        }

        if (input.EndPersianCreationClock.HasValue)
        {
            var endClock = input.EndPersianCreationClock.Value;
            query = query.Where(r => (r.PersianCreationTime % 1000000) <= endClock);
        }

        if (input.FileCategory.HasValue)
        {
            var imageExts = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".svg", ".tiff" };
            var videoExts = new[] { ".mp4", ".avi", ".mov", ".wmv", ".flv", ".mkv", ".3gp" };
            var audioExts = new[] { ".webm", ".mp3", ".wav", ".ogg", ".m4a", ".flac", ".wma" };
            var docExts = new[] { ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".txt", ".odt", ".ods", ".odp" };

            switch (input.FileCategory.Value)
            {
                case ReportFileCategory.Image:
                    query = query.Where(r => r.Files.Any(f => imageExts.Any(e => f.FileName.ToLower().EndsWith(e))));
                    break;
                case ReportFileCategory.Video:
                    query = query.Where(r => r.Files.Any(f => videoExts.Any(e => f.FileName.ToLower().EndsWith(e))));
                    break;
                case ReportFileCategory.Audio:
                    query = query.Where(r => r.Files.Any(f => audioExts.Any(e => f.FileName.ToLower().EndsWith(e))));
                    break;
                case ReportFileCategory.Document:
                    query = query.Where(r => r.Files.Any(f => docExts.Any(e => f.FileName.ToLower().EndsWith(e))));
                    break;
            }
        }
        var countImageExts = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".svg", ".tiff" };
        var countVideoExts = new[] { ".mp4", ".avi", ".mov", ".wmv", ".flv", ".mkv", ".3gp" };
        var countAudioExts = new[] { ".webm", ".mp3", ".wav", ".ogg", ".m4a", ".flac", ".wma" };
        var countDocExts = new[] { ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".txt", ".odt", ".ods", ".odp" };


        var starredIdsQuery = _reportStarRepository.GetAll()
            .Where(s => s.UserId == user.Id)
            .Select(s => s.ReportId);
        List<Guid> starredIds = null;
        if (input.OnlyStarredByCurrentUser.HasValue)
        {
            starredIds = await starredIdsQuery.ToListAsync();
            //query = query.Where(r => starredIds.Contains(r.Id));
            if (input.OnlyStarredByCurrentUser.Value)
            {
                query = query.Where(r => starredIds.Contains(r.Id));
            }
            else
            {
                query = query.Where(r => !starredIds.Contains(r.Id));
            }
        }

        if (input.HasNotes.HasValue)
        {
            if (input.HasNotes.Value)
            {
                query = query.Where(r => r.Notes.Any());
            }
            else
            {
                query = query.Where(r => !r.Notes.Any());
            }
        }


        var totalCount = await query.CountAsync();


        var reports = await query
            .OrderBy(input.Sorting)
            .PageBy<Report>(input)
            .ToListAsync();

        var dto = ObjectMapper.Map<List<ReportDto>>(reports);

        starredIds ??= await starredIdsQuery.ToListAsync();
        for (int i = 0; i < dto.Count; i++)
        {
            var d = dto[i];
            var r = reports[i];
            d.IsStarredByCurrentUser = starredIds.Contains(d.Id);
            var counts = new ReportFileCountsDto();
            foreach (var f in r.Files)
            {
                var ext = Path.GetExtension(f.FileName).ToLowerInvariant();
                if (countImageExts.Any(e => ext.EndsWith(e)))
                {
                    counts.ImageCount++;
                }
                else if (countVideoExts.Any(e => ext.EndsWith(e)))
                {
                    counts.VideoCount++;
                }
                else if (countAudioExts.Any(e => ext.EndsWith(e)))
                {
                    counts.VoiceCount++;
                }
                else if (countDocExts.Any(e => ext.EndsWith(e)))
                {
                    counts.DocumentCount++;
                }
            }
            d.FileCounts = counts;
        }
        return new PagedResultDto<ReportDto>(totalCount, dto);
    }

    [AbpAuthorize]
    public async Task<PagedResultDto<ReportWithLastNoteDto>> GetReportsWithLastNoteForAdminAsync(GetReportsInput input)
    {
        input.HasNotes = true;
        input.IsArchived = false;

        var baseResult = await GetReportsForAdminAsync(input);
        if (!baseResult.Items.Any())
        {
            return new PagedResultDto<ReportWithLastNoteDto>(baseResult.TotalCount, new List<ReportWithLastNoteDto>());
        }

        var reportIds = baseResult.Items.Select(r => r.Id).ToList();

        var reports = await _reportRepository.GetAll()
            .Where(r => reportIds.Contains(r.Id))
            .Include(r => r.Notes)
            .ToListAsync();

        var reporterIds = reports
            .Where(r => r.CreatorUserId.HasValue)
            .Select(r => r.CreatorUserId.Value)
            .Distinct()
            .ToList();

        var reporterNames = await UserManager.Users
            .Where(u => reporterIds.Contains(u.Id))
            .Select(u => new { u.Id, u.Name, u.Surname })
            .ToListAsync();

        var reporterDict = reporterNames.ToDictionary(u => u.Id, u => $"{u.Name} {u.Surname}");

        var reportDict = reports.ToDictionary(r => r.Id);

        var dtoList = new List<ReportWithLastNoteDto>();
        foreach (var r in baseResult.Items)
        {
            var entity = reportDict[r.Id];
            var lastNote = entity.Notes.OrderByDescending(n => n.CreationTime).FirstOrDefault();

            var reporterName = entity.CreatorUserId.HasValue && reporterDict.TryGetValue(entity.CreatorUserId.Value, out var name)
                ? name
                : null;

            dtoList.Add(new ReportWithLastNoteDto
            {
                Id = r.Id,
                UniqueId = r.UniqueId,
                CategoryId = r.CategoryId,
                CategoryName = r.CategoryName,
                SubCategoryId = r.SubCategoryId,
                SubCategoryName = r.SubCategoryName,
                LastNoteText = lastNote?.Text,
                LastNoteCreationTime = lastNote?.CreationTime,
                LastNoteAuthorId = lastNote?.CreatorUserId,
                ReporterFullName = reporterName,
                NoteCount = r.NoteCount,
                Status = r.Status
            });
        }

        return new PagedResultDto<ReportWithLastNoteDto>(baseResult.TotalCount, dtoList);
    }


    [AbpAuthorize]
    public async Task<List<ReportMapPointDto>> GetReportMapPointsForAdminAsync(GetReportMapPointsInput input)
    {
        var user = await GetCurrentUserAsync();
        var roles = await UserManager.GetRolesAsync(user);

        if (!roles.Contains(StaticRoleNames.Host.SuperAdmin) &&
            !roles.Contains(StaticRoleNames.Host.ProvinceAdmin) &&
            !roles.Contains(StaticRoleNames.Host.CityAdmin))
        {
            throw new AbpAuthorizationException("Not authorized to view reports.");
        }

        var query = _reportRepository.GetAll().AsNoTracking();

        if (roles.Contains(StaticRoleNames.Host.CityAdmin))
        {
            if (!string.IsNullOrEmpty(user.City))
            {
                query = query.Where(r => r.City == user.City);
            }
        }
        else if (roles.Contains(StaticRoleNames.Host.ProvinceAdmin))
        {
            if (!string.IsNullOrEmpty(user.Province))
            {
                query = query.Where(r => r.Province == user.Province);
            }
        }

        if (input.CategoryId.HasValue)
        {
            query = query.Where(r => r.Category.PublicId == input.CategoryId.Value);
        }

        if (input.SubCategoryId.HasValue)
        {
            query = query.Where(r => r.SubCategory.PublicId == input.SubCategoryId.Value);
        }

        if (input.StartDate.HasValue)
        {
            query = query.Where(r => r.CreationTime >= input.StartDate.Value);
        }

        if (input.EndDate.HasValue)
        {
            query = query.Where(r => r.CreationTime <= input.EndDate.Value);
        }

        if (!string.IsNullOrEmpty(input.Province))
        {
            query = query.Where(r => r.Province == input.Province);
        }

        if (!string.IsNullOrEmpty(input.City))
        {
            query = query.Where(r => r.City == input.City);
        }

        if (input.StartPersianCreationClock.HasValue)
        {
            var startClock = input.StartPersianCreationClock.Value;
            query = query.Where(r => (r.PersianCreationTime % 1000000) >= startClock);
        }

        if (input.EndPersianCreationClock.HasValue)
        {
            var endClock = input.EndPersianCreationClock.Value;
            query = query.Where(r => (r.PersianCreationTime % 1000000) <= endClock);
        }

        query = query.Where(r => r.Longitude.HasValue && r.Latitude.HasValue);

        return await query
            .Select(r => new ReportMapPointDto
            {
                UniqueId = r.Id,
                CategoryId = r.Category.PublicId,
                SubCategoryId = r.SubCategory.PublicId,
                CategoryName = r.Category.Name,
                SubCategoryName = r.SubCategory.Name,
                Longitude = r.Longitude.Value,
                Latitude = r.Latitude.Value
            })
            .ToListAsync();
    }


    [AbpAuthorize]
    public async Task ChangeReportStatus(ChangeReportStatusDto input)
    {
        var user = await GetCurrentUserAsync();
        await EnsureReportAccessAsync(input.Id, user);

        var report = await _reportRepository.FirstOrDefaultAsync(input.Id);
        if (report == null)
        {
            throw new EntityNotFoundException("Report not found");
        }

        report.Status = input.Status;
        await _reportRepository.UpdateAsync(report);
    }
    [AbpAuthorize]
    public async Task ChangeReportCategoryAsync(ChangeReportCategoryDto input)
    {
        var user = await GetCurrentUserAsync();
        await EnsureReportAccessAsync(input.Id, user);

        var report = await _reportRepository.FirstOrDefaultAsync(input.Id);
        if (report == null)
        {
            throw new EntityNotFoundException("Report not found");
        }

        var category = await _categoryRepository.FirstOrDefaultAsync(c => c.PublicId == input.CategoryId)
                       ?? throw new EntityNotFoundException("Category not found");

        var subCategory = await _subCategoryRepository.FirstOrDefaultAsync(s => s.PublicId == input.SubCategoryId)
                          ?? throw new EntityNotFoundException("SubCategory not found");

        if (subCategory.CategoryId != category.Id)
        {
            throw new UserFriendlyException("SubCategory does not belong to specified Category");
        }

        report.CategoryId = category.Id;
        report.SubCategoryId = subCategory.Id;
        await _reportRepository.UpdateAsync(report);
    }

    [AbpAuthorize]
    public async Task ArchiveReport(EntityDto<Guid> input)
    {
        var user = await GetCurrentUserAsync();
        var roles = await UserManager.GetRolesAsync(user);
        if (!roles.Contains(StaticRoleNames.Host.SuperAdmin))
        {
            throw new AbpAuthorizationException("Only super admin can archive reports.");
        }

        var report = await _reportRepository.FirstOrDefaultAsync(input.Id);
        if (report == null)
        {
            throw new EntityNotFoundException("Report not found");
        }

        report.IsArchived = true;
        report.ArchiveTime = Clock.Now;
        await _reportRepository.UpdateAsync(report);
    }

    [AbpAuthorize]
    public async Task RestoreReport(EntityDto<Guid> input)
    {
        var user = await GetCurrentUserAsync();
        var roles = await UserManager.GetRolesAsync(user);
        if (!roles.Contains(StaticRoleNames.Host.SuperAdmin))
        {
            throw new AbpAuthorizationException("Only super admin can restore reports.");
        }

        var report = await _reportRepository.FirstOrDefaultAsync(input.Id);
        if (report == null)
        {
            throw new EntityNotFoundException("Report not found");
        }

        report.IsArchived = false;
        report.ArchiveTime = null;
        await _reportRepository.UpdateAsync(report);
    }

    [AbpAuthorize]
    public async Task RestoreReports(List<Guid> reportIds)
    {
        var user = await GetCurrentUserAsync();
        var roles = await UserManager.GetRolesAsync(user);
        if (!roles.Contains(StaticRoleNames.Host.SuperAdmin))
        {
            throw new AbpAuthorizationException("Only super admin can restore reports.");
        }

        var reports = await _reportRepository.GetAll()
            .Where(r => reportIds.Contains(r.Id) && r.IsArchived)
            .ToListAsync();

        foreach (var report in reports)
        {
            report.IsArchived = false;
            report.ArchiveTime = null;
        }

        await CurrentUnitOfWork.SaveChangesAsync();
    }

    [AbpAuthorize]
    public async Task DeleteReports(List<Guid> reportIds)
    {
        var user = await GetCurrentUserAsync();
        var roles = await UserManager.GetRolesAsync(user);
        if (!roles.Contains(StaticRoleNames.Host.SuperAdmin))
        {
            throw new AbpAuthorizationException("Only super admin can delete reports.");
        }

        var reports = await _reportRepository.GetAll()
            .Where(r => reportIds.Contains(r.Id) && r.IsArchived)
            .ToListAsync();

        foreach (var report in reports)
        {
            report.IsDeleted = true;
            report.DeletionTime = Clock.Now;
            report.DeleterUserId = user.Id;
        }

        await CurrentUnitOfWork.SaveChangesAsync();
    }

    [AbpAuthorize]
    public async Task StarReport(EntityDto<Guid> input)
    {
        var user = await GetCurrentUserAsync();
        await EnsureReportAccessAsync(input.Id, user);

        var exists = await _reportStarRepository.FirstOrDefaultAsync(s => s.ReportId == input.Id && s.UserId == user.Id);
        if (exists == null)
        {
            await _reportStarRepository.InsertAsync(new ReportStar { ReportId = input.Id, UserId = user.Id });
        }
    }

    [AbpAuthorize]
    public async Task UnstarReport(EntityDto<Guid> input)
    {
        var user = await GetCurrentUserAsync();
        await EnsureReportAccessAsync(input.Id, user);

        var star = await _reportStarRepository.FirstOrDefaultAsync(s => s.ReportId == input.Id && s.UserId == user.Id);
        if (star != null)
        {
            await _reportStarRepository.DeleteAsync(star);
        }
    }

    [AbpAuthorize]
    public async Task<List<ProvinceReportPercentageDto>> GetReportDistributionByProvinceAsync()
    {
        var user = await GetCurrentUserAsync();
        var roles = await UserManager.GetRolesAsync(user);

        IQueryable<Report> query = _reportRepository.GetAll();

        if (roles.Contains(StaticRoleNames.Host.CityAdmin))
        {
            if (!string.IsNullOrEmpty(user.City))
            {
                query = query.Where(r => r.City == user.City);
            }
            else
            {
                return new List<ProvinceReportPercentageDto>();
            }
        }
        else if (roles.Contains(StaticRoleNames.Host.ProvinceAdmin))
        {
            if (!string.IsNullOrEmpty(user.Province))
            {
                query = query.Where(r => r.Province == user.Province);
            }
            else
            {
                return new List<ProvinceReportPercentageDto>();
            }
        }
        else if (!(roles.Contains(StaticRoleNames.Host.SuperAdmin) || roles.Contains(StaticRoleNames.Host.Admin)))
        {
            throw new AbpAuthorizationException("Not authorized to view reports.");
        }

        var totalCount = await query.CountAsync();
        if (totalCount == 0)
        {
            return new List<ProvinceReportPercentageDto>();
        }

        var data = await query
            .GroupBy(r => r.Province)
            .Select(g => new { Province = g.Key, Count = g.Count() })
            .ToListAsync();

        return data
            .Select(d => new ProvinceReportPercentageDto
            {
                Province = d.Province,
                Percentage = (double)d.Count * 100 / totalCount,
                PercentageFormatted = FormatPercentage((double)d.Count * 100 / totalCount)
            })
            .ToList();
    }



    public async Task<List<ProvinceCityReportPercentageDto>> GetReportDistributionByProvinceAndCityAsync()
    {
        var user = await GetCurrentUserAsync();
        var roles = await UserManager.GetRolesAsync(user);

        if (roles.Contains(StaticRoleNames.Host.CityAdmin))
        {
            throw new AbpAuthorizationException("Not authorized to view reports.");
        }

        if (!(roles.Contains(StaticRoleNames.Host.SuperAdmin) || roles.Contains(StaticRoleNames.Host.ProvinceAdmin)))
        {
            throw new AbpAuthorizationException("Not authorized to view reports.");
        }

        var query = _reportRepository.GetAll();

        var totalCount = await query.CountAsync();
        if (totalCount == 0)
        {
            return new List<ProvinceCityReportPercentageDto>();
        }

        var provinceData = await query
            .GroupBy(r => r.Province)
            .Select(g => new { Province = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToListAsync();

        var result = new List<ProvinceCityReportPercentageDto>();

        foreach (var p in provinceData)
        {
            var provinceDto = new ProvinceCityReportPercentageDto
            {
                Province = p.Province,
                TotalReports = p.Count,
                Percentage = (double)p.Count * 100 / totalCount,
                PercentageFormatted = FormatPercentage((double)p.Count * 100 / totalCount),
                Cities = new List<CityReportPercentageDto>()
            };

            var canSeeCities =
                roles.Contains(StaticRoleNames.Host.SuperAdmin) ||
                (roles.Contains(StaticRoleNames.Host.ProvinceAdmin) &&
                 !string.IsNullOrEmpty(user.Province) &&
                 user.Province == p.Province);

            if (canSeeCities)
            {
                var cityData = await query
                    .Where(r => r.Province == p.Province)
                    .GroupBy(r => r.City)
                    .Select(g => new { City = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .ToListAsync();

                foreach (var c in cityData)
                {
                    provinceDto.Cities.Add(new CityReportPercentageDto
                    {
                        City = c.City,
                        TotalReports = c.Count,
                        Percentage = (double)c.Count * 100 / p.Count,
                        PercentageFormatted = FormatPercentage((double)c.Count * 100 / p.Count)
                    });
                }
            }

            result.Add(provinceDto);
        }

        return result;
    }

    [AbpAuthorize]
    public async Task<List<FileCategoryReportPercentageDto>> GetReportDistributionByFileCategoryAsync()
    {
        var user = await GetCurrentUserAsync();
        var roles = await UserManager.GetRolesAsync(user);

        IQueryable<ReportFile> query = _reportFileRepository.GetAllIncluding(f => f.Report);

        if (roles.Contains(StaticRoleNames.Host.CityAdmin))
        {
            if (!string.IsNullOrEmpty(user.City))
            {
                query = query.Where(f => f.Report.City == user.City);
            }
            else
            {
                return new List<FileCategoryReportPercentageDto>();
            }
        }
        else if (roles.Contains(StaticRoleNames.Host.ProvinceAdmin))
        {
            if (!string.IsNullOrEmpty(user.Province))
            {
                query = query.Where(f => f.Report.Province == user.Province);
            }
            else
            {
                return new List<FileCategoryReportPercentageDto>();
            }
        }
        else if (!(roles.Contains(StaticRoleNames.Host.SuperAdmin) || roles.Contains(StaticRoleNames.Host.Admin)))
        {
            throw new AbpAuthorizationException("Not authorized to view reports.");
        }

        var totalCount = await query.CountAsync();
        if (totalCount == 0)
        {
            return new List<FileCategoryReportPercentageDto>();
        }

        var imageExts = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".svg", ".tiff" };
        var videoExts = new[] { ".mp4", ".avi", ".mov", ".wmv", ".flv", ".mkv", ".3gp" };
        var audioExts = new[] { ".webm", ".mp3", ".wav", ".ogg", ".m4a", ".flac", ".wma" };
        var docExts = new[] { ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".txt", ".odt", ".ods", ".odp" };

        var fileNames = await query.Select(f => f.FileName.ToLower()).ToListAsync();

        int imageCount = 0, videoCount = 0, audioCount = 0, docCount = 0;
        foreach (var name in fileNames)
        {
            if (imageExts.Any(e => name.EndsWith(e)))
            {
                imageCount++;
            }
            else if (videoExts.Any(e => name.EndsWith(e)))
            {
                videoCount++;
            }
            else if (audioExts.Any(e => name.EndsWith(e)))
            {
                audioCount++;
            }
            else if (docExts.Any(e => name.EndsWith(e)))
            {
                docCount++;
            }
            else
            {
                docCount++;
            }
        }

        return new List<FileCategoryReportPercentageDto>
        {
            new FileCategoryReportPercentageDto
            {
                FileCategory = ReportFileCategory.Image,
                Percentage = (double)imageCount * 100 / totalCount,
                PercentageFormatted = FormatPercentage((double)imageCount * 100 / totalCount)
            },
            new FileCategoryReportPercentageDto
            {
                FileCategory = ReportFileCategory.Video,
                Percentage = (double)videoCount * 100 / totalCount,
                PercentageFormatted = FormatPercentage((double)videoCount * 100 / totalCount)
            },
            new FileCategoryReportPercentageDto
            {
                FileCategory = ReportFileCategory.Audio,
                Percentage = (double)audioCount * 100 / totalCount,
                PercentageFormatted = FormatPercentage((double)audioCount * 100 / totalCount)
            },
            new FileCategoryReportPercentageDto
            {
                FileCategory = ReportFileCategory.Document,
                Percentage = (double)docCount * 100 / totalCount,
                PercentageFormatted = FormatPercentage((double)docCount * 100 / totalCount)
            }
        };
    }

    [AbpAuthorize]
    public async Task<List<CategoryReportPercentageDto>> GetReportDistributionByCategoryAsync()
    {
        var user = await GetCurrentUserAsync();
        var roles = await UserManager.GetRolesAsync(user);

        IQueryable<Report> query = _reportRepository.GetAllIncluding(r => r.Category);

        if (roles.Contains(StaticRoleNames.Host.CityAdmin))
        {
            if (!string.IsNullOrEmpty(user.City))
            {
                query = query.Where(r => r.City == user.City);
            }
            else
            {
                return new List<CategoryReportPercentageDto>();
            }
        }
        else if (roles.Contains(StaticRoleNames.Host.ProvinceAdmin))
        {
            if (!string.IsNullOrEmpty(user.Province))
            {
                query = query.Where(r => r.Province == user.Province);
            }
            else
            {
                return new List<CategoryReportPercentageDto>();
            }
        }
        else if (!(roles.Contains(StaticRoleNames.Host.SuperAdmin) || roles.Contains(StaticRoleNames.Host.Admin)))
        {
            throw new AbpAuthorizationException("Not authorized to view reports.");
        }

        var totalCount = await query.CountAsync();
        if (totalCount == 0)
        {
            return new List<CategoryReportPercentageDto>();
        }

        var data = await query
            .GroupBy(r => new { r.CategoryId, r.Category.PublicId, r.Category.Name })
            .Select(g => new { g.Key.PublicId, g.Key.Name, Count = g.Count() })
            .ToListAsync();

        return data
            .Select(d => new CategoryReportPercentageDto
            {
                CategoryId = d.PublicId,
                CategoryName = d.Name,
                Percentage = (double)d.Count * 100 / totalCount,
                PercentageFormatted = FormatPercentage((double)d.Count * 100 / totalCount)
            })
            .ToList();
    }

    [AbpAuthorize]
    public async Task<List<SubCategoryReportCountDto>> GetReportCountBySubCategoryAsync()
    {
        var user = await GetCurrentUserAsync();
        var roles = await UserManager.GetRolesAsync(user);

        IQueryable<Report> query = _reportRepository.GetAllIncluding(r => r.SubCategory);
        var endDate = Clock.Now;
        var startDate = endDate.AddDays(-1);
        var previousStartDate = startDate.AddDays(-1);
        query = query.Where(r => r.CreationTime >= previousStartDate);

        if (roles.Contains(StaticRoleNames.Host.CityAdmin))
        {
            if (!string.IsNullOrEmpty(user.City))
            {
                query = query.Where(r => r.City == user.City);
            }
            else
            {
                return new List<SubCategoryReportCountDto>();
            }
        }
        else if (roles.Contains(StaticRoleNames.Host.ProvinceAdmin))
        {
            if (!string.IsNullOrEmpty(user.Province))
            {
                query = query.Where(r => r.Province == user.Province);
            }
            else
            {
                return new List<SubCategoryReportCountDto>();
            }
        }
        else if (!(roles.Contains(StaticRoleNames.Host.SuperAdmin) || roles.Contains(StaticRoleNames.Host.Admin)))
        {
            throw new AbpAuthorizationException("Not authorized to view reports.");
        }

        var data = await query
            .GroupBy(r => new { r.SubCategoryId, r.SubCategory.PublicId, r.SubCategory.Name })
            .Select(g => new
            {
                g.Key.PublicId,
                g.Key.Name,
                CurrentCount = g.Count(r => r.CreationTime >= startDate),
                PreviousCount = g.Count(r => r.CreationTime < startDate)
            })
            .ToListAsync();

        return data
            .Select(d =>
            {
                double growth;
                if (d.PreviousCount == 0)
                {
                    growth = d.CurrentCount > 0 ? 100 : 0;
                }
                else
                {
                    growth = ((double)d.CurrentCount - d.PreviousCount) * 100 / d.PreviousCount;
                }
                return new SubCategoryReportCountDto
                {
                    SubCategoryId = d.PublicId,
                    SubCategoryName = d.Name,
                    Count = d.CurrentCount,
                    GrowthPercentage = growth,
                    CountFormatted = FormatLargeNumber(d.CurrentCount),
                    GrowthPercentageFormatted = FormatPercentage(growth)
                };
            })
            .ToList();
    }

    [AbpAuthorize]
    public async Task<List<CategoryWithSubCategoryReportCountDto>> GetReportCountByCategoryAsync()
    {
        var user = await GetCurrentUserAsync();
        var roles = await UserManager.GetRolesAsync(user);

        IQueryable<Report> query = _reportRepository.GetAll();

        if (roles.Contains(StaticRoleNames.Host.CityAdmin))
        {
            if (!string.IsNullOrEmpty(user.City))
            {
                query = query.Where(r => r.City == user.City);
            }
            else
            {
                return new List<CategoryWithSubCategoryReportCountDto>();
            }
        }
        else if (roles.Contains(StaticRoleNames.Host.ProvinceAdmin))
        {
            if (!string.IsNullOrEmpty(user.Province))
            {
                query = query.Where(r => r.Province == user.Province);
            }
            else
            {
                return new List<CategoryWithSubCategoryReportCountDto>();
            }
        }
        else if (!(roles.Contains(StaticRoleNames.Host.SuperAdmin) || roles.Contains(StaticRoleNames.Host.Admin)))
        {
            throw new AbpAuthorizationException("Not authorized to view reports.");
        }

        var counts = await query
            .GroupBy(r => r.SubCategoryId)
            .Select(g => new { SubCategoryId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.SubCategoryId, g => g.Count);

        var categories = await _categoryRepository.GetAllIncluding(c => c.SubCategories).ToListAsync();

        return categories
            .Select(c => new CategoryWithSubCategoryReportCountDto
            {
                CategoryId = c.PublicId,
                CategoryName = c.Name,
                SubCategories = c.SubCategories.Select(sc =>
                {
                    counts.TryGetValue(sc.Id, out var cnt);
                    return new SubCategoryReportCountByCategoryDto()
                    {
                        SubCategoryId = sc.PublicId,
                        SubCategoryName = sc.Name,
                        Count = cnt
                    };
                }).ToList()
            })
            .ToList();
    }



    private async Task EnsureReportAccessAsync(Guid reportId, Authorization.Users.User user)
    {
        var roles = await UserManager.GetRolesAsync(user);
        if (!(roles.Contains(StaticRoleNames.Host.Admin) ||
              roles.Contains(StaticRoleNames.Host.SuperAdmin) ||
              roles.Contains(StaticRoleNames.Host.ProvinceAdmin) ||
              roles.Contains(StaticRoleNames.Host.CityAdmin)))
        {
            throw new AbpAuthorizationException("Not authorized to view reports.");
        }

        if (roles.Contains(StaticRoleNames.Host.CityAdmin))
        {
            var report = await _reportRepository.FirstOrDefaultAsync(reportId);
            if (report != null && !string.IsNullOrEmpty(user.City) && report.City != user.City)
            {
                throw new AbpAuthorizationException("Not authorized to access this report");
            }
        }
        else if (roles.Contains(StaticRoleNames.Host.ProvinceAdmin))
        {
            var report = await _reportRepository.FirstOrDefaultAsync(reportId);
            if (report != null && !string.IsNullOrEmpty(user.Province) && report.Province != user.Province)
            {
                throw new AbpAuthorizationException("Not authorized to access this report");
            }
        }
    }




    [AbpAuthorize]
    public async Task<List<ReportLocationDto>> GetReportLocationsAsync(GetReportLocationsInput input)
    {
        input.Normalize();
        var (city, province) = await NormalizeLocationAsync(input.City, input.Province);

        var query = _reportRepository.GetAllIncluding(r => r.Category, r => r.SubCategory);

        if (input.CategoryIds != null && input.CategoryIds.Any())
        {
            query = query.Where(r => input.CategoryIds.Contains(r.Category.PublicId));
        }

        if (input.SubCategoryIds != null && input.SubCategoryIds.Any())
        {
            query = query.Where(r => input.SubCategoryIds.Contains(r.SubCategory.PublicId));
        }

        if (input.StartDate.HasValue)
        {
            query = query.Where(r => r.CreationTime >= input.StartDate.Value);
        }

        if (input.EndDate.HasValue)
        {
            query = query.Where(r => r.CreationTime <= input.EndDate.Value);
        }

        if (!string.IsNullOrEmpty(province))
        {
            query = query.Where(r => r.Province == province);
        }

        if (!string.IsNullOrEmpty(city))
        {
            query = query.Where(r => r.City == city);
        }

        if (input.StartClock.HasValue)
        {
            query = query.Where(r => (r.PersianCreationTime % 1000000) >= input.StartClock.Value);
        }

        if (input.EndClock.HasValue)
        {
            query = query.Where(r => (r.PersianCreationTime % 1000000) <= input.EndClock.Value);
        }

        return await query
            .Select(r => new ReportLocationDto
            {
                Id = r.Id,
                UniqueId = r.UniqueId,
                Latitude = r.Latitude,
                Longitude = r.Longitude,
                CategoryId = r.Category.PublicId,
                CategoryName = r.Category.Name,
                SubCategoryId = r.SubCategory.PublicId,
                SubCategoryName = r.SubCategory.Name
            })
            .ToListAsync();
    }

    [AbpAuthorize]
    public async Task<List<TopReporterDto>> GetTopReportersAsync()
    {
        var user = await GetCurrentUserAsync();
        var roles = await UserManager.GetRolesAsync(user);

        IQueryable<Report> query = _reportRepository.GetAll();

        if (roles.Contains(StaticRoleNames.Host.CityAdmin))
        {
            if (!string.IsNullOrEmpty(user.City))
            {
                query = query.Where(r => r.City == user.City);
            }
            else
            {
                return new List<TopReporterDto>();
            }
        }
        else if (roles.Contains(StaticRoleNames.Host.ProvinceAdmin))
        {
            if (!string.IsNullOrEmpty(user.Province))
            {
                query = query.Where(r => r.Province == user.Province);
            }
            else
            {
                return new List<TopReporterDto>();
            }
        }
        else if (!(roles.Contains(StaticRoleNames.Host.SuperAdmin) || roles.Contains(StaticRoleNames.Host.Admin)))
        {
            throw new AbpAuthorizationException("Not authorized to view reports.");
        }

        var totalCount = await query.CountAsync();
        if (totalCount == 0)
        {
            return new List<TopReporterDto>();
        }

        var data = await query
            .GroupBy(r => r.PhoneNumber)
            .Select(g => new
            {
                PhoneNumber = g.Key,
                City = g.OrderBy(r => r.CreationTime).Select(r => r.City).FirstOrDefault(),
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .Take(5)
            .ToListAsync();

        return data.Select(d => new TopReporterDto
        {
            PhoneNumber = d.PhoneNumber.ToString(),
            CityName = d.City,
            TotalReports = d.Count,
            Percentage = (double)d.Count * 100 / totalCount
        }).ToList();
    }

    public async Task<List<CityReportCountDto>> GetTopCitiesByReportCountAsync()
    {
        var query = _reportRepository.GetAll();
        var totalCount = await query.CountAsync();
        if (totalCount == 0)
        {
            return new List<CityReportCountDto>();
        }

        var data = await query
            .GroupBy(r => r.City)
            .Select(g => new { City = g.Key, Count = g.Count() })
            .OrderByDescending(g => g.Count)
            .Take(5)
            .ToListAsync();

        return data
            .Select(d => new CityReportCountDto
            {
                CityName = d.City,
                TotalReports = d.Count,
                Percentage = (double)d.Count * 100 / totalCount,
                PercentageFormatted = FormatPercentage((double)d.Count * 100 / totalCount)
            })
            .ToList();
    }

    private async Task<(string city, string province)> NormalizeLocationAsync(string city, string province)
    {
        province = TrimPrefix(province);
        city = TrimPrefix(city);

        if (!string.IsNullOrEmpty(province))
        {
            var provinceEntity = await _provinceRepository.FirstOrDefaultAsync(p => p.Name == province);
            if (provinceEntity != null)
            {
                province = provinceEntity.Name;

                if (!string.IsNullOrEmpty(city))
                {
                    var cityEntity = await _cityRepository.FirstOrDefaultAsync(c => c.Name == city && c.ProvinceId == provinceEntity.Id);
                    if (cityEntity != null)
                    {
                        city = cityEntity.Name;
                    }
                }
            }
        }

        return (city, province);
    }

    private static string TrimPrefix(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return name;
        }

        name = name.Trim();
        if (name.StartsWith("استان "))
        {
            name = name.Substring(6);
        }
        if (name.StartsWith("شهرستان "))
        {
            name = name.Substring(8);
        }
        if (name.StartsWith("شهر "))
        {
            name = name.Substring(4);
        }

        return name;
    }

    private static string FormatLargeNumber(double value)
    {
        if (value >= 1_000_000_000)
        {
            return (value / 1_000_000_000d).ToString("0.#") + "B";
        }
        if (value >= 1_000_000)
        {
            return (value / 1_000_000d).ToString("0.#") + "M";
        }
        if (value >= 1_000)
        {
            return (value / 1_000d).ToString("0.#") + "K";
        }
        return value.ToString("0");
    }

    private static string FormatPercentage(double value)
    {
        return value.ToString("0.00");
    }

    private async Task EnsureNotBlacklistedAsync(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            return;
        }
        var normalized = PhoneNumberHelper.Normalize(phoneNumber);
        if (long.TryParse(normalized, out var digits))
        {
            if (await _blackListRepository.CountAsync(e => e.PhoneNumber == digits) > 0)
            {
                throw new UserFriendlyException(L("PhoneNumberBlackListed"));
            }
        }
    }

    private async Task CheckBlackListFromHeaderAsync()
    {
        var phone = _httpContextAccessor.HttpContext?.Request?.Headers["X-PhoneNumber"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(phone))
        {
            return;
        }
        var normalized = PhoneNumberHelper.Normalize(phone);
        if (long.TryParse(normalized, out var digits))
        {
            if (await _blackListRepository.CountAsync(e => e.PhoneNumber == digits) > 0)
            {
                throw new UserFriendlyException(L("PhoneNumberBlackListed"));
            }
        }
    }

    [AbpAuthorize]
    public async Task<FileDto> GetReportsForAdminToExcelAsync(GetReportsInput input)
    {
        var result = await GetReportsForAdminAsync(input);
        return await _reportListExcelExporter.ExportToFile(result.Items.ToList());
    }

    [AbpAuthorize]
    [HttpGet]
    public async Task<FileContentResult> DownloadReportsForAdminToExcel(GetReportsInput input)
    {
        var fileDto = await GetReportsForAdminToExcelAsync(input);
        var bytes = _tempFileCacheManager.GetFile(fileDto.FileToken);
        _tempFileCacheManager.ClearFile(fileDto.FileToken);

        return new FileContentResult(bytes, fileDto.FileType)
        {
            FileDownloadName = fileDto.FileName
        };
    }


}