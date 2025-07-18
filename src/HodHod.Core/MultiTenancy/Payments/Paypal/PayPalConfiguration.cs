using System.Collections.Generic;
using Abp.Extensions;
using Microsoft.Extensions.Configuration;
using HodHod.Configuration;

namespace HodHod.MultiTenancy.Payments.Paypal;

public class PayPalPaymentGatewayConfiguration : IPaymentGatewayConfiguration
{
    private readonly IConfigurationRoot _appConfiguration;

    public SubscriptionPaymentGatewayType GatewayType => SubscriptionPaymentGatewayType.Paypal;

    public string PayPalEnvironment => System.Environment.GetEnvironmentVariable("Payment:PayPal:Environment");

    public string ClientId => System.Environment.GetEnvironmentVariable("Payment:PayPal:ClientId");

    public string ClientSecret => System.Environment.GetEnvironmentVariable("Payment:PayPal:ClientSecret");

    public string DemoUsername => System.Environment.GetEnvironmentVariable("Payment:PayPal:DemoUsername");

    public string DemoPassword => System.Environment.GetEnvironmentVariable("Payment:PayPal:DemoPassword");

    public bool IsActive => System.Environment.GetEnvironmentVariable("Payment:PayPal:IsActive").To<bool>();


    public List<string> DisabledFundings =>
        _appConfiguration.GetSection("Payment:PayPal:DisabledFundings").Get<List<string>>();

    public bool SupportsRecurringPayments => false;

    public PayPalPaymentGatewayConfiguration(IAppConfigurationAccessor configurationAccessor)
    {
        _appConfiguration = configurationAccessor.Configuration;
    }
}

