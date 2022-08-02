using Microsoft.Azure.Cosmos.Table;

namespace Firepuma.Payments.FunctionApp.Infrastructure.TableStorage;

public interface ITableProvider
{
    CloudTable Table { get; }
}