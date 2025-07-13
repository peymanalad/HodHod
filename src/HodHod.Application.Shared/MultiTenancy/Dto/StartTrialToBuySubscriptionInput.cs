using HodHod.MultiTenancy.Payments;

namespace HodHod.MultiTenancy.Dto;

public class StartTrialToBuySubscriptionInput
{
    public PaymentPeriodType PaymentPeriodType { get; set; }

    public string SuccessUrl { get; set; }

    public string ErrorUrl { get; set; }
}

