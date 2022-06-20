using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Firepuma.PaymentsService.Implementations.Config;
using MediatR;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedType.Global
// ReSharper disable RedundantNameQualifier

namespace Firepuma.PaymentsService.FunctionAppManager.Commands;

public static class AddClientAppTableRecord
{
    public class Command : IRequest<Result>
    {
        public CloudTable CloudTable { get; set; }
        public ClientAppConfig TableRow { get; set; }

        public Command(
            CloudTable cloudTable,
            ClientAppConfig tableRow)
        {
            CloudTable = cloudTable;
            TableRow = tableRow;
        }
    }

    public class Result
    {
        public string TableName { get; set; }
        public bool IsNew { get; set; }
        public ClientAppConfig TableRow { get; set; }
    }


    public class Handler : IRequestHandler<Command, Result>
    {
        private readonly ILogger<Handler> _logger;

        public Handler(
            ILogger<Handler> logger)
        {
            _logger = logger;
        }

        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            var table = command.CloudTable;
            var newRow = command.TableRow;

            var result = new Result
            {
                TableName = table.Name,
            };

            try
            {
                await table.ExecuteAsync(TableOperation.Insert(newRow), cancellationToken);

                result.IsNew = true;
                result.TableRow = newRow;
            }
            catch (Microsoft.Azure.Cosmos.Table.StorageException storageException) when (storageException.RequestInformation.HttpStatusCode == (int)HttpStatusCode.Conflict)
            {
                var existingTableRow = await LoadClientAppConfig(
                    table,
                    newRow.PaymentProviderName,
                    newRow.ApplicationId,
                    cancellationToken);

                result.IsNew = false;
                result.TableRow = existingTableRow;
            }

            return result;
        }

        private async Task<ClientAppConfig> LoadClientAppConfig(
            CloudTable table,
            string paymentProviderName,
            string applicationId,
            CancellationToken cancellationToken)
        {
            var retrieveOperation = ClientAppConfig.GetRetrieveOperation(paymentProviderName, applicationId);
            var loadResult = await table.ExecuteAsync(retrieveOperation, cancellationToken);

            if (loadResult.Result == null)
            {
                _logger.LogError(
                    "loadResult.Result was null for paymentProviderName: {ProviderName} and applicationId: {ApplicationId}",
                    paymentProviderName, applicationId);
                return null;
            }

            return loadResult.Result as ClientAppConfig;
        }
    }
}