using Abp.Events.Bus.Handlers;
using HodHod.MultiTenancy.Subscription;

namespace HodHod.MultiTenancy.Payments;

public interface ISupportsRecurringPayments :
    IEventHandler<RecurringPaymentsDisabledEventData>,
    IEventHandler<RecurringPaymentsEnabledEventData>,
    IEventHandler<SubscriptionUpdatedEventData>,
    IEventHandler<SubscriptionCancelledEventData>
{

}

