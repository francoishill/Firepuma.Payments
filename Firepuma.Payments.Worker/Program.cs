using AutoMapper;
using Firepuma.Payments.Domain.Payments.Commands;
using Firepuma.Payments.Infrastructure.Admin;
using Firepuma.Payments.Infrastructure.Gateways.PayFast;
using Firepuma.Payments.Infrastructure.Payments;
using Firepuma.Payments.Infrastructure.Plumbing.CommandHandling;
using Firepuma.Payments.Infrastructure.Plumbing.GoogleLogging;
using Firepuma.Payments.Infrastructure.Plumbing.IntegrationEvents;
using Firepuma.Payments.Infrastructure.Plumbing.MongoDb;
using Firepuma.Payments.Worker.Admin.Controllers;
using Firepuma.Payments.Worker.Plumbing.LocalDevelopment;
using Firepuma.Payments.Worker.Plumbing.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddInvalidModelStateLogging();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAutoMapper(
    typeof(ManualHealthCheckingController),
    typeof(Firepuma.Payments.Infrastructure.Payments.ServiceCollectionExtensions),
    typeof(Firepuma.Payments.Domain.Payments.ValueObjects.PaymentId));

var mongoDbConfigSection = builder.Configuration.GetSection("MongoDb");
builder.Services.AddMongoDbRepositories(mongoDbConfigSection, builder.Environment.IsDevelopment(), out var mongoDbOptions);

var assembliesWithCommandHandlers = new[]
{
    typeof(AddPaymentCommand).Assembly,
}.Distinct().ToArray();

builder.Services.AddCommandsAndQueriesFunctionality(
    mongoDbOptions.AuthorizationFailuresCollectionName,
    mongoDbOptions.CommandExecutionsCollectionName,
    assembliesWithCommandHandlers);

var integrationEventsConfigSection = builder.Configuration.GetSection("IntegrationEvents");
builder.Services.AddIntegrationEvents(integrationEventsConfigSection);

var adminConfigSection = builder.Configuration.GetSection("Admin");
builder.Services.AddAdminFeature(adminConfigSection);

var paymentWebhookUrlsConfigSection = builder.Configuration.GetSection("PaymentWebhookUrls");
builder.Services.AddPaymentsFeature(
    mongoDbOptions.AppConfigurationsCollectionName,
    mongoDbOptions.PaymentsCollectionName,
    mongoDbOptions.NotificationTracesCollectionName);
builder.Services.AddPaymentWebhookUrlGeneration(paymentWebhookUrlsConfigSection);

builder.Services.AddPayFastFeature();

var googleLoggingConfigSection = builder.Configuration.GetSection("Logging:GoogleLogging");
builder.Logging.AddCustomGoogleLogging(googleLoggingConfigSection);

if (builder.Environment.IsDevelopment())
{
    var localDevelopmentOptionsConfigSection = builder.Configuration.GetSection("LocalDevelopment");
    builder.Services.AddLocalDevelopmentServices(localDevelopmentOptionsConfigSection);
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    var autoMapper = app.Services.GetRequiredService<IMapper>();
    autoMapper.ConfigurationProvider.AssertConfigurationIsValid(); // this is expensive on startup, so only do it in Dev environment, unit tests will fail before reaching PROD
}

// app.UseHttpsRedirection(); // this is not necessary in Google Cloud Run, they enforce HTTPs for external connections but the app in the container runs on HTTP

app.UseAuthorization();

app.MapControllers();

var port = Environment.GetEnvironmentVariable("PORT");
if (port != null)
{
    app.Urls.Add($"http://0.0.0.0:{port}");
}

app.Run();