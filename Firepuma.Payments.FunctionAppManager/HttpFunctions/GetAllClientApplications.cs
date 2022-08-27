using System.Threading;
using System.Threading.Tasks;
using Firepuma.Payments.FunctionAppManager.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Local

namespace Firepuma.Payments.FunctionAppManager.HttpFunctions;

public class GetAllClientApplications
{
    private readonly IMediator _mediator;

    public GetAllClientApplications(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    [FunctionName("GetAllClientApplications")]
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
        ILogger log,
        CancellationToken cancellationToken)
    {
        log.LogInformation("C# HTTP trigger function processed a request");

        var query = new GetAllClientApps.Query();

        var result = await _mediator.Send(query, cancellationToken);
        //FIX: consider using each gatewayManager's implementation to map to the gateway's specific response DTO (instead of just using BasePaymentApplicationConfig)
        return new OkObjectResult(result);
        // var mappedResults = _mapper.Map<IEnumerable<ClientAppResponseDto>>(result);
        //
        // return new OkObjectResult(mappedResults);
    }

    // [AutoMap(typeof(PayFastClientAppConfig))]
    // private class ClientAppResponseDto
    // {
    //     public string GatewayTypeId { get; set; }
    //     public string ApplicationId { get; set; }
    //     public string ApplicationSecret { get; set; }
    //     public bool IsSandbox { get; set; }
    //     public string MerchantId { get; set; }
    //     public string MerchantKey { get; set; }
    //     public string PassPhrase { get; set; }
    //     public DateTimeOffset Timestamp { get; set; }
    // }
}