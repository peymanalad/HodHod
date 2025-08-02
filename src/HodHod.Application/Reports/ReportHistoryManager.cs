using System;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Timing;

namespace HodHod.Reports;

public class ReportHistoryManager : IReportHistoryManager, ITransientDependency
{
    private readonly IRepository<ReportHistoryLog, Guid> _repository;

    public ReportHistoryManager(IRepository<ReportHistoryLog, Guid> repository)
    {
        _repository = repository;
    }

    public async Task LogAsync(Guid reportId, long userId, string fullName, ReportActionType actionType, string details, ReportHistoryVisibility visibility)
    {
        var entity = new ReportHistoryLog
        {
            ReportId = reportId,
            PerformedByUserId = userId,
            PerformedByFullName = fullName,
            ActionType = actionType,
            ActionDetails = details,
            ActionTime = Clock.Now,
            Visibility = visibility
        };
        await _repository.InsertAsync(entity);
    }
}