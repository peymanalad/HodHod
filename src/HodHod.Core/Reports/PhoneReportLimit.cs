using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;

namespace HodHod.Reports;

[Table("AppPhoneReportLimits")]
public class PhoneReportLimit : FullAuditedEntity<int>
{
    [Required]
    [Range(989000000000, 999999999999)]
    public long PhoneNumber { get; set; }

    public int MaxFileCount { get; set; }
    public long MaxFileSizeInBytes { get; set; }
    public int MaxReportsPerHour { get; set; }
}