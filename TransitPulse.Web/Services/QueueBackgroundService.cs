using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

namespace TransitPulse.Web.Services;

public class QueueBackgroundService : BackgroundService
{
    private readonly IServiceBusService _serviceBus;
    private readonly IMemoryCache _cache;

    public QueueBackgroundService(IServiceBusService serviceBus, IMemoryCache cache)
    {
        _serviceBus = serviceBus;
        _cache = cache;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var queues = await _serviceBus.GetQueues();
            
            _cache.Set("queues", queues);

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
