using System.Net;
using AutoMapper;
using Firepuma.Payments.Core.Payments.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Sample.PaymentsClientApp.Simple.Controllers.Responses;

namespace Sample.PaymentsClientApp.Simple.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SamplePaymentsController : ControllerBase
{
    private readonly ILogger<SamplePaymentsController> _logger;
    private readonly Services.PaymentsService _paymentsService;
    private readonly IMapper _mapper;

    public SamplePaymentsController(
        ILogger<SamplePaymentsController> logger,
        Services.PaymentsService paymentsService,
        IMapper mapper)
    {
        _logger = logger;
        _paymentsService = paymentsService;
        _mapper = mapper;
    }

    [HttpPost("prepare-payfast-payment")]
    public async Task<ActionResult<PreparePayfastOnceOffPaymentResponse>> PreparePayfastPayment(
        CancellationToken cancellationToken)
    {
        var newPaymentId = PaymentId.GenerateNew();

        _logger.LogInformation("Preparing payment for new payment id '{Id}'", newPaymentId);

        var returnUrl = Url.Action(nameof(PaymentsReturnCallback), null, new { paymentId = newPaymentId }, Request.Scheme);
        var cancelUrl = Url.Action(nameof(PaymentsCancelledCallback), null, new { paymentId = newPaymentId }, Request.Scheme);

        if (string.IsNullOrWhiteSpace(returnUrl))
        {
            _logger.LogCritical("Unable to determine return url, it is empty");
            return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
        }

        if (string.IsNullOrWhiteSpace(cancelUrl))
        {
            _logger.LogCritical("Unable to determine cancel url, it is empty");
            return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
        }

        var preparedPaymentResult = await _paymentsService.PreparePayfastOnceOffPayment(
            newPaymentId,
            returnUrl,
            cancelUrl,
            cancellationToken);

        if (!preparedPaymentResult.IsSuccessful)
        {
            return new BadRequestObjectResult($"{preparedPaymentResult.FailedReason.ToString()}, {string.Join(", ", preparedPaymentResult.FailedErrors)}");
        }

        return _mapper.Map<PreparePayfastOnceOffPaymentResponse>(preparedPaymentResult.Result);
    }

    [HttpGet("payments-return-callback/{paymentId}")]
    public async Task<IActionResult> PaymentsReturnCallback(
        string paymentId,
        CancellationToken cancellationToken)
    {
        var paymentResult = await _paymentsService.GetPaymentDetails(paymentId, cancellationToken);
        return new OkObjectResult($"Thank you, your payment ID {paymentId} is being processed in the background. {JsonConvert.SerializeObject(paymentResult, new Newtonsoft.Json.Converters.StringEnumConverter())}");
    }

    [HttpGet("payments-cancelled-callback/{paymentId}")]
    public async Task<IActionResult> PaymentsCancelledCallback(
        string paymentId,
        CancellationToken cancellationToken)
    {
        var paymentResult = await _paymentsService.GetPaymentDetails(paymentId, cancellationToken);
        return new OkObjectResult($"Your payment ID {paymentId} has been cancelled. {JsonConvert.SerializeObject(paymentResult, new Newtonsoft.Json.Converters.StringEnumConverter())}");
    }
}