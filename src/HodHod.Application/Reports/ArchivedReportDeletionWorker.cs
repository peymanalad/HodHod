using System;
using System.Linq;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Threading.BackgroundWorkers;
using Abp.Threading.Timers;
using Abp.Timing;

namespace HodHod.Reports;

public class ArchivedReportDeletionWorker : PeriodicBackgroundWorkerBase, ISingletonDependency
{
    private const int CheckPeriodAsMilliseconds = 1 * 60 * 60 * 1000 * 24; // 1 day

    private readonly IRepository<Report, Guid> _reportRepository;
    private readonly IRepository<ReportFile, Guid> _reportFileRepository;
    private readonly IUnitOfWorkManager _unitOfWorkManager;

    public ArchivedReportDeletionWorker(
        AbpTimer timer,
        IRepository<Report, Guid> reportRepository,
        IRepository<ReportFile, Guid> reportFileRepository,
        IUnitOfWorkManager unitOfWorkManager)
        : base(timer)
    {
        _reportRepository = reportRepository;
        _reportFileRepository = reportFileRepository;
        _unitOfWorkManager = unitOfWorkManager;

        Timer.Period = CheckPeriodAsMilliseconds;
        Timer.RunOnStart = true;

        LocalizationSourceName = HodHodConsts.LocalizationSourceName;
    }

    protected override void DoWork()
    {
        _unitOfWorkManager.WithUnitOfWork(() =>
        {
            var threshold = Clock.Now.AddDays(-90);
            var reports = _reportRepository.GetAllIncluding(r => r.Files)
                .Where(r => r.IsArchived && r.ArchiveTime <= threshold)
                .ToList();

            foreach (var report in reports)
            {
                foreach (var file in report.Files.ToList())
                {
                    _reportFileRepository.Delete(file);
                }

                _reportRepository.Delete(report);
            }
        });
    }
}