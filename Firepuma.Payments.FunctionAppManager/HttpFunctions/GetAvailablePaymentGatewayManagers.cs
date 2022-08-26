using System.Collections.Generic;
using System.Linq;
using Firepuma.Payments.FunctionAppManager.GatewayAbstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace Firepuma.Payments.FunctionAppManager.HttpFunctions;

public class GetAvailablePaymentGatewayManagers
{
    private readonly IEnumerable<IPaymentGatewayManager> _gatewayManagers;

    public GetAvailablePaymentGatewayManagers(
        IEnumerable<IPaymentGatewayManager> gatewayManagers)
    {
        _gatewayManagers = gatewayManagers;
    }

    [FunctionName("GetAvailablePaymentGatewayManagers")]
    public IActionResult RunAsync([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req, ILogger log)
    {
        log.LogInformation("C# HTTP trigger function processed a request");

        var gatewayNames = _gatewayManagers.Select(g => new
        {
            g.TypeId,
            g.DisplayName,
        });

        return new OkObjectResult(gatewayNames);
    }
}