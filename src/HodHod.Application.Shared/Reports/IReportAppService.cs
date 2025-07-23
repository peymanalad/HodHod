using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using HodHod.Reports.Dto;

namespace HodHod.Reports;

public interface IReportAppService : IApplicationService
{
    Task SendReportOtpAsync(SendReportOtpInput input);
    Task SubmitReport(CreateReportDto input);
    /// <summary>
    /// Returns reports filtered based on the role of the current user.
    /// Super admins see everything, province admins see their province
    /// and city admins see their city.
    /// </summary>
    Task<List<ReportDto>> GetReportsForAdminAsync();
}