using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Firepuma.PaymentsService.Abstractions.Constants;
using Firepuma.PaymentsService.Abstractions.DTOs.Responses;
using Firepuma.PaymentsService.FunctionApp.PayFast.Queries;
using Firepuma.PaymentsService.FunctionApp.PayFast.TableModels;
using Firepuma.PaymentsService.Implementations.Factories;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace Firepuma.PaymentsService.FunctionApp.PayFast.Api.HttpFunctions;

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
            ApplicationId = applicationId,
            ApplicationSecret = requestAppSecret,
            PaymentId = paymentId,
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