using BackendEgitimiYeni.Data;
using Microsoft.EntityFrameworkCore;

namespace BackendEgitimiYeni.BackgroundServices;

public class SystemMonitorService : BackgroundService
{
    private readonly ILogger<SystemMonitorService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public SystemMonitorService(
        ILogger<SystemMonitorService> logger,
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "SystemMonitorService başlatıldı."
        );

        using var timer =
            new PeriodicTimer(
                TimeSpan.FromMinutes(1)
            );

        try
        {
            while (await timer.WaitForNextTickAsync(
                stoppingToken))
            {
                await CheckSystemAsync(
                    stoppingToken
                );
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation(
                "SystemMonitorService durduruldu."
            );
        }
    }

    private async Task CheckSystemAsync(
        CancellationToken cancellationToken)
    {
        await using var scope =
            _scopeFactory.CreateAsyncScope();

        var context =
            scope.ServiceProvider
                .GetRequiredService<AppDbContext>();

        var productCount =
            await context.Products.CountAsync(
                cancellationToken
            );

        _logger.LogInformation(
            "Background kontrol tamamlandı. " +
            "Ürün sayısı: {ProductCount}. " +
            "Zaman: {Time}",
            productCount,
            DateTimeOffset.Now
        );
    }
}