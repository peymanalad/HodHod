using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;

namespace HodHod.Categories;

[Table("AppCategories")]
public class Category : FullAuditedEntity<int>
{
    [Required]
    [StringLength(256)]
    public string Name { get; set; }
    public Guid PublicId { get; set; }

    public ICollection<SubCategory> SubCategories { get; set; }

    public Category()
    {
        SubCategories = new List<SubCategory>();
    }
}