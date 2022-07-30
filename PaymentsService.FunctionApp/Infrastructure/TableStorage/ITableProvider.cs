using Microsoft.Azure.Cosmos.Table;

namespace Firepuma.PaymentsService.FunctionApp.Infrastructure.TableStorage;

public interface ITableProvider
{
    CloudTable Table { get; }
}