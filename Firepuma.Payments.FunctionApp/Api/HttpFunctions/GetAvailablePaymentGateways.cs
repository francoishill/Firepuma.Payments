using System.Collections.Generic;
using System.Linq;
using Firepuma.Payments.FunctionApp.Gateways;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace Firepuma.Payments.FunctionApp.Api.HttpFunctions;

public class GetAvailablePaymentGateways
{
    private readonly IEnumerable<IPaymentGateway> _gateways;

    public GetAvailablePaymentGateways(
        IEnumerable<IPaymentGateway> gateways)
    {
        _gateways = gateways;
    }

    [FunctionName("GetAvailablePaymentGateways")]
    public IActionResult RunAsync([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req, ILogger log)
    {
        log.LogInformation("C# HTTP trigger function processed a request");

        var gatewayNames = _gateways.Select(g => new
        {
            g.TypeId,
            g.DisplayName,
            g.Features,
        });

        return new OkObjectResult(gatewayNames);
    }
}