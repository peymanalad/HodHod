using System.Collections.Generic;
using System.Threading.Tasks;
using HodHod.Dto;
using HodHod.Reports.Dto;

namespace HodHod.Reports.Exporting;

public interface IReportListExcelExporter
{
    Task<FileDto> ExportToFile(List<ReportDto> reports);
}