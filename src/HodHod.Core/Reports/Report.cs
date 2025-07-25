using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;
using HodHod.Categories;

namespace HodHod.Reports;

[Table("AppReports")]
public class Report : FullAuditedEntity<Guid>
{
    [StringLength(12)]
    public string UniqueId { get; set; }
    [Required]
    public int CategoryId { get; set; }

    [Required]
    public int SubCategoryId { get; set; }

    [ForeignKey(nameof(CategoryId))]
    public Category Category { get; set; }

    [ForeignKey(nameof(SubCategoryId))]
    public SubCategory SubCategory { get; set; }

    [Required]
    [StringLength(4000)]
    public string Description { get; set; }

    [StringLength(1024)]
    public string Address { get; set; }

    public double? Longitude { get; set; }
    public double? Latitude { get; set; }

    [Range(989000000000, 999999999999)]
    public long PhoneNumber { get; set; }


    [StringLength(50)]
    public string Province { get; set; }

    [StringLength(50)]
    public string City { get; set; }

    public long? PersianCreationTime { get; set; }

    public long? PersianLastModificationTime { get; set; }

    public long? PersianDeletionTime { get; set; }

    public ReportStatus Status { get; set; }
    public ReportPriority Priority { get; set; }
    public bool IsReferred { get; set; }
    public bool IsStarred { get; set; }
    public bool IsArchived { get; set; }

    public ICollection<ReportFile> Files { get; set; }
    public ICollection<ReportNote> Notes { get; set; }

    public Report()
    {
        Files = new List<ReportFile>();
        Notes = new List<ReportNote>();
    }
}