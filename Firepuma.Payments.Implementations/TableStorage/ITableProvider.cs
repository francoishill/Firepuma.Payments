using Azure.Data.Tables;

namespace Firepuma.Payments.Implementations.TableStorage;

public interface ITableProvider
{
    TableClient Table { get; }
}