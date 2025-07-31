using System;

namespace HodHod.Reports.Dto;

public class ReportLocationDto
{
    public Guid Id { get; set; }
    public string UniqueId { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}