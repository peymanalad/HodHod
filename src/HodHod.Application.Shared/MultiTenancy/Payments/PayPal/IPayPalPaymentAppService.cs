using System.Threading.Tasks;
using Abp.Application.Services;
using HodHod.MultiTenancy.Payments.PayPal.Dto;

namespace HodHod.MultiTenancy.Payments.PayPal;

public interface IPayPalPaymentAppService : IApplicationService
{
    Task ConfirmPayment(long paymentId, string paypalOrderId);

    PayPalConfigurationDto GetConfiguration();
}

