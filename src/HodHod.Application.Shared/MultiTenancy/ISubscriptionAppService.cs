using System.Threading.Tasks;
using Abp.Application.Services;
using HodHod.MultiTenancy.Dto;
using HodHod.MultiTenancy.Payments.Dto;

namespace HodHod.MultiTenancy;

public interface ISubscriptionAppService : IApplicationService
{
    Task DisableRecurringPayments();

    Task EnableRecurringPayments();

    Task<long> StartExtendSubscription(StartExtendSubscriptionInput input);

    Task<StartUpgradeSubscriptionOutput> StartUpgradeSubscription(StartUpgradeSubscriptionInput input);

    Task<long> StartTrialToBuySubscription(StartTrialToBuySubscriptionInput input);
}

