using System.Collections.Generic;

namespace HodHod.MultiTenancy.Payments;

public interface IPaymentGatewayStore
{
    List<PaymentGatewayModel> GetActiveGateways();
}

