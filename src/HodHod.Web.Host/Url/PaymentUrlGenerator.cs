using Abp.Dependency;
using HodHod.MultiTenancy.Payments;
using HodHod.Url;

namespace HodHod.Web.Url;

public class PaymentUrlGenerator : IPaymentUrlGenerator, ITransientDependency
{
    private readonly IWebUrlService _webUrlService;

    public PaymentUrlGenerator(IWebUrlService webUrlService)
    {
        _webUrlService = webUrlService;
    }

    public string CreatePaymentRequestUrl(SubscriptionPayment subscriptionPayment)
    {
        var webSiteRootAddress = _webUrlService.GetSiteRootAddress();

        return webSiteRootAddress +
               "account/gateway-selection?paymentId=" +
               subscriptionPayment.Id;
    }
}

