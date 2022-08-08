using Azure;
using Azure.Data.Tables;

namespace Firepuma.Payments.Implementations.TableStorage;

public class BaseAzureTable : ITableEntity
{
    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}