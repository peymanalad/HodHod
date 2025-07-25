using System;
using System.ComponentModel.DataAnnotations;
using Abp.Application.Services.Dto;

namespace HodHod.Reports.Dto;

public class ChangeReportStatusDto : EntityDto<Guid>
{
    [Required]
    public ReportStatus Status { get; set; }
}