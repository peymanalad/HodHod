using Abp.Events.Bus;

namespace HodHod.MultiTenancy.Subscription;

public class RecurringPaymentsEnabledEventData : EventData
{
    public int TenantId { get; set; }
}

