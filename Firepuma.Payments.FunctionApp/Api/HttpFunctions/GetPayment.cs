using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firepuma.Payments.Abstractions.Constants;
using Firepuma.Payments.Abstractions.DTOs.Responses;
using Firepuma.Payments.Abstractions.ValueObjects;
using Firepuma.Payments.FunctionApp.PaymentGatewayAbstractions;
using Firepuma.Payments.FunctionApp.Queries;
using Firepuma.Payments.Implementations.Config;
using Firepuma.Payments.Implementations.Factories;
using Firepuma.Payments.Implementations.TableStorage;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

// ReSharper disable RedundantAnonymousTypePropertyName

namespace Firepuma.Payments.FunctionApp.Api.HttpFunctions;

public class GetPayment
{
    private readonly ILogger<GetPayment> _logger;
    private readonly ITableProvider<IPaymentApplicationConfig> _applicationConfigsTableProvider;
    private readonly IMediator _mediator;
    private readonly IEnumerable<IPaymentGateway> _gateways;

    public GetPayment(
        ILogger<GetPayment> logger,
        ITableProvider<IPaymentApplicationConfig> applicationConfigsTableProvider,
        IMediator mediator,
        IEnumerable<IPaymentGateway> gateways)
    {
        _logger = logger;
        _applicationConfigsTableProvider = applicationConfigsTableProvider;
        _mediator = mediator;
        _gateways = gateways;
    }

    [FunctionName("GetPayment")]
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "GetPayment/{gatewayTypeId}/{applicationId}/{paymentId}")] HttpRequest req,
        ILogger log,
        string gatewayTypeId,
        string applicationId,
        string paymentId,
        CancellationToken cancellationToken)
    {
        log.LogInformation("C# HTTP trigger function processed a request with gatewayTypeId '{GatewayTypeId}' and applicationId '{ApplicationId}'", gatewayTypeId, applicationId);

        var gateway = _gateways.GetFromTypeIdOrNull(new PaymentGatewayTypeId(gatewayTypeId));

        if (gateway == null)
        {
            return HttpResponseFactory.CreateBadRequestResponse($"The payment gateway type '{gatewayTypeId}' is not supported");
        }

        if (!gateway.Features.PreparePayment)
        {
            return HttpResponseFactory.CreateBadRequestResponse($"Payment gateway '{gatewayTypeId}' does not support feature PreparePayment");
        }

        var requestAppSecret = req.Headers[PaymentHttpRequestHeaderKeys.APP_SECRET].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(requestAppSecret))
        {
            _logger.LogError($"A value is required for header {PaymentHttpRequestHeaderKeys.APP_SECRET}");
            return HttpResponseFactory.CreateBadRequestResponse($"A value is required for header {PaymentHttpRequestHeaderKeys.APP_SECRET}");
        }

        var applicationConfig = await gateway.GetApplicationConfigAsync(
            _applicationConfigsTableProvider,
            new ClientApplicationId(applicationId),
            cancellationToken);

        if (applicationConfig.ApplicationSecret != requestAppSecret)
        {
            _logger.LogError($"The application secret is invalid");
            return HttpResponseFactory.CreateBadRequestResponse($"The application secret is invalid");
        }

        var query = new GetPaymentDetails.Query
        {
            GatewayTypeId = new PaymentGatewayTypeId(gatewayTypeId),
            ApplicationId = new ClientApplicationId(applicationId),
            PaymentId = new PaymentId(paymentId),
        };

        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccessful)
        {
            return HttpResponseFactory.CreateBadRequestResponse($"{result.FailedReason.ToString()}, {string.Join(", ", result.FailedErrors)}");
        }

        var dto = new GetPaymentResponse
        {
            PaymentId = result.PaymentTableEntity.PaymentId,
            GatewayTypeId = result.PaymentTableEntity.GatewayTypeId,
            PaymentEntity = result.PaymentTableEntity,
        };

        return new OkObjectResult(dto);
    }
}