using HodHod.Dto;

namespace HodHod.Organizations.Dto;

public class FindOrganizationUnitUsersInput : PagedAndFilteredInputDto
{
    public long OrganizationUnitId { get; set; }
}

