using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Firepuma.Payments.Abstractions.Constants;
using Firepuma.Payments.Abstractions.DTOs.Responses;
using Firepuma.Payments.Abstractions.ValueObjects;
using Firepuma.Payments.FunctionApp.PayFast.Queries;
using Firepuma.Payments.FunctionApp.PayFast.TableModels;
using Firepuma.Payments.Implementations.Factories;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace Firepuma.Payments.FunctionApp.PayFast.Api.HttpFunctions;

public class GetPayFastPaymentTransactionDetails
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public GetPayFastPaymentTransactionDetails(
        IMediator mediator,
        IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    [FunctionName("GetPayFastPaymentTransactionDetails")]
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "GetPayFastPaymentTransactionDetails/{applicationId}/{paymentId}")] HttpRequest req,
        ILogger log,
        string applicationId,
        string paymentId,
        CancellationToken cancellationToken)
    {
        log.LogInformation("C# HTTP trigger function processed a request");

        var requestAppSecret = req.Headers[PaymentHttpRequestHeaderKeys.APP_SECRET].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(requestAppSecret))
        {
            return HttpResponseFactory.CreateBadRequestResponse($"A value is required for header {PaymentHttpRequestHeaderKeys.APP_SECRET}");
        }

        var query = new GetPayFastOnceOffPayment.Query
        {
            ApplicationId = new ClientApplicationId(applicationId),
            ApplicationSecret = requestAppSecret,
            PaymentId = new PaymentId(paymentId),
        };

        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccessful)
        {
            return HttpResponseFactory.CreateBadRequestResponse($"{result.FailedReason.ToString()}, {string.Join(", ", result.FailedErrors)}");
        }

        var dto = _mapper.Map<PayFastOnceOffPaymentResponse>(result.OnceOffPayment);

        return new OkObjectResult(dto);
    }

    // ReSharper disable once UnusedType.Local
    private class PayFastOnceOffPaymentMappingProfile : Profile
    {
        public PayFastOnceOffPaymentMappingProfile()
        {
            CreateMap<PayFastOnceOffPayment, PayFastOnceOffPaymentResponse>();
        }
    }
}