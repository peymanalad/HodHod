using System;

namespace HodHod.Reports.Dto;

public class FileCategoryReportPercentageDto
{
    public ReportFileCategory FileCategory { get; set; }
    public double Percentage { get; set; }
    public string PercentageFormatted { get; set; }
}