using A12.Infrastructure.Data;

namespace A12.SchedulerWorker;

public class Worker(ILogger<Worker> logger, IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<A12DbContext>();
            var canConnect = await db.Database.CanConnectAsync(stoppingToken);
            logger.LogInformation("SQL Server connection {Status}", canConnect ? "succeeded" : "failed");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "SQL Server connection failed");
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }
            await Task.Delay(1000, stoppingToken);
        }
    }
}
