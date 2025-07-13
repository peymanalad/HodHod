using HodHod.Dto;

namespace HodHod.WebHooks.Dto;

public class GetAllSendAttemptsInput : PagedInputDto
{
    public string SubscriptionId { get; set; }
}

