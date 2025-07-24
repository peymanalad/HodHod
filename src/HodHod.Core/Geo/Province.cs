using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;

namespace HodHod.Geo;

[Table("AppProvinces")]
public class Province : FullAuditedEntity<int>
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; }

    public ICollection<City> Cities { get; set; }

    public Province()
    {
        Cities = new List<City>();
    }
}