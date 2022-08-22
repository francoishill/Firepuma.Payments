using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firepuma.Payments.Abstractions.Constants;
using Firepuma.Payments.Abstractions.DTOs.Responses;
using Firepuma.Payments.Abstractions.ValueObjects;
using Firepuma.Payments.FunctionApp.Commands;
using Firepuma.Payments.FunctionApp.PaymentGatewayAbstractions;
using Firepuma.Payments.Implementations.Config;
using Firepuma.Payments.Implementations.Factories;
using Firepuma.Payments.Implementations.TableStorage;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Firepuma.Payments.FunctionApp.Api.HttpFunctions;

public class PreparePayment
{
    private readonly ILogger<PreparePayment> _logger;
    private readonly ITableService<IPaymentApplicationConfig> _applicationConfigsTableService;
    private readonly IMediator _mediator;
    private readonly IEnumerable<IPaymentGateway> _gateways;

    public PreparePayment(
        ILogger<PreparePayment> logger,
        ITableService<IPaymentApplicationConfig> applicationConfigsTableService,
        IMediator mediator,
        IEnumerable<IPaymentGateway> gateways)
    {
        _logger = logger;
        _applicationConfigsTableService = applicationConfigsTableService;
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
            _logger.LogError("The payment gateway type \'{GatewayTypeId}\' is not supported", gatewayTypeId);
            return HttpResponseFactory.CreateBadRequestResponse($"The payment gateway type '{gatewayTypeId}' is not supported");
        }

        if (!gateway.Features.PreparePayment)
        {
            _logger.LogError("Payment gateway \'{GatewayTypeId}\' does not support feature PreparePayment", gatewayTypeId);
            return HttpResponseFactory.CreateBadRequestResponse($"Payment gateway '{gatewayTypeId}' does not support feature PreparePayment");
        }

        var requestAppSecret = req.Headers[PaymentHttpRequestHeaderKeys.APP_SECRET].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(requestAppSecret))
        {
            _logger.LogError($"A value is required for header {PaymentHttpRequestHeaderKeys.APP_SECRET}");
            return HttpResponseFactory.CreateBadRequestResponse($"A value is required for header {PaymentHttpRequestHeaderKeys.APP_SECRET}");
        }

        var applicationConfig = await gateway.GetApplicationConfigAsync(
            _applicationConfigsTableService,
            new ClientApplicationId(applicationId),
            cancellationToken);

        if (applicationConfig.ApplicationSecret != requestAppSecret)
        {
            _logger.LogError($"The application secret is invalid");
            return HttpResponseFactory.CreateBadRequestResponse($"The application secret is invalid");
        }

        var prepareRequest = await gateway.DeserializePrepareRequestAsync(req, cancellationToken);
        if (!prepareRequest.IsSuccessful)
        {
            _logger.LogError("{Reason}, {Errors}", prepareRequest.FailedReason.ToString(), string.Join(", ", prepareRequest.FailedErrors));
            return HttpResponseFactory.CreateBadRequestResponse($"{prepareRequest.FailedReason.ToString()}, {string.Join(", ", prepareRequest.FailedErrors)}");
        }

        var paymentId = prepareRequest.Result.PaymentId;

        var addCommand = new AddPayment.Command
        {
            GatewayTypeId = new PaymentGatewayTypeId(gatewayTypeId),
            ApplicationId = new ClientApplicationId(applicationId),
            ApplicationConfig = applicationConfig,
            PaymentId = paymentId,
            RequestDto = prepareRequest.Result.RequestDto,
        };

        var result = await _mediator.Send(addCommand, cancellationToken);

        if (!result.IsSuccessful)
        {
            _logger.LogError("{Reason}, {Errors}", result.FailedReason.ToString(), string.Join(", ", result.FailedErrors));
            return HttpResponseFactory.CreateBadRequestResponse($"{result.FailedReason.ToString()}, {string.Join(", ", result.FailedErrors)}");
        }

        var response = new PreparePaymentResponse(paymentId, result.RedirectUrl);
        return new OkObjectResult(response);
    }
}