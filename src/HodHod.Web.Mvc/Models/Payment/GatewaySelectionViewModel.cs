using System.Collections.Generic;
using System.Linq;
using HodHod.MultiTenancy.Payments;
using HodHod.MultiTenancy.Payments.Dto;

namespace HodHod.Web.Models.Payment;

public class GatewaySelectionViewModel
{
    public SubscriptionPaymentDto Payment { get; set; }

    public List<PaymentGatewayModel> PaymentGateways { get; set; }

    public bool AllowRecurringPaymentOption()
    {
        return Payment.AllowRecurringPayment() && PaymentGateways.Any(gateway => gateway.SupportsRecurringPayments);
    }
}

