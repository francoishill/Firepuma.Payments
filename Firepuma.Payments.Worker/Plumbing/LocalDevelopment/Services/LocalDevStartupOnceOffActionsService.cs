using Firepuma.DatabaseRepositories.MongoDb.Abstractions.Indexes;

namespace Firepuma.Payments.Worker.Plumbing.LocalDevelopment.Services;

public class LocalDevStartupOnceOffActionsService : BackgroundService
{
    private readonly ILogger<LocalDevStartupOnceOffActionsService> _logger;
    private readonly IMongoIndexesApplier _mongoIndexesApplier;

    public LocalDevStartupOnceOffActionsService(
        ILogger<LocalDevStartupOnceOffActionsService> logger,
        IMongoIndexesApplier mongoIndexesApplier)
    {
        _logger = logger;
        _mongoIndexesApplier = mongoIndexesApplier;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Executing local development once-off actions on startup");

        await _mongoIndexesApplier.ApplyAllIndexes(stoppingToken);
    }
}