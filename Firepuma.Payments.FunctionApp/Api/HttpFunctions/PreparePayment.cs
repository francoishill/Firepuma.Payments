using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firepuma.Payments.Abstractions.Constants;
using Firepuma.Payments.Abstractions.DTOs.Responses;
using Firepuma.Payments.Abstractions.ValueObjects;
using Firepuma.Payments.FunctionApp.Commands;
using Firepuma.Payments.FunctionApp.PaymentGatewayAbstractions;
using Firepuma.Payments.Implementations.Factories;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Firepuma.Payments.FunctionApp.Api.HttpFunctions;

public class PreparePayment
{
    private readonly IMediator _mediator;
    private readonly IEnumerable<IPaymentGateway> _gateways;

    public PreparePayment(
        IMediator mediator,
        IEnumerable<IPaymentGateway> gateways)
    {
        _mediator = mediator;
        _gateways = gateways;
    }

    [FunctionName("PreparePayment")]
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "PreparePayment/{gatewayTypeId}/{applicationId}")] HttpRequest req,
        string gatewayTypeId,
        string applicationId,
        ILogger log,
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
            return HttpResponseFactory.CreateBadRequestResponse($"A value is required for header {PaymentHttpRequestHeaderKeys.APP_SECRET}");
        }

        var prepareRequest = await gateway.DeserializePrepareRequestAsync(req, cancellationToken);
        if (!prepareRequest.IsSuccessful)
        {
            return HttpResponseFactory.CreateBadRequestResponse($"{prepareRequest.FailedReason.ToString()}, {string.Join(", ", prepareRequest.FailedErrors)}");
        }

        var paymentId = prepareRequest.Result.PaymentId;

        var addCommand = new AddPayment.Command
        {
            GatewayTypeId = new PaymentGatewayTypeId(gatewayTypeId),
            ApplicationId = new ClientApplicationId(applicationId),
            ApplicationSecret = requestAppSecret,
            PaymentId = paymentId,
            RequestDto = prepareRequest.Result.RequestDto,
        };

        var result = await _mediator.Send(addCommand, cancellationToken);

        if (!result.IsSuccessful)
        {
            return HttpResponseFactory.CreateBadRequestResponse($"{result.FailedReason.ToString()}, {string.Join(", ", result.FailedErrors)}");
        }

        var response = new PreparePaymentResponse(paymentId, result.RedirectUrl);
        return new OkObjectResult(response);
    }
}