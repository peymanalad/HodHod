using System.Threading.Tasks;
using Abp.Application.Services;
using HodHod.MultiTenancy.Payments.Dto;
using HodHod.MultiTenancy.Payments.Stripe.Dto;

namespace HodHod.MultiTenancy.Payments.Stripe;

public interface IStripePaymentAppService : IApplicationService
{
    Task ConfirmPayment(StripeConfirmPaymentInput input);

    StripeConfigurationDto GetConfiguration();

    Task<string> CreatePaymentSession(StripeCreatePaymentSessionInput input);
}

