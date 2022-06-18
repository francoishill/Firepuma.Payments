using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Firepuma.PaymentsService.Abstractions.DTOs.Requests;
using Firepuma.PaymentsService.Abstractions.Infrastructure.Validation;
using Firepuma.PaymentsService.FunctionAppManager.Helpers;
using Firepuma.PaymentsService.FunctionAppManager.Services;
using Firepuma.PaymentsService.Implementations.Config;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

// ReSharper disable RedundantNameQualifier

namespace Firepuma.PaymentsService.FunctionAppManager.HttpFunctions;

public class CreatePayFastClientApplication
{
    private readonly IClientAppManagerService _clientAppManagerService;

    public CreatePayFastClientApplication(
        IClientAppManagerService clientAppManagerService)
    {
        _clientAppManagerService = clientAppManagerService;
    }

    [FunctionName("CreatePayFastClientApplication")]
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "CreatePayFastClientApplication/{applicationId}")]
        HttpRequest req,
        ILogger log,
        [Table("PaymentsConfigPerApplication")] CloudTable clientAppConfigTable,
        string applicationId,
        CancellationToken cancellationToken)
    {
        log.LogInformation("C# HTTP trigger function processed a request");

        var serviceBusConnectionString = EnvironmentVariableHelpers.GetRequiredEnvironmentVariable("ServiceBusConnectionString");

        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var requestDTO = JsonConvert.DeserializeObject<CreatePayFastClientApplicationRequest>(requestBody);

        if (requestDTO == null)
        {
            return CreateBadRequestResponse("Request body is required but empty");
        }

        if (!ValidationHelpers.ValidateDataAnnotations(requestDTO, out var validationResultsForRequest))
        {
            return CreateBadRequestResponse(new[] { "Request body is invalid" }.Concat(validationResultsForRequest.Select(s => s.ErrorMessage)).ToArray());
        }

        var responseMessages = new List<string>();

        var result = await _clientAppManagerService.CreateServiceBusQueueIfNotExists(
            serviceBusConnectionString,
            applicationId,
            cancellationToken);

        responseMessages.Add(
            result.IsNew
                ? $"Queue '{result.QueueName}' created with properties: {result.QueueProperties}"
                : $"Queue '{result.QueueName}' already existed with the following properties: {result.QueueProperties}");

        //TODO:
        //  * Expand to create:
        //    * [DONE] Queues
        //    * New Function Key/code to authenticate
        //    * New Shared access policies key for client app to listen on Service Bus
        //  * Add function to GetAllClientApplications
        //  * Add function to ...
        //  * Is there a native Azure way to authenticate the function calls (instead of 'code') and that can automatically derive the "Application Id" from the auth token?

        var newClientAppConfig = new ClientAppConfig(
            "PayFast",
            applicationId,
            requestDTO.IsSandbox,
            requestDTO.MerchantId,
            requestDTO.MerchantKey,
            requestDTO.PassPhrase);

        try
        {
            await clientAppConfigTable.ExecuteAsync(TableOperation.Insert(newClientAppConfig), cancellationToken);

            responseMessages.Add($"{clientAppConfigTable.Name} table record created");
        }
        catch (StorageException storageException) when (storageException.RequestInformation.HttpStatusCode == (int)HttpStatusCode.Conflict)
        {
            responseMessages.Add($"{clientAppConfigTable.Name} table record already existed, cannot overwrite existing");
        }

        return new OkObjectResult(responseMessages);
    }

    private static IActionResult CreateBadRequestResponse(params string[] errors)
    {
        return new BadRequestObjectResult(new Dictionary<string, object>
        {
            { "Errors", errors }
        });
    }
}