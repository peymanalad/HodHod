using System;
using System.Collections.Generic;
using Abp.Application.Services.Dto;
using HodHod.Reports;

namespace HodHod.Reports.Dto;

public class ReportDto : EntityDto<Guid>
{
    public string UniqueId { get; set; }
    //public Guid CategoryId { get; set; }
    //public Guid SubCategoryId { get; set; }
    public string CategoryName { get; set; }
    public string SubCategoryName { get; set; }
    public string Description { get; set; }
    public string Address { get; set; }
    public double? Longitude { get; set; }
    public double? Latitude { get; set; }
    public string PhoneNumber { get; set; }
    public string Province { get; set; }
    public string City { get; set; }
    public long? PersianCreationTime { get; set; }
    public long? PersianLastModificationTime { get; set; }
    public long? PersianDeletionTime { get; set; }
    public ReportStatus Status { get; set; }
    public ReportPriority Priority { get; set; }
    public bool IsReferred { get; set; }
    public bool IsStarred { get; set; }
    public bool IsArchived { get; set; }
    public bool IsStarredByCurrentUser { get; set; }
    public bool HasNotes { get; set; }
    public int NoteCount { get; set; }
    public DateTime CreationTime { get; set; }
    public List<string> FilePaths { get; set; }
    public ReportFileCountsDto FileCounts { get; set; }
}