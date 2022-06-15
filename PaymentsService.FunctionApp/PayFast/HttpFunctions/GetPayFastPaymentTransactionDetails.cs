using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace Firepuma.PaymentsService.FunctionApp.PayFast.HttpFunctions;

public static class GetPayFastPaymentTransactionDetails
{
    [FunctionName("GetPayFastPaymentTransactionDetails")]
    public static async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]
        HttpRequest req,
        ILogger log)
    {
        log.LogInformation("C# HTTP trigger function processed a request");

        var TODO = "";
        // Find single PayfastOnceOffPayment from table by TransactionId
        // Respond with the entity or null

        return new BadRequestObjectResult("Function is not yet implemented");
    }
}