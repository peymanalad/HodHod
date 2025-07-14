using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Runtime.Caching;
using Abp.Timing;
using Abp.UI;
using HodHod.Authorization.PasswordlessLogin;
using HodHod.Net.Sms;
using HodHod.Reports.Dto;
using HodHod.Storage;
using Twilio.TwiML.Voice;
using Task = System.Threading.Tasks.Task;

namespace HodHod.Reports;

[AbpAllowAnonymous]
public class ReportAppService : HodHodAppServiceBase, IReportAppService
{
    private readonly IRepository<Report, long> _reportRepository;
    private readonly IRepository<ReportFile, Guid> _reportFileRepository;
    //private readonly IBinaryObjectManager _binaryObjectManager;
    private readonly ITempFileCacheManager _tempFileCacheManager;
    private readonly IPasswordlessLoginManager _passwordlessLoginManager;
    private readonly IAppFolders _appFolders;
    private readonly ISmsSender _smsSender;
    private readonly ICacheManager _cacheManager;
    public ReportAppService(
        IRepository<Report, long> reportRepository,
        IRepository<ReportFile, Guid> reportFileRepository,
        //IBinaryObjectManager binaryObjectManager,
        ITempFileCacheManager tempFileCacheManager,
                //IPasswordlessLoginManager passwordlessLoginManager)
        IPasswordlessLoginManager passwordlessLoginManager,
        IAppFolders appFolders,
        ISmsSender smsSender,
        ICacheManager cacheManager)
    {
        _reportRepository = reportRepository;
        _reportFileRepository = reportFileRepository;
        //_binaryObjectManager = binaryObjectManager;
        _tempFileCacheManager = tempFileCacheManager;
        _passwordlessLoginManager = passwordlessLoginManager;
        _appFolders = appFolders;
        _smsSender = smsSender;
        _cacheManager = cacheManager;
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
        if (limiter != null && limiter.WindowStart.AddHours(1) > now && limiter.Count >= 200)
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
    }

    public async System.Threading.Tasks.Task SubmitReport(CreateReportDto input)
    {
        var normalized = PhoneNumberHelper.Normalize(input.PhoneNumber);
        await _passwordlessLoginManager.VerifyPasswordlessLoginCode(
            AbpSession.TenantId,
            input.PhoneNumber,
        input.OtpCode);

        var report = new Report
        {
            CategoryId = input.CategoryId,
            SubCategoryId = input.SubCategoryId,
            Description = input.Description,
            Address = input.Address,
            Longitude = input.Longitude,
            Latitude = input.Latitude,
            PhoneNumber = normalized
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

                var ext = Path.GetExtension(info.FileName);
                var uniqueName = Guid.NewGuid().ToString("N") + ext;
                var savePath = Path.Combine(_appFolders.ReportFilesFolder, uniqueName);

                File.WriteAllBytes(savePath, info.File);

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
}