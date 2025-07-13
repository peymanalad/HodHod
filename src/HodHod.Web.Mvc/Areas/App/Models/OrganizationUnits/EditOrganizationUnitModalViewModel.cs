using Abp.AutoMapper;
using Abp.Organizations;

namespace HodHod.Web.Areas.App.Models.OrganizationUnits;

[AutoMapFrom(typeof(OrganizationUnit))]
public class EditOrganizationUnitModalViewModel
{
    public long? Id { get; set; }

    public string DisplayName { get; set; }
}

