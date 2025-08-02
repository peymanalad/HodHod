using System;

namespace HodHod.Reports.Dto;

public class ReferReportDto
{
    public Guid ReportId { get; set; }
    public long TargetUserId { get; set; }
}