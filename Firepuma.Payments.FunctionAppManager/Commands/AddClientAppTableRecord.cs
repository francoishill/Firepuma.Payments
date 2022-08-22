using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Firepuma.Payments.Abstractions.ValueObjects;
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
        public PayFastClientAppConfig TableRow { get; set; }

        public Command(
            PayFastClientAppConfig tableRow)
        {
            TableRow = tableRow;
        }
    }

    public class Result
    {
        public string TableName { get; set; }
        public bool IsNew { get; set; }
        public PayFastClientAppConfig TableRow { get; set; }
    }


    public class Handler : IRequestHandler<Command, Result>
    {
        private readonly ILogger<Handler> _logger;
        private readonly ITableProvider<IPaymentApplicationConfig> _applicationConfigsTableProvider;

        public Handler(
            ILogger<Handler> logger,
            ITableProvider<IPaymentApplicationConfig> applicationConfigsTableProvider)
        {
            _logger = logger;
            _applicationConfigsTableProvider = applicationConfigsTableProvider;
        }

        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            var newRow = command.TableRow;

            var result = new Result
            {
                TableName = _applicationConfigsTableProvider.TableName,
            };

            try
            {
                await _applicationConfigsTableProvider.AddEntityAsync(newRow, cancellationToken);

                result.IsNew = true;
                result.TableRow = newRow;
            }
            catch (RequestFailedException requestFailedException) when (requestFailedException.Status == (int)HttpStatusCode.Conflict)
            {
                var existingTableRow = await LoadClientAppConfig(
                    newRow.GatewayTypeId,
                    newRow.ApplicationId,
                    cancellationToken);

                result.IsNew = false;
                result.TableRow = existingTableRow;
            }

            return result;
        }

        private async Task<PayFastClientAppConfig> LoadClientAppConfig(
            PaymentGatewayTypeId gatewayTypeId,
            ClientApplicationId applicationId,
            CancellationToken cancellationToken)
        {
            try
            {
                return await _applicationConfigsTableProvider.GetEntityAsync<PayFastClientAppConfig>(gatewayTypeId.Value, applicationId.Value, cancellationToken: cancellationToken);
            }
            catch (RequestFailedException requestFailedException) when (requestFailedException.Status == (int)HttpStatusCode.NotFound)
            {
                _logger.LogError(
                    requestFailedException,
                    "ClientAppConfig does not exist for gatewayTypeId: {GatewayTypeId} and applicationId: {ApplicationId}",
                    gatewayTypeId, applicationId);
                return null;
            }
        }
    }
}