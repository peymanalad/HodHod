using HodHod.Dto;

namespace HodHod.Common.Dto;

public class FindUsersInput : PagedAndFilteredInputDto
{
    public int? TenantId { get; set; }

    public bool ExcludeCurrentUser { get; set; }
}

