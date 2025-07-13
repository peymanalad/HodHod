using HodHod.MultiTenancy.Payments.Stripe;

namespace HodHod.Web.Controllers;

public class StripeController : StripeControllerBase
{
    public StripeController(
        StripeGatewayManager stripeGatewayManager,
        StripePaymentGatewayConfiguration stripeConfiguration,
        IStripePaymentAppService stripePaymentAppService)
        : base(stripeGatewayManager, stripeConfiguration, stripePaymentAppService)
    {
    }
}

