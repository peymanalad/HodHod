using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HodHod.Reports.Dto
{
    public class GetReportLocationsInput
    {
        public List<Guid> CategoryIds { get; set; }
        public List<Guid> SubCategoryIds { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Province { get; set; }
        public string City { get; set; }
        public int? StartClock { get; set; }
        public int? EndClock { get; set; }

        public void Normalize()
        {
            // nothing to normalize for now
        }
    }
}
