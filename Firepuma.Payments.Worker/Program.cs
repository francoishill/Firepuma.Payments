using AutoMapper;
using Firepuma.Payments.Domain.Notifications.Commands;
using Firepuma.Payments.Infrastructure.Plumbing.CommandHandling;
using Firepuma.Payments.Infrastructure.Plumbing.IntegrationEvents;
using Firepuma.Payments.Infrastructure.Plumbing.MongoDb;
using Firepuma.Payments.Worker.Admin.Controllers;
using Firepuma.Payments.Worker.Plumbing.LocalDevelopment;
using Firepuma.Payments.Worker.Plumbing.Middleware;
using Google.Cloud.Diagnostics.Common;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddInvalidModelStateLogging();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAutoMapper(typeof(ManualHealthCheckingController));

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

// builder.Services.AddPaymentsFeature(
//     mongoDbOptions.PaymentsCollectionName);

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