using System;

namespace HodHod.Reports.Dto;

public class SubCategoryReportCountByCategoryDto
{
    public Guid SubCategoryId { get; set; }
    public string SubCategoryName { get; set; }
    public int Count { get; set; }
}