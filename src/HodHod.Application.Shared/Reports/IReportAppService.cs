using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using HodHod.Reports.Dto;

namespace HodHod.Reports;

public interface IReportAppService : IApplicationService
{
    Task SendReportOtpAsync(SendReportOtpInput input);
    Task SubmitReport(CreateReportDto input);
    Task<PagedResultDto<ReportDto>> GetReportsForAdminAsync(GetReportsInput input);
    Task ChangeReportStatus(ChangeReportStatusDto input);
    Task StarReport(EntityDto<Guid> input);
    Task UnstarReport(EntityDto<Guid> input);
    Task<List<ProvinceReportPercentageDto>> GetReportDistributionByProvinceAsync();
    Task<List<CategoryReportPercentageDto>> GetReportDistributionByCategoryAsync();
    Task<List<SubCategoryReportCountDto>> GetReportCountBySubCategoryAsync();
}