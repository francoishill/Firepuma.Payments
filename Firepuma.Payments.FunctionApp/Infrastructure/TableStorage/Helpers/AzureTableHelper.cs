using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;

namespace Firepuma.PaymentsService.FunctionApp.Infrastructure.TableStorage.Helpers;

public static class AzureTableHelper
{
    public static async Task<IEnumerable<T>> GetAllTableRecordsAsync<T>(
        CloudTable table,
        TableQuery<T> filter,
        CancellationToken cancellationToken)
        where T : TableEntity, new()
    {
        var rows = new List<T>();
        TableContinuationToken token = null;

        do
        {
            var queryResult = await table.ExecuteQuerySegmentedAsync(filter, token, cancellationToken);
            rows.AddRange(queryResult.Results);

            token = queryResult.ContinuationToken;
        } while (token != null);

        return rows;
    }

    public static async Task<T> GetSingleRecordOrNullAsync<T>(
        CloudTable table,
        string partitionKey,
        string rowKey,
        CancellationToken cancellationToken)
        where T : TableEntity, new()
    {
        var filter = new TableQuery<T>().Where(
            TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition(nameof(TableEntity.PartitionKey), QueryComparisons.Equal, partitionKey),
                TableOperators.And,
                TableQuery.GenerateFilterCondition(nameof(TableEntity.RowKey), QueryComparisons.Equal, rowKey)
            ));

        var queryResult = await table.ExecuteQuerySegmentedAsync(filter, null, cancellationToken);

        return queryResult.Results.SingleOrDefault();
    }
}