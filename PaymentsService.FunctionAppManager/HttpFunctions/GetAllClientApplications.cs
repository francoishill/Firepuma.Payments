using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Firepuma.PaymentsService.FunctionAppManager.Queries;
using Firepuma.PaymentsService.Implementations.Config;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging;

// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Local

namespace Firepuma.PaymentsService.FunctionAppManager.HttpFunctions;

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
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]
        HttpRequest req,
        ILogger log,
        [Table("PaymentsConfigPerApplication")] CloudTable clientAppConfigTable,
        CancellationToken cancellationToken)
    {
        log.LogInformation("C# HTTP trigger function processed a request");

        var query = new GetAllClientApps(clientAppConfigTable);

        var result = await _mediator.Send(query, cancellationToken);
        var mappedResults = _mapper.Map<IEnumerable<ClientAppResponseDto>>(result);

        return new OkObjectResult(mappedResults);
    }

    [AutoMap(typeof(ClientAppConfig))]
    private class ClientAppResponseDto
    {
        public string PaymentProviderName { get; set; }
        public string ApplicationId { get; set; }
        public string ApplicationSecret { get; set; }
        public bool IsSandbox { get; set; }
        public string MerchantId { get; set; }
        public string MerchantKey { get; set; }
        public string PassPhrase { get; set; }
        public DateTimeOffset Timestamp { get; set; }
    }
}