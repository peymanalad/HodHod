using System;
using System.Collections.Generic;
using Abp.Extensions;
using Microsoft.Extensions.Configuration;
using HodHod.Configuration;

namespace HodHod.MultiTenancy.Payments.Stripe;

public class StripePaymentGatewayConfiguration : IPaymentGatewayConfiguration
{
    private readonly IConfigurationRoot _appConfiguration;

    public SubscriptionPaymentGatewayType GatewayType => SubscriptionPaymentGatewayType.Stripe;

    public string BaseUrl => Environment.GetEnvironmentVariable("Payment:Stripe:BaseUrl").EnsureEndsWith('/');

    public string PublishableKey => Environment.GetEnvironmentVariable("Payment:Stripe:PublishableKey");

    public string SecretKey => Environment.GetEnvironmentVariable("Payment:Stripe:SecretKey");

    public string WebhookSecret => Environment.GetEnvironmentVariable("Payment:Stripe:WebhookSecret");

    public bool IsActive => Environment.GetEnvironmentVariable("Payment:Stripe:IsActive").To<bool>();

    public bool SupportsRecurringPayments => true;

    public List<string> PaymentMethodTypes => _appConfiguration.GetSection("Payment:Stripe:PaymentMethodTypes").Get<List<string>>();

    public StripePaymentGatewayConfiguration(IAppConfigurationAccessor configurationAccessor)
    {
        _appConfiguration = configurationAccessor.Configuration;
    }
}

