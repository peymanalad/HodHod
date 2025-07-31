using System;
using System.Collections.Generic;

namespace HodHod.Reports.Dto;

public class CategoryWithSubCategoryReportCountDto
{
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; }
    public List<SubCategoryReportCountByCategoryDto> SubCategories { get; set; }
}