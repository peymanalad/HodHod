using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;

namespace HodHod.Geo;

[Table("AppCities")]
public class City : FullAuditedEntity<int>
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; }

    public int ProvinceId { get; set; }

    [ForeignKey(nameof(ProvinceId))]
    public Province Province { get; set; }
}