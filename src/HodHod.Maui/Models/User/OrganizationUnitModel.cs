using Abp.AutoMapper;
using HodHod.Organizations.Dto;

namespace HodHod.Maui.Models.User;

[AutoMapFrom(typeof(OrganizationUnitDto))]
public class OrganizationUnitModel : OrganizationUnitDto
{
    public bool IsAssigned { get; set; }
}