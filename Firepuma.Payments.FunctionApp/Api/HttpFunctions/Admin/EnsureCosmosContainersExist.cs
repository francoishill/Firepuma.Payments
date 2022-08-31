using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Firepuma.Payments.Infrastructure.CosmosDb;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

// ReSharper disable RedundantAnonymousTypePropertyName

namespace Firepuma.Payments.FunctionApp.Api.HttpFunctions.Admin;

public class EnsureCosmosContainersExist
{
    private readonly Database _cosmosDb;

    public EnsureCosmosContainersExist(
        Database cosmosDb)
    {
        _cosmosDb = cosmosDb;
    }

    [FunctionName("EnsureCosmosContainersExist")]
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Admin, "post", Route = null)] HttpRequest req,
        ILogger log,
        CancellationToken cancellationToken)
    {
        log.LogInformation("C# HTTP trigger function processed a request");

        var containersToCreate = new[]
        {
            new
            {
                ContainerProperties = new ContainerProperties(CosmosContainerNames.COMMAND_EXECUTIONS, "/TypeName"),
            },

            new
            {
                ContainerProperties = new ContainerProperties(CosmosContainerNames.PAYMENTS, "/ApplicationId"),
            },

            new
            {
                ContainerProperties = new ContainerProperties(CosmosContainerNames.NOTIFICATION_TRACES, "/ApplicationId"),
            },

            new
            {
                ContainerProperties = new ContainerProperties(CosmosContainerNames.APPLICATION_CONFIGS, "/ApplicationId"),
            },
        };

        var successfulContainers = new List<object>();
        var failedContainers = new List<object>();
        foreach (var container in containersToCreate)
        {
            log.LogDebug(
                "Creating container {Container} with PartitionKeyPath {PartitionKeyPath}",
                container.ContainerProperties.Id, container.ContainerProperties.PartitionKeyPath);

            try
            {
                await _cosmosDb.CreateContainerIfNotExistsAsync(
                    container.ContainerProperties,
                    cancellationToken: cancellationToken);

                log.LogInformation(
                    "Successfully created container {Container} with PartitionKeyPath {PartitionKeyPath}",
                    container.ContainerProperties.Id, container.ContainerProperties.PartitionKeyPath);

                successfulContainers.Add(new
                {
                    Container = container,
                });
            }
            catch (Exception exception)
            {
                log.LogError(
                    exception,
                    "Failed to create container {Container} with PartitionKeyPath {PartitionKeyPath}, error: {Error}, stack: {Stack}",
                    container.ContainerProperties.Id, container.ContainerProperties.PartitionKeyPath,
                    exception.Message, exception.StackTrace);

                failedContainers.Add(new
                {
                    Container = container,
                    Exception = exception,
                });
            }
        }

        var responseDto = new
        {
            failedCount = failedContainers.Count,
            successfulCount = successfulContainers.Count,

            failedContainers,
            successfulContainers,
        };

        return new OkObjectResult(responseDto);
    }
}