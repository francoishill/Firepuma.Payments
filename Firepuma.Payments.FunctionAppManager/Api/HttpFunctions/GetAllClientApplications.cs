using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Firepuma.Payments.Core.PaymentAppConfiguration.Entities;
using Firepuma.Payments.Core.PaymentAppConfiguration.ValueObjects;
using Firepuma.Payments.Core.Payments.ValueObjects;
using Firepuma.Payments.FunctionAppManager.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Local

namespace Firepuma.Payments.FunctionAppManager.Api.HttpFunctions;

public class GetAllClientApplications
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public GetAllClientApplications(
        IMediator mediator,
        IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
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

        var mappedResults = _mapper.Map<IEnumerable<ClientAppResponseDto>>(result);
        return new OkObjectResult(mappedResults);
    }

    [AutoMap(typeof(PaymentApplicationConfig))]
    private class ClientAppResponseDto
    {
        public ClientApplicationId ApplicationId { get; set; }
        public PaymentGatewayTypeId GatewayTypeId { get; set; }

        public string ApplicationSecret { get; set; }

        public Dictionary<string, object> ExtraValues { get; set; }
    }
}