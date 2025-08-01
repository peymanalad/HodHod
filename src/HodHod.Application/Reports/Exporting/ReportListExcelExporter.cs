using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Abp.Dependency;
using ClosedXML.Excel;
using Abp.AspNetZeroCore.Net;
using HodHod.Dto;
using HodHod.Reports.Dto;
using HodHod.Storage;
using HodHod;
using HodHod.Reports;

namespace HodHod.Reports.Exporting;

public class ReportListExcelExporter : HodHodServiceBase, IReportListExcelExporter, ITransientDependency
{
    private readonly ITempFileCacheManager _tempFileCacheManager;

    public ReportListExcelExporter(ITempFileCacheManager tempFileCacheManager)
    {
        _tempFileCacheManager = tempFileCacheManager;
    }

    public async Task<FileDto> ExportToFile(List<ReportDto> reports)
    {
        var file = new FileDto("Reports.xlsx", MimeTypeNames.ApplicationVndOpenxmlformatsOfficedocumentSpreadsheetmlSheet);

        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("Reports");
            worksheet.RightToLeft = true;
            worksheet.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet.Style.Alignment.ReadingOrder = XLAlignmentReadingOrderValues.RightToLeft;

            var headers = new[]
            {
                "شناسه گزارش",
                "دسته بندی",
                "موضوع",
                "تاریخ",
                "ساعت",
                "وضعیت",
                "متن گزارش",
                "استان",
                "شهر"
            };

            for (var i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(1, i + 1).Value = headers[i];
            }

            var row = 2;
            foreach (var report in reports)
            {
                worksheet.Cell(row, 1).Value = report.UniqueId;
                worksheet.Cell(row, 2).Value = report.CategoryName;
                worksheet.Cell(row, 3).Value = report.SubCategoryName;

                if (report.PersianCreationTime.HasValue)
                {
                    var val = report.PersianCreationTime.Value;
                    var datePart = val / 1000000;
                    var timePart = val % 1000000;
                    worksheet.Cell(row, 4).Value = FormatDate(datePart);
                    worksheet.Cell(row, 5).Value = FormatTime(timePart);
                }
                worksheet.Cell(row, 6).Value = report.Status switch
                {
                    ReportStatus.Unreviewed => "بررسی نشده",
                    ReportStatus.Approved => "تائید شده",
                    ReportStatus.Rejected => "رد شده",
                    _ => string.Empty
                };
                worksheet.Cell(row, 7).Value = report.Description;
                worksheet.Cell(row, 8).Value = report.Province;
                worksheet.Cell(row, 9).Value = report.City;

                if (report.IsStarredByCurrentUser)
                {
                    worksheet.Range(row, 1, row, headers.Length).Style.Fill.BackgroundColor = XLColor.Yellow;
                }

                row++;
            }

            var used = worksheet.Range(1, 1, row - 1, headers.Length);
            used.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            used.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            used.Style.Alignment.ReadingOrder = XLAlignmentReadingOrderValues.RightToLeft;

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            _tempFileCacheManager.SetFile(file.FileToken, stream.ToArray());
        }

        return file;
    }

    private static string FormatDate(long datePart)
    {
        var s = datePart.ToString("00000000");
        return $"{s.Substring(0, 4)}/{s.Substring(4, 2)}/{s.Substring(6, 2)}";
    }

    private static string FormatTime(long timePart)
    {
        var s = timePart.ToString("000000");
        return $"{s.Substring(0, 2)}:{s.Substring(2, 2)}:{s.Substring(4, 2)}";
    }
}