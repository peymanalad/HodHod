using System;

namespace HodHod.WebHooks.Dto;

public class ActivateWebhookSubscriptionInput
{
    public Guid SubscriptionId { get; set; }

    public bool IsActive { get; set; }
}

