using System.Threading;
using System.Threading.Tasks;
using Firepuma.DatabaseRepositories.CosmosDb.Services;
using Firepuma.Payments.Infrastructure.Config;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

// ReSharper disable RedundantAnonymousTypePropertyName

namespace Firepuma.Payments.FunctionAppManager.Api.HttpFunctions;

public class EnsureCosmosContainersExist
{
    private readonly ICosmosDbAdminService _cosmosDbAdminService;

    public EnsureCosmosContainersExist(
        ICosmosDbAdminService cosmosDbAdminService)
    {
        _cosmosDbAdminService = cosmosDbAdminService;
    }

    [FunctionName("EnsureCosmosContainersExist")]
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
        ILogger log,
        CancellationToken cancellationToken)
    {
        log.LogInformation("C# HTTP trigger function processed a request");

        var containersToCreate = CosmosContainerConfiguration.AllContainers;

        var result = await _cosmosDbAdminService.CreateContainersIfNotExist(containersToCreate, cancellationToken);

        return new OkObjectResult(result);
    }
}