using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;

namespace HodHod.Categories;

[Table("AppSubCategories")]
public class SubCategory : FullAuditedEntity<int>
{
    [Required]
    [StringLength(256)]
    public string Name { get; set; }
    public Guid PublicId { get; set; }

    public int CategoryId { get; set; }

    [ForeignKey(nameof(CategoryId))]
    public Category Category { get; set; }
}