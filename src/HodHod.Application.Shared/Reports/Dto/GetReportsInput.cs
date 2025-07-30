using System;
using Abp.Runtime.Validation;
using HodHod.Dto;

namespace HodHod.Reports.Dto;

public class GetReportsInput : PagedAndSortedInputDto, IShouldNormalize
{
    public Guid? CategoryId { get; set; }
    public Guid? SubCategoryId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Province { get; set; }
    public string City { get; set; }
    public string PhoneNumber { get; set; }
    public string UniqueId { get; set; }
    public ReportStatus? Status { get; set; }
    public long? StartPersianCreationTime { get; set; }
    public long? EndPersianCreationTime { get; set; }
    public int? StartPersianCreationClock { get; set; }
    public int? EndPersianCreationClock { get; set; }
    public ReportFileCategory? FileCategory { get; set; }
    public bool? OnlyStarredByCurrentUser { get; set; }
    public bool? HasNotes { get; set; }

    public void Normalize()
    {
        if (string.IsNullOrEmpty(Sorting))
        {
            Sorting = "CreationTime DESC";
        }
    }
}