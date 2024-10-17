using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using TransitPulse.Web.Models;

namespace TransitPulse.Web.Services;

public class ServiceBusService : IServiceBusService
{
    private readonly ServiceBusAdministrationClient _adminClient;
    private readonly ServiceBusClient _client;

    public ServiceBusService(ServiceBusAdministrationClient adminClient, ServiceBusClient client)
    {
        _adminClient = adminClient;
        _client = client;
    }

    public async Task<QueueState[]> GetQueues()
    {
        var queues = _adminClient.GetQueuesRuntimePropertiesAsync();
        var results = new List<QueueState>();
        await foreach (var queue in queues)
        {
            results.Add(new QueueState
            {
                Name = queue.Name,
                ActiveCount = queue.ActiveMessageCount,
                TotalCount = queue.TotalMessageCount
            });
        }
        return results.ToArray();
    }
}
