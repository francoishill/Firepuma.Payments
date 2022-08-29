﻿using Firepuma.Payments.Core.PaymentAppConfiguration.Entities;
using Firepuma.Payments.Core.PaymentAppConfiguration.ValueObjects;
using Firepuma.Payments.Core.Payments.ValueObjects;
using Firepuma.Payments.Core.Repositories;

namespace Firepuma.Payments.Core.PaymentAppConfiguration.Repositories;

public interface IPaymentApplicationConfigRepository : IRepository<PaymentApplicationConfig>
{
    Task<PaymentApplicationConfig> GetItemOrDefaultAsync(
        ClientApplicationId applicationId,
        PaymentGatewayTypeId gatewayTypeId,
        CancellationToken cancellationToken);
}