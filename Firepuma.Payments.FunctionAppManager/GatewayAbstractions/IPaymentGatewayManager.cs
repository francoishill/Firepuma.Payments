﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Firepuma.Payments.Core.ValueObjects;
using Firepuma.Payments.FunctionAppManager.GatewayAbstractions.Results;
using Microsoft.AspNetCore.Http;

namespace Firepuma.Payments.FunctionAppManager.GatewayAbstractions;

public interface IPaymentGatewayManager
{
    /// <summary>
    /// Unique type ID to distinguish the type during dependency injection
    /// </summary>
    PaymentGatewayTypeId TypeId { get; }

    /// <summary>
    /// The display name that might be showed to a user
    /// </summary>
    string DisplayName { get; }

    Task<ResultContainer<CreateClientApplicationRequestResult, CreateClientApplicationRequestFailureReason>> DeserializeCreateClientApplicationRequestAsync(
        HttpRequest req,
        CancellationToken cancellationToken);

    Dictionary<string, object> CreatePaymentApplicationConfigExtraValues(object genericRequestDto);
}