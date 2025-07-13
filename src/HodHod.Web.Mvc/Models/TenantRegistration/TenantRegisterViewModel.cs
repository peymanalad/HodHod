using HodHod.Editions;
using HodHod.Editions.Dto;
using HodHod.MultiTenancy.Payments;
using HodHod.Security;
using HodHod.MultiTenancy.Payments.Dto;

namespace HodHod.Web.Models.TenantRegistration;

public class TenantRegisterViewModel
{
    public int? EditionId { get; set; }

    public EditionSelectDto Edition { get; set; }

    public PasswordComplexitySetting PasswordComplexitySetting { get; set; }

    public EditionPaymentType EditionPaymentType { get; set; }

    public SubscriptionStartType? SubscriptionStartType { get; set; }

    public PaymentPeriodType? PaymentPeriodType { get; set; }

    public string SuccessUrl { get; set; }

    public string ErrorUrl { get; set; }
}

