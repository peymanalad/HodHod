using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;
using HodHod.Categories;

namespace HodHod.Reports;

[Table("AppReports")]
public class Report : FullAuditedEntity<long>
{
    [Required]
    public int CategoryId { get; set; }

    [Required]
    public int SubCategoryId { get; set; }

    [ForeignKey(nameof(CategoryId))]
    public Category Category { get; set; }

    [ForeignKey(nameof(SubCategoryId))]
    public SubCategory SubCategory { get; set; }

    [Required]
    [StringLength(4096)]
    public string Description { get; set; }

    [StringLength(1024)]
    public string Address { get; set; }

    public double? Longitude { get; set; }
    public double? Latitude { get; set; }

    [StringLength(12)]
    public string PhoneNumber { get; set; }

    public ICollection<ReportFile> Files { get; set; }

    public Report()
    {
        Files = new List<ReportFile>();
    }
}