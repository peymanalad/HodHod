using HodHod.MultiTenancy.Payments.Dto;
using HodHod.MultiTenancy.Payments.Stripe;

namespace HodHod.Web.Models.Stripe;

public class StripePurchaseViewModel
{
    public SubscriptionPaymentDto Payment { get; set; }

    public decimal Amount { get; set; }

    public bool IsRecurring { get; set; }

    public bool IsProrationPayment { get; set; }

    public string SessionId { get; set; }

    public StripePaymentGatewayConfiguration Configuration { get; set; }
}

