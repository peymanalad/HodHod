using System.Collections.Generic;
using HodHod.Organizations.Dto;

namespace HodHod.Web.Areas.App.Models.Common;

public interface IOrganizationUnitsEditViewModel
{
    List<OrganizationUnitDto> AllOrganizationUnits { get; set; }

    List<string> MemberedOrganizationUnits { get; set; }
}

