using System;

namespace HodHod.Reports.Dto;

public class SubCategoryReportCountDto
{
    public Guid SubCategoryId { get; set; }
    public string SubCategoryName { get; set; }
    public int Count { get; set; }
    public double GrowthPercentage { get; set; }
    public string CountFormatted { get; set; }
    public string GrowthPercentageFormatted { get; set; }
}