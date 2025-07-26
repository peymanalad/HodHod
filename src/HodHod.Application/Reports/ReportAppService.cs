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
    //private readonly IBinaryObjectManager _binaryObjectManager;
    private readonly ITempFileCacheManager _tempFileCacheManager;
    private readonly IPasswordlessLoginManager _passwordlessLoginManager;
    private readonly IAppFolders _appFolders;
    private readonly ISmsSender _smsSender;
    private readonly ICacheManager _cacheManager;
    private readonly ILocationAppService _locationAppService;

    public ReportAppService(
        IRepository<Report, Guid> reportRepository,
        IRepository<ReportFile, Guid> reportFileRepository,
        //IBinaryObjectManager binaryObjectManager,
        ITempFileCacheManager tempFileCacheManager,
        //IPasswordlessLoginManager passwordlessLoginManager)
        IPasswordlessLoginManager passwordlessLoginManager,
        IAppFolders appFolders,
        ISmsSender smsSender,
        ICacheManager cacheManager,
        IRepository<Category, int> categoryRepository,
        IRepository<SubCategory, int> subCategoryRepository,
        IRepository<PhoneReportLimit, int> phoneReportLimitRepository,
        IRepository<Province, int> provinceRepository,
        IRepository<City, int> cityRepository,
        ILocationAppService locationAppService,
        IRepository<ReportStar, Guid> reportStarRepository)
    {
        _reportRepository = reportRepository;
        _reportFileRepository = reportFileRepository;
        //_binaryObjectManager = binaryObjectManager;
        _tempFileCacheManager = tempFileCacheManager;
        _passwordlessLoginManager = passwordlessLoginManager;
        _appFolders = appFolders;
        _smsSender = smsSender;
        _cacheManager = cacheManager;
        _categoryRepository = categoryRepository;
        _subCategoryRepository = subCategoryRepository;
        _phoneReportLimitRepository = phoneReportLimitRepository;
        _provinceRepository = provinceRepository;
        _cityRepository = cityRepository;
        _locationAppService = locationAppService;
        _reportStarRepository = reportStarRepository;
    }
    public async Task SendReportOtpAsync(SendReportOtpInput input)
    {
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

    public async System.Threading.Tasks.Task SubmitReport(CreateReportDto input)
    {
        var normalized = PhoneNumberHelper.Normalize(input.PhoneNumber);
        await _passwordlessLoginManager.VerifyPasswordlessLoginCode(
            AbpSession.TenantId,
            input.PhoneNumber,
            input.OtpCode);

        var phoneDigits = long.Parse(normalized);
        var limit = await _phoneReportLimitRepository.FirstOrDefaultAsync(l => l.PhoneNumber == phoneDigits); var maxFileCount = limit?.MaxFileCount ?? PhoneReportLimitDefaults.MaxFileCount;
        var maxFileSize = limit?.MaxFileSizeInBytes ?? PhoneReportLimitDefaults.MaxFileSizeInBytes;
        var maxReportsPerHour = limit?.MaxReportsPerHour ?? PhoneReportLimitDefaults.MaxReportsPerHour;

        var recentCount = await _reportRepository.CountAsync(r => r.PhoneNumber == phoneDigits && r.CreationTime > Clock.Now.AddHours(-1)); if (recentCount >= maxReportsPerHour)
        {
            throw new UserFriendlyException("Report limit reached");
        }

        if (input.FileTokens != null && input.FileTokens.Count > maxFileCount)
        {
            throw new UserFriendlyException("Too many files");
        }

        var category = await _categoryRepository
            .GetAll()
            .FirstOrDefaultAsync(c => c.PublicId == input.CategoryId);
        if (category == null)
        {
            throw new EntityNotFoundException("Category not found");
        }

        var subCategory = await _subCategoryRepository
            .GetAll()
            .FirstOrDefaultAsync(s => s.PublicId == input.SubCategoryId);
        if (subCategory == null)
        {
            throw new EntityNotFoundException("SubCategory not found");
        }

        var loc = await _locationAppService.ReverseGeocodeAsync(latitude: input.Latitude, longitude: input.Longitude);

        var report = new Report
        {
            CategoryId = category.Id,
            SubCategoryId = subCategory.Id,
            UniqueId = $"HD-{Guid.NewGuid().ToString("N").Substring(0, 8)}",
            Description = input.Description,
            Address = input.Address,
            Longitude = input.Longitude,
            Latitude = input.Latitude,
            PhoneNumber = long.Parse(normalized),
            Province = loc.Province,
            City = loc.City,
            PersianCreationTime = PersianDateTimeHelper.ToCompactPersianNumber(Clock.Now),
            Status = ReportStatus.Unreviewed,
            IsReferred = false,
            IsStarred = false,
            IsArchived = false
        };

        await _reportRepository.InsertAsync(report);
        try
        {
            await CurrentUnitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Logger.Error("Error in SaveChangesAsync: " + ex.ToString());
            throw;
        }

        if (input.FileTokens != null)
        {
            foreach (var token in input.FileTokens)
            {
                var info = _tempFileCacheManager.GetFileInfo(token);
                if (info == null)
                {
                    continue;
                }

                //var binary = new BinaryObject(AbpSession.TenantId, info.File,
                //    $"Report {report.Id} file {info.FileName}");
                //await _binaryObjectManager.SaveAsync(binary);

                if (info.File.Length > maxFileSize)
                {
                    throw new UserFriendlyException("File too large");
                }

                var ext = Path.GetExtension(info.FileName);
                var uniqueName = Guid.NewGuid().ToString("N") + ext;
                var savePath = Path.Combine(_appFolders.ReportFilesFolder, uniqueName);

                File.WriteAllBytes(savePath, info.File);
                _tempFileCacheManager.ClearFile(token);

                await _reportFileRepository.InsertAsync(new ReportFile
                {
                    ReportId = report.Id,
                    FileName = info.FileName,
                    FilePath = uniqueName
                });
            }
        }
    }
    /// <summary>
    /// Retrieves reports for admins with paging, sorting and filtering.
    /// Reports are limited based on the admin role.
    /// </summary>
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
            var audioExts = new[] { ".mp3", ".wav", ".ogg", ".m4a", ".flac", ".wma" };
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
        var countAudioExts = new[] { ".webm",".mp3", ".wav", ".ogg", ".m4a", ".flac", ".wma" };
        var countDocExts = new[] { ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".txt", ".odt", ".ods", ".odp" };
        var totalCount = await query.CountAsync();


        var reports = await query
            .OrderBy(input.Sorting)
            .PageBy<Report>(input)
            .ToListAsync();

        var dto = ObjectMapper.Map<List<ReportDto>>(reports);

        var starredIds = await _reportStarRepository.GetAll()
            .Where(s => s.UserId == user.Id)
            .Select(s => s.ReportId)
            .ToListAsync();
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
                Percentage = (double)d.Count * 100 / totalCount
            })
            .ToList();
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
                Percentage = (double)d.Count * 100 / totalCount
            })
            .ToList();
    }

    [AbpAuthorize]
    public async Task<List<SubCategoryReportCountDto>> GetReportCountBySubCategoryAsync()
    {
        var user = await GetCurrentUserAsync();
        var roles = await UserManager.GetRolesAsync(user);

        IQueryable<Report> query = _reportRepository.GetAllIncluding(r => r.SubCategory);

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
            .Select(g => new { g.Key.PublicId, g.Key.Name, Count = g.Count() })
            .ToListAsync();

        return data
            .Select(d => new SubCategoryReportCountDto
            {
                SubCategoryId = d.PublicId,
                SubCategoryName = d.Name,
                Count = d.Count
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

}