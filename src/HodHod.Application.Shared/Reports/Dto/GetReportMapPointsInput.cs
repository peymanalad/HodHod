using System;

namespace HodHod.Reports.Dto;

public class GetReportMapPointsInput
{
    public Guid? CategoryId { get; set; }
    public Guid? SubCategoryId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Province { get; set; }
    public string City { get; set; }
    public int? StartPersianCreationClock { get; set; }
    public int? EndPersianCreationClock { get; set; }
}