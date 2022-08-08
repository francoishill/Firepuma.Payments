using System.Net;
using Azure;
using Azure.Data.Tables;

namespace Firepuma.Payments.Implementations.TableStorage.Helpers;

public static class AzureTableHelper
{
    public static async Task<T> GetSingleRecordOrNullAsync<T>(
        TableClient table,
        string partitionKey,
        string rowKey,
        CancellationToken cancellationToken)
        where T : class, ITableEntity, new()
    {
        try
        {
            return await table.GetEntityAsync<T>(partitionKey, rowKey, cancellationToken: cancellationToken);
        }
        catch (RequestFailedException requestFailedException) when (requestFailedException.Status == (int)HttpStatusCode.NotFound)
        {
            return null;
        }
    }
}