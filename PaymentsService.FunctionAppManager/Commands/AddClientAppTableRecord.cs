using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Firepuma.PaymentsService.FunctionAppManager.Commands.Results;
using Firepuma.PaymentsService.Implementations.Config;
using MediatR;
using Microsoft.Azure.Cosmos.Table;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedType.Global
// ReSharper disable RedundantNameQualifier

namespace Firepuma.PaymentsService.FunctionAppManager.Commands;

public class AddClientAppTableRecord : IRequest<object>
{
    public CloudTable CloudTable { get; set; }
    public ClientAppConfig TableRow { get; set; }

    public AddClientAppTableRecord(CloudTable cloudTable, ClientAppConfig tableRow)
    {
        CloudTable = cloudTable;
        TableRow = tableRow;
    }


    public class Handler : IRequestHandler<AddClientAppTableRecord, object>
    {
        public async Task<object> Handle(AddClientAppTableRecord command, CancellationToken cancellationToken)
        {
            var table = command.CloudTable;
            var newRow = command.TableRow;

            var addTableRecordResult = new AddTableRecordResult
            {
                TableName = table.Name,
                TableRow = newRow,
            };

            try
            {
                await table.ExecuteAsync(TableOperation.Insert(newRow), cancellationToken);

                addTableRecordResult.IsNew = true;
            }
            catch (Microsoft.Azure.Cosmos.Table.StorageException storageException) when (storageException.RequestInformation.HttpStatusCode == (int)HttpStatusCode.Conflict)
            {
                addTableRecordResult.IsNew = false;
            }

            return addTableRecordResult;
        }
    }
}