using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Firepuma.Payments.Implementations.Config;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

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
                ThroughPut = ThroughputProperties.CreateAutoscaleThroughput(4000),
            },

            new
            {
                ContainerProperties = new ContainerProperties(CosmosContainerNames.PAYMENTS, "/ApplicationId"),
                ThroughPut = ThroughputProperties.CreateAutoscaleThroughput(4000),
            },

            new
            {
                ContainerProperties = new ContainerProperties(CosmosContainerNames.NOTIFICATION_TRACES, "/ApplicationId"),
                ThroughPut = ThroughputProperties.CreateAutoscaleThroughput(4000),
            },

            new
            {
                ContainerProperties = new ContainerProperties(CosmosContainerNames.APPLICATION_CONFIGS, "/ApplicationId"),
                ThroughPut = ThroughputProperties.CreateAutoscaleThroughput(4000),
            },
        };

        var successfulContainers = new List<object>();
        var failedContainers = new List<object>();
        foreach (var container in containersToCreate)
        {
            log.LogDebug(
                "Creating container {Container} with PartitionKeyPath {PartitionKeyPath} and throughput {Throughput}",
                container.ContainerProperties.Id, container.ContainerProperties.PartitionKeyPath, JsonConvert.SerializeObject(container.ThroughPut.ToString()));

            try
            {
                await _cosmosDb.CreateContainerIfNotExistsAsync(
                    container.ContainerProperties,
                    container.ThroughPut,
                    requestOptions: null,
                    cancellationToken);

                log.LogInformation(
                    "Successfully created container {Container} with PartitionKeyPath {PartitionKeyPath} and throughput {Throughput}",
                    container.ContainerProperties.Id, container.ContainerProperties.PartitionKeyPath, JsonConvert.SerializeObject(container.ThroughPut.ToString()));

                successfulContainers.Add(new
                {
                    Container = container,
                });
            }
            catch (Exception exception)
            {
                log.LogError(
                    exception,
                    "Failed to create container {Container} with PartitionKeyPath {PartitionKeyPath} and throughput {Throughput}, error: {Error}, stack: {Stack}",
                    container.ContainerProperties.Id, container.ContainerProperties.PartitionKeyPath, JsonConvert.SerializeObject(container.ThroughPut.ToString()),
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