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

    public void Normalize()
    {
        if (string.IsNullOrEmpty(Sorting))
        {
            Sorting = "CreationTime DESC";
        }
    }
}