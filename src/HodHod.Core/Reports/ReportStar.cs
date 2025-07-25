using System;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;
using HodHod.Authorization.Users;

namespace HodHod.Reports;

[Table("AppReportStars")]
public class ReportStar : CreationAuditedEntity<Guid>
{
    public Guid ReportId { get; set; }

    [ForeignKey(nameof(ReportId))]
    public Report Report { get; set; }

    public long UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User User { get; set; }
}