using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using HodHod.Dto;
using HodHod.Reports.Dto;
using Microsoft.AspNetCore.Mvc;

namespace HodHod.Reports;

public interface IReportAppService : IApplicationService
{
    Task SendReportOtpAsync(SendReportOtpInput input);
    Task SubmitReport(CreateReportDto input);
    Task<PagedResultDto<ReportDto>> GetReportsForAdminAsync(GetReportsInput input);
    Task<List<ReportMapPointDto>> GetReportMapPointsForAdminAsync(GetReportMapPointsInput input);
    Task ChangeReportStatus(ChangeReportStatusDto input);
    Task ChangeReportCategoryAsync(ChangeReportCategoryDto input);
    Task ArchiveReport(EntityDto<Guid> input);
    Task RestoreReport(EntityDto<Guid> input);
    Task RestoreReports(List<Guid> reportIds);
    Task DeleteReports(List<Guid> reportIds);
    Task StarReport(EntityDto<Guid> input);
    Task UnstarReport(EntityDto<Guid> input);
    Task<List<ProvinceReportPercentageDto>> GetReportDistributionByProvinceAsync();
    Task<List<ProvinceCityReportPercentageDto>> GetReportDistributionByProvinceAndCityAsync();
    Task<List<CategoryReportPercentageDto>> GetReportDistributionByCategoryAsync();
    Task<List<FileCategoryReportPercentageDto>> GetReportDistributionByFileCategoryAsync();
    Task<List<SubCategoryReportCountDto>> GetReportCountBySubCategoryAsync();
    Task<List<CategoryWithSubCategoryReportCountDto>> GetReportCountByCategoryAsync();
    Task<List<ReportLocationDto>> GetReportLocationsAsync(GetReportLocationsInput input);
    Task<FileDto> GetReportsForAdminToExcelAsync(GetReportsInput input);
    Task<FileContentResult> DownloadReportsForAdminToExcel(GetReportsInput input);
    Task<List<TopReporterDto>> GetTopReportersAsync();
    Task<List<CityReportCountDto>> GetTopCitiesByReportCountAsync();
}