﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Firepuma.Payments.Core.ClientDtos.ClientResponses;
using Firepuma.Payments.Core.Constants;
using Firepuma.Payments.Core.PaymentAppConfiguration.Repositories;
using Firepuma.Payments.Core.PaymentAppConfiguration.ValueObjects;
using Firepuma.Payments.Core.Payments.Entities;
using Firepuma.Payments.Core.Payments.ValueObjects;
using Firepuma.Payments.FunctionApp.Gateways;
using Firepuma.Payments.FunctionApp.Queries;
using Firepuma.Payments.Infrastructure.HttpResponses;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

// ReSharper disable RedundantAnonymousTypePropertyName

namespace Firepuma.Payments.FunctionApp.Api.HttpFunctions;

public class GetPayment
{
    private readonly ILogger<GetPayment> _logger;
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    private readonly IEnumerable<IPaymentGateway> _gateways;
    private readonly IPaymentApplicationConfigRepository _applicationConfigRepository;

    public GetPayment(
        ILogger<GetPayment> logger,
        IMediator mediator,
        IMapper mapper,
        IEnumerable<IPaymentGateway> gateways,
        IPaymentApplicationConfigRepository applicationConfigRepository)
    {
        _logger = logger;
        _mediator = mediator;
        _mapper = mapper;
        _gateways = gateways;
        _applicationConfigRepository = applicationConfigRepository;
    }

    [FunctionName("GetPayment")]
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "GetPayment/{gatewayTypeId}/{applicationId}/{paymentId}")] HttpRequest req,
        ILogger log,
        string gatewayTypeId,
        string applicationId,
        string paymentId,
        CancellationToken cancellationToken)
    {
        log.LogInformation("C# HTTP trigger function processed a request with gatewayTypeId '{GatewayTypeId}' and applicationId '{ApplicationId}'", gatewayTypeId, applicationId);

        var gateway = _gateways.GetFromTypeIdOrNull(new PaymentGatewayTypeId(gatewayTypeId));

        if (gateway == null)
        {
            return HttpResponseFactory.CreateBadRequestResponse($"The payment gateway type '{gatewayTypeId}' is not supported");
        }

        var requestAppSecret = req.Headers[PaymentHttpRequestHeaderKeys.APP_SECRET].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(requestAppSecret))
        {
            _logger.LogError($"A value is required for header {PaymentHttpRequestHeaderKeys.APP_SECRET}");
            return HttpResponseFactory.CreateBadRequestResponse($"A value is required for header {PaymentHttpRequestHeaderKeys.APP_SECRET}");
        }

        var applicationConfig = await _applicationConfigRepository.GetItemOrDefaultAsync(
            new ClientApplicationId(applicationId),
            new PaymentGatewayTypeId(gatewayTypeId),
            cancellationToken);

        if (applicationConfig == null)
        {
            _logger.LogError("Unable to find applicationConfig for applicationId: {ApplicationId} and gatewayTypeId: {GatewayTypeId}", applicationId, gatewayTypeId);
            return HttpResponseFactory.CreateBadRequestResponse($"Unable to find applicationConfig for applicationId: {applicationId} and gatewayTypeId: {gatewayTypeId}");
        }

        if (applicationConfig.ApplicationSecret != requestAppSecret)
        {
            _logger.LogError($"The application secret is invalid");
            return HttpResponseFactory.CreateBadRequestResponse($"The application secret is invalid");
        }

        var query = new GetPaymentDetails.Query
        {
            GatewayTypeId = new PaymentGatewayTypeId(gatewayTypeId),
            ApplicationId = new ClientApplicationId(applicationId),
            PaymentId = new PaymentId(paymentId),
        };

        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccessful)
        {
            return HttpResponseFactory.CreateBadRequestResponse($"{result.FailedReason.ToString()}, {string.Join(", ", result.FailedErrors)}");
        }

        var dto = _mapper.Map<GetPaymentResponse>(result.PaymentEntity);

        return new OkObjectResult(dto);
    }

    // ReSharper disable once UnusedType.Local
    private class SplitPaymentMappingProfile : Profile
    {
        public SplitPaymentMappingProfile()
        {
            CreateMap<PaymentEntity, GetPaymentResponse>();
        }
    }
}