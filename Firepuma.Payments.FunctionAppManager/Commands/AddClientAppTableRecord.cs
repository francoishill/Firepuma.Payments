using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Firepuma.Payments.Core.PaymentAppConfiguration.Entities;
using Firepuma.Payments.Core.PaymentAppConfiguration.Repositories;
using Firepuma.Payments.FunctionAppManager.Gateways;
using MediatR;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedType.Global
// ReSharper disable RedundantNameQualifier

namespace Firepuma.Payments.FunctionAppManager.Commands;

public static class AddClientAppTableRecord
{
    public class Command : IRequest<Result>
    {
        public PaymentApplicationConfig TableRow { get; set; }

        public Command(
            PaymentApplicationConfig tableRow)
        {
            TableRow = tableRow;
        }
    }

    public class Result
    {
        public bool IsNew { get; set; }
        public PaymentApplicationConfig TableRow { get; set; }
    }


    public class Handler : IRequestHandler<Command, Result>
    {
        private readonly ILogger<Handler> _logger;
        private readonly IEnumerable<IPaymentGatewayManager> _gatewayManagers;
        private readonly IPaymentApplicationConfigRepository _applicationConfigRepository;

        public Handler(
            ILogger<Handler> logger,
            IEnumerable<IPaymentGatewayManager> gatewayManagers,
            IPaymentApplicationConfigRepository applicationConfigRepository)
        {
            _logger = logger;
            _gatewayManagers = gatewayManagers;
            _applicationConfigRepository = applicationConfigRepository;
        }

        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            var newRow = command.TableRow;
            var gatewayTypeId = newRow.GatewayTypeId;

            var gatewayManager = _gatewayManagers.GetFromTypeIdOrNull(gatewayTypeId);

            if (gatewayManager == null)
            {
                //FIX: consider rather making this a Result.Fail and checking for it in the HttpTrigger
                throw new Exception($"The payment gateway type '{gatewayTypeId.Value}' is not supported");
            }

            var result = new Result();

            try
            {
                await _applicationConfigRepository.AddItemAsync(newRow, cancellationToken);

                result.IsNew = true;
                result.TableRow = newRow;
            }
            catch (CosmosException cosmosException) when (cosmosException.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                var existingTableRow = await _applicationConfigRepository.GetItemOrDefaultAsync(
                    newRow.ApplicationId,
                    gatewayTypeId,
                    cancellationToken);

                if (existingTableRow == null)
                {
                    _logger.LogError(
                        cosmosException,
                        "ClientAppConfig does not exist for applicationId: {ApplicationId} and gatewayTypeId: {GatewayTypeId}",
                        gatewayManager.TypeId.Value, newRow.ApplicationId);
                }

                result.IsNew = false;
                result.TableRow = existingTableRow;
            }

            return result;
        }
    }
}