﻿using Abp.Dependency;
using Abp.Extensions;
using Abp.Runtime.Session;
using HodHod.Editions;
using HodHod.ExtraProperties;
using HodHod.MultiTenancy.Payments;
using HodHod.Url;

namespace HodHod.Web.Url;

public class PaymentUrlGenerator : IPaymentUrlGenerator, ITransientDependency
{
    private readonly IWebUrlService _webUrlService;

    public PaymentUrlGenerator(
        IWebUrlService webUrlService)
    {
        _webUrlService = webUrlService;
    }

    public string CreatePaymentRequestUrl(SubscriptionPayment subscriptionPayment)
    {
        var webSiteRootAddress = _webUrlService.GetSiteRootAddress();

        var url = webSiteRootAddress.EnsureEndsWith('/') +
                  "Payment/GatewaySelection" +
                  "?paymentId=" + subscriptionPayment.Id;

        return url;
    }
}

