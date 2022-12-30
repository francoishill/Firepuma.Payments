using System.Text.Json;
using AutoMapper;
using Firepuma.Payments.Domain.Notifications.Commands;
using Firepuma.Payments.Domain.Payments.Abstractions;
using Firepuma.Payments.Domain.Payments.Commands;
using Firepuma.Payments.Domain.Payments.Extensions;
using Firepuma.Payments.Domain.Payments.ValueObjects;
using Firepuma.Payments.Infrastructure.Gateways.PayFast;
using Firepuma.Payments.Infrastructure.Payments;
using Firepuma.Payments.Infrastructure.Plumbing.CommandHandling;
using Firepuma.Payments.Infrastructure.Plumbing.IntegrationEvents;
using Firepuma.Payments.Infrastructure.Plumbing.MongoDb;
using Firepuma.Payments.WebHooks.Plumbing.Extensions;
using Google.Cloud.Diagnostics.Common;
using MediatR;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;

// ReSharper disable RedundantNameQualifier

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAutoMapper(
    typeof(Firepuma.Payments.Infrastructure.Payments.ServiceCollectionExtensions),
    typeof(Firepuma.Payments.Domain.Payments.ValueObjects.PaymentId));

var mongoDbConfigSection = builder.Configuration.GetSection("MongoDb");
builder.Services.AddMongoDbRepositories(mongoDbConfigSection, builder.Environment.IsDevelopment(), out var mongoDbOptions);

var assembliesWithCommandHandlers = new[]
{
    typeof(SendEmailCommand).Assembly,
}.Distinct().ToArray();

builder.Services.AddCommandsAndQueriesFunctionality(
    mongoDbOptions.AuthorizationFailuresCollectionName,
    mongoDbOptions.CommandExecutionsCollectionName,
    assembliesWithCommandHandlers);

var integrationEventsConfigSection = builder.Configuration.GetSection("IntegrationEvents");
builder.Services.AddIntegrationEvents(integrationEventsConfigSection);

builder.Services.AddPaymentsFeature(
    mongoDbOptions.AppConfigurationsCollectionName,
    mongoDbOptions.PaymentsCollectionName,
    mongoDbOptions.NotificationTracesCollectionName);

builder.Services.AddPayFastFeature();

if (!builder.Environment.IsDevelopment())
{
    builder.Logging.ClearProviders();
    builder.Logging.AddGoogle(new LoggingServiceOptions
    {
        ProjectId = null, // leave null because it is running in Google Cloud when in non-Development mode
        Options = LoggingOptions.Create(
            LogLevel.Trace,
            retryOptions: RetryOptions.Retry(ExceptionHandling.Propagate),
            bufferOptions: BufferOptions.NoBuffer() //refer to https://github.com/googleapis/google-cloud-dotnet/pull/7025
        ),
    });
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    var autoMapper = app.Services.GetRequiredService<IMapper>();
    autoMapper.ConfigurationProvider.AssertConfigurationIsValid(); // this is expensive on startup, so only do it in Dev environment, unit tests will fail before reaching PROD
}

app.MapPost("/api/IncomingPaymentNotificationWebhook/{applicationId}/{gatewayTypeId}",
    async (
        [FromServices] IEnumerable<IPaymentGateway> gateways,
        [FromServices] IApplicationConfigProvider applicationConfigProvider,
        [FromServices] IMediator mediator,
        [FromRoute] ClientApplicationId applicationId,
        [FromRoute] PaymentGatewayTypeId gatewayTypeId,
        HttpRequest request,
        CancellationToken cancellationToken) =>
    {
        var logger = app.Logger;
        var gateway = gateways.GetFromTypeIdOrNull(gatewayTypeId);

        if (gateway == null)
        {
            logger.LogError("The payment gateway type \'{GatewayTypeId}\' is not supported", gatewayTypeId);
            return Results.BadRequest($"The payment gateway type '{gatewayTypeId}' is not supported");
        }

        var applicationConfig = await applicationConfigProvider.GetApplicationConfigAsync(
            applicationId,
            gatewayTypeId,
            cancellationToken);

        var remoteIp = request.ExtractRemoteIp(logger);
        if (remoteIp == null)
        {
            logger.LogCritical("The remote ip is required but null");
            return Results.BadRequest("The remote ip is required but null");
        }

        var paymentNotificationRequest = await gateway.DeserializePaymentNotificationRequestAsync(request, cancellationToken);

        logger.LogInformation("Validating PaymentNotification with payload {Payload}", JsonSerializer.Serialize(paymentNotificationRequest.PaymentNotificationPayload));

        var enqueuePaymentNotificationCommand = new ValidatePaymentNotificationCommand.Payload
        {
            CorrelationId = request.HttpContext.TraceIdentifier,
            GatewayTypeId = gatewayTypeId,
            ApplicationId = applicationId,
            ApplicationConfig = applicationConfig,
            PaymentNotificationPayload = paymentNotificationRequest.PaymentNotificationPayload,
            RemoteIp = remoteIp.ToString(),
            IncomingRequestUri = request.GetDisplayUrl(),
        };

        await mediator.Send(enqueuePaymentNotificationCommand, cancellationToken);

        return Results.Ok();
    });

var port = Environment.GetEnvironmentVariable("PORT");
if (port != null)
{
    app.Urls.Add($"http://0.0.0.0:{port}");
}

app.Run();