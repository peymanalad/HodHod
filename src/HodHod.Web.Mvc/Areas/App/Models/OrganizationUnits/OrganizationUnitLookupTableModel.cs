using System.Collections.Generic;
using HodHod.Organizations.Dto;
using HodHod.Web.Areas.App.Models.Common;

namespace HodHod.Web.Areas.App.Models.OrganizationUnits;

public class OrganizationUnitLookupTableModel : IOrganizationUnitsEditViewModel
{
    public List<OrganizationUnitDto> AllOrganizationUnits { get; set; }

    public List<string> MemberedOrganizationUnits { get; set; }

    public OrganizationUnitLookupTableModel()
    {
        AllOrganizationUnits = new List<OrganizationUnitDto>();
        MemberedOrganizationUnits = new List<string>();
    }
}

