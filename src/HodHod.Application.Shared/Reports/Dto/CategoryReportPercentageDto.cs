using System;

namespace HodHod.Reports.Dto;

public class CategoryReportPercentageDto
{
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; }
    public double Percentage { get; set; }
    public string PercentageFormatted { get; set; }
}