using Abp.Application.Services.Dto;

namespace HodHod.Notifications.Dto;

public class GetAllForLookupTableInput : PagedAndSortedResultRequestDto
{
    public string Filter { get; set; }
}

