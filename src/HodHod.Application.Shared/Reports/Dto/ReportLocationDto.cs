using System;

namespace HodHod.Reports.Dto;

public class ReportLocationDto
{
    public Guid Id { get; set; }
    public string UniqueId { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; }
    public Guid SubCategoryId { get; set; }
    public string SubCategoryName { get; set; }
}