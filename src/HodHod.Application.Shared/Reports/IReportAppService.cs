using System.Threading.Tasks;
using Abp.Application.Services;
using HodHod.Reports.Dto;

namespace HodHod.Reports;

public interface IReportAppService : IApplicationService
{
    Task SendReportOtpAsync(SendReportOtpInput input);
    Task SubmitReport(CreateReportDto input);
}