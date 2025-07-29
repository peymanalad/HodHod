using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;

namespace HodHod.BlackLists;

[Table("AppBlackListEntries")]
public class BlackListEntry : FullAuditedEntity<int>
{
    [Required]
    [Range(989000000000, 999999999999)]
    public long PhoneNumber { get; set; }

    public bool IsTransferred { get; set; }
}