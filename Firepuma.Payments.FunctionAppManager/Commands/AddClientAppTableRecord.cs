using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Firepuma.Payments.Abstractions.ValueObjects;
using Firepuma.Payments.FunctionAppManager.GatewayAbstractions;
using Firepuma.Payments.Implementations.Config;
using Firepuma.Payments.Implementations.TableStorage;
using MediatR;
using Microsoft.Extensions.Logging;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedType.Global
// ReSharper disable RedundantNameQualifier

namespace Firepuma.Payments.FunctionAppManager.Commands;

public static class AddClientAppTableRecord
{
    public class Command : IRequest<Result>
    {
        public BasePaymentApplicationConfig TableRow { get; set; }

        public Command(
            BasePaymentApplicationConfig tableRow)
        {
            TableRow = tableRow;
        }
    }

    public class Result
    {
        public string TableName { get; set; }
        public bool IsNew { get; set; }
        public BasePaymentApplicationConfig TableRow { get; set; }
    }


    public class Handler : IRequestHandler<Command, Result>
    {
        private readonly ILogger<Handler> _logger;
        private readonly IEnumerable<IPaymentGatewayManager> _gatewayManagers;
        private readonly ITableService<BasePaymentApplicationConfig> _applicationConfigsTableService;

        public Handler(
            ILogger<Handler> logger,
            IEnumerable<IPaymentGatewayManager> gatewayManagers,
            ITableService<BasePaymentApplicationConfig> applicationConfigsTableService)
        {
            _logger = logger;
            _gatewayManagers = gatewayManagers;
            _applicationConfigsTableService = applicationConfigsTableService;
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

            var result = new Result
            {
                TableName = _applicationConfigsTableService.TableName,
            };

            try
            {
                await _applicationConfigsTableService.AddEntityAsync(newRow, cancellationToken);

                result.IsNew = true;
                result.TableRow = newRow;
            }
            catch (RequestFailedException requestFailedException) when (requestFailedException.Status == (int)HttpStatusCode.Conflict)
            {
                var existingTableRow = await LoadClientAppConfig(
                    gatewayManager,
                    newRow.ApplicationId,
                    cancellationToken);

                result.IsNew = false;
                result.TableRow = existingTableRow;
            }

            return result;
        }

        private async Task<BasePaymentApplicationConfig> LoadClientAppConfig(
            IPaymentGatewayManager gatewayManager,
            ClientApplicationId applicationId,
            CancellationToken cancellationToken)
        {
            try
            {
                return await gatewayManager.GetApplicationConfigAsync(applicationId, cancellationToken);
            }
            catch (RequestFailedException requestFailedException) when (requestFailedException.Status == (int)HttpStatusCode.NotFound)
            {
                _logger.LogError(
                    requestFailedException,
                    "ClientAppConfig does not exist for gatewayTypeId: {GatewayTypeId} and applicationId: {ApplicationId}",
                    gatewayManager.TypeId.Value, applicationId);
                return null;
            }
        }
    }
}