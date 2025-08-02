using System;
using System.Threading.Tasks;

namespace HodHod.Reports;

public interface IReportHistoryManager
{
    Task LogAsync(Guid reportId, long userId, string fullName, ReportActionType actionType, string details, ReportHistoryVisibility visibility);
}