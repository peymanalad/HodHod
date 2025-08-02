using System;

namespace HodHod.Reports.Dto;

public class ReportMapPointDto
{
    public Guid UniqueId { get; set; }
    public Guid CategoryId { get; set; }
    public Guid SubCategoryId { get; set; }
    public string CategoryName { get; set; }
    public string SubCategoryName { get; set; }
    public double Longitude { get; set; }
    public double Latitude { get; set; }
}