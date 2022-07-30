using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Firepuma.PaymentsService.Abstractions.Constants;
using Firepuma.PaymentsService.Abstractions.DTOs.Requests;
using Firepuma.PaymentsService.Abstractions.DTOs.Responses;
using Firepuma.PaymentsService.Abstractions.Infrastructure.Validation;
using Firepuma.PaymentsService.FunctionApp.PayFast.Commands;
using Firepuma.PaymentsService.Implementations.Factories;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Firepuma.PaymentsService.FunctionApp.PayFast.HttpFunctions;

public class PreparePayFastOnceOffPayment
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public PreparePayFastOnceOffPayment(
        IMediator mediator,
        IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    [FunctionName("PreparePayFastOnceOffPayment")]
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "PreparePayFastOnceOffPayment/{applicationId}")] HttpRequest req,
        ILogger log,
        string applicationId,
        CancellationToken cancellationToken)
    {
        log.LogInformation("C# HTTP trigger function processed a request");

        var requestAppSecret = req.Headers[PaymentHttpRequestHeaderKeys.APP_SECRET].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(requestAppSecret))
        {
            return HttpResponseFactory.CreateBadRequestResponse($"A value is required for header {PaymentHttpRequestHeaderKeys.APP_SECRET}");
        }

        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var requestDTO = JsonConvert.DeserializeObject<PreparePayFastOnceOffPaymentRequest>(requestBody);

        if (requestDTO == null)
        {
            return HttpResponseFactory.CreateBadRequestResponse("Request body is required but empty");
        }

        if (!ValidationHelpers.ValidateDataAnnotations(requestDTO, out var validationResultsForRequest))
        {
            return HttpResponseFactory.CreateBadRequestResponse(new[] { "Request body is invalid" }.Concat(validationResultsForRequest.Select(s => s.ErrorMessage)).ToArray());
        }

        if (requestDTO.SplitPayment != null)
        {
            if (!ValidationHelpers.ValidateDataAnnotations(requestDTO.SplitPayment, out var validationResultsForSplitPayment))
            {
                return HttpResponseFactory.CreateBadRequestResponse(new[] { "SplitPayment is invalid" }.Concat(validationResultsForSplitPayment.Select(s => s.ErrorMessage)).ToArray());
            }
        }

        var paymentId = requestDTO.PaymentId;

        var mappedCommandSplitPaymentConfig = _mapper.Map<AddPayFastOnceOffPayment.Command.SplitPaymentConfig>(requestDTO.SplitPayment);

        var addCommand = new AddPayFastOnceOffPayment.Command
        {
            ApplicationSecret = requestAppSecret,
            ApplicationId = applicationId,
            PaymentId = paymentId,
            BuyerEmailAddress = requestDTO.BuyerEmailAddress,
            BuyerFirstName = requestDTO.BuyerFirstName,
            ImmediateAmountInRands = requestDTO.ImmediateAmountInRands ?? throw new ArgumentNullException(nameof(requestDTO.ImmediateAmountInRands)),
            ItemName = requestDTO.ItemName,
            ItemDescription = requestDTO.ItemDescription,
            ReturnUrl = requestDTO.ReturnUrl,
            CancelUrl = requestDTO.CancelUrl,
            SplitPayment = mappedCommandSplitPaymentConfig,
        };

        var result = await _mediator.Send(addCommand, cancellationToken);

        if (!result.IsSuccessful)
        {
            return HttpResponseFactory.CreateBadRequestResponse($"{result.FailedReason.ToString()}, {string.Join(", ", result.FailedErrors)}");
        }


        var response = new PreparePayFastOnceOffPaymentResponse(paymentId, result.RedirectUrl);
        return new OkObjectResult(response);
    }

    // ReSharper disable once UnusedType.Local
    private class SplitPaymentMappingProfile : Profile
    {
        public SplitPaymentMappingProfile()
        {
            CreateMap<PreparePayFastOnceOffPaymentRequest.SplitPaymentConfig, AddPayFastOnceOffPayment.Command.SplitPaymentConfig>();
        }
    }
}