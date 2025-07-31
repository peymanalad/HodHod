using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HodHod.Reports.Dto
{
    public class CategoryWithSubCategoryReportDto
    {
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; }
        public List<SubCategoryReportCountDto> SubCategories { get; set; }
    }
}
