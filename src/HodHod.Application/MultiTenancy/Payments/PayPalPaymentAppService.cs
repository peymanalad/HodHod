using System.Threading.Tasks;
using HodHod.MultiTenancy.Payments.Paypal;
using HodHod.MultiTenancy.Payments.PayPal;
using HodHod.MultiTenancy.Payments.PayPal.Dto;

namespace HodHod.MultiTenancy.Payments;

public class PayPalPaymentAppService : HodHodAppServiceBase, IPayPalPaymentAppService
{
    private readonly PayPalGatewayManager _payPalGatewayManager;
    private readonly ISubscriptionPaymentRepository _subscriptionPaymentRepository;
    private readonly PayPalPaymentGatewayConfiguration _payPalPaymentGatewayConfiguration;

    public PayPalPaymentAppService(
        PayPalGatewayManager payPalGatewayManager,
        ISubscriptionPaymentRepository subscriptionPaymentRepository,
        PayPalPaymentGatewayConfiguration payPalPaymentGatewayConfiguration)
    {
        _payPalGatewayManager = payPalGatewayManager;
        _subscriptionPaymentRepository = subscriptionPaymentRepository;
        _payPalPaymentGatewayConfiguration = payPalPaymentGatewayConfiguration;
    }

    public async Task ConfirmPayment(long paymentId, string paypalOrderId)
    {
        var payment = await _subscriptionPaymentRepository.GetAsync(paymentId);

        await _payPalGatewayManager.CaptureOrderAsync(
            new PayPalCaptureOrderRequestInput(paypalOrderId)
        );

        payment.Gateway = SubscriptionPaymentGatewayType.Paypal;
        payment.ExternalPaymentId = paypalOrderId;
        payment.SetAsPaid();
    }

    public PayPalConfigurationDto GetConfiguration()
    {
        return new PayPalConfigurationDto
        {
            ClientId = _payPalPaymentGatewayConfiguration.ClientId,
            DemoUsername = _payPalPaymentGatewayConfiguration.DemoUsername,
            DemoPassword = _payPalPaymentGatewayConfiguration.DemoPassword,
            DisabledFundings = _payPalPaymentGatewayConfiguration.DisabledFundings
        };
    }
}
