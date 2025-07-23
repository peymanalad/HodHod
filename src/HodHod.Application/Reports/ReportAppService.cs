using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Domain.Entities;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Runtime.Caching;
using Abp.Timing;
using Abp.UI;
using HodHod.Authorization.PasswordlessLogin;
using HodHod.Authorization.Roles;
using HodHod.Categories;
using HodHod.Net.Sms;
using HodHod.Reports.Dto;
using HodHod.Storage;
using Microsoft.EntityFrameworkCore;
using Twilio.TwiML.Voice;
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
    //private readonly IBinaryObjectManager _binaryObjectManager;
    private readonly ITempFileCacheManager _tempFileCacheManager;
    private readonly IPasswordlessLoginManager _passwordlessLoginManager;
    private readonly IAppFolders _appFolders;
    private readonly ISmsSender _smsSender;
    private readonly ICacheManager _cacheManager;
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
        IRepository<PhoneReportLimit, int> phoneReportLimitRepository)
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
    }
    public async Task SendReportOtpAsync(SendReportOtpInput input)
    {
        if (String.IsNullOrEmpty(input.PhoneNumber))
        {
            return;
        }


        var limiterCache = _cacheManager.GetCache<string, OtpSendLimitCacheItem>(OtpSendLimitCacheItem.CacheName);
        var limiter = await limiterCache.GetOrDefaultAsync(input.PhoneNumber);
        var now = Clock.Now;
        if (limiter != null && limiter.WindowStart.AddHours(1) > now && limiter.Count >= PhoneReportLimitDefaults.MaxReportsPerHour)
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

        var limit = await _phoneReportLimitRepository.FirstOrDefaultAsync(l => l.PhoneNumber == normalized);
        var maxFileCount = limit?.MaxFileCount ?? PhoneReportLimitDefaults.MaxFileCount;
        var maxFileSize = limit?.MaxFileSizeInBytes ?? PhoneReportLimitDefaults.MaxFileSizeInBytes;
        var maxReportsPerHour = limit?.MaxReportsPerHour ?? PhoneReportLimitDefaults.MaxReportsPerHour;

        var recentCount = await _reportRepository.CountAsync(r => r.PhoneNumber == long.Parse(normalized) && r.CreationTime > Clock.Now.AddHours(-1));
        if (recentCount >= maxReportsPerHour)
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

        var report = new Report
        {
            CategoryId = category.Id,
            SubCategoryId = subCategory.Id,
            Description = input.Description,
            Address = input.Address,
            Longitude = input.Longitude,
            Latitude = input.Latitude,
            PhoneNumber = long.Parse(normalized),
            Province = input.Province,
            City = input.City,
            PersianCreationTime = PersianDateTimeHelper.ToCompactPersianNumber(Clock.Now),
            Status = ReportStatus.New,
            //Priority = input.Priority,
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
                    //BinaryObjectId = binary.Id,
                    //FileName = info.FileName
                    FileName = info.FileName,
                    FilePath = uniqueName
                });
            }
        }
    }
    /// <summary>
    /// Retrieves reports filtered based on the current admin's role.
    /// </summary>
    [AbpAuthorize]
    public async Task<List<ReportDto>> GetReportsForAdminAsync()
    {
        var user = await GetCurrentUserAsync();
        var roles = await UserManager.GetRolesAsync(user);

        if (!roles.Contains(StaticRoleNames.Host.SuperAdmin) &&
            !roles.Contains(StaticRoleNames.Host.ProvinceAdmin) &&
            !roles.Contains(StaticRoleNames.Host.CityAdmin))
        {
            throw new AbpAuthorizationException("Not authorized to view reports.");
        }

        var query = _reportRepository.GetAll();

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
        // SuperAdmin can see all reports without filtering

        var reports = await query.Include(r => r.Files).ToListAsync();
        return ObjectMapper.Map<List<ReportDto>>(reports);
    }
}