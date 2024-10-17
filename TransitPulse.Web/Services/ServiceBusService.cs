using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using System.Text.Json;
using TransitPulse.Web.Models;

namespace TransitPulse.Web.Services;

public class ServiceBusService : IServiceBusService
{
    private readonly ServiceBusAdministrationClient _adminClient;
    private readonly ServiceBusClient _client;

    private static readonly ServiceBusReceiverOptions _receiverOptions = new ServiceBusReceiverOptions()
    {
        Identifier = "transitpulse.web",
        ReceiveMode = ServiceBusReceiveMode.PeekLock
    };

    private static readonly JsonSerializerOptions _serializerOptions = new JsonSerializerOptions
    {
        WriteIndented = true,
    };

    public ServiceBusService(ServiceBusAdministrationClient adminClient, ServiceBusClient client)
    {
        _adminClient = adminClient;
        _client = client;
    }

    public async IAsyncEnumerable<QueueState> GetQueues()
    {
        await foreach (var queue in _adminClient.GetQueuesRuntimePropertiesAsync())
        {
            yield return new QueueState
            {
                Name = queue.Name,
                ActiveCount = queue.ActiveMessageCount,
                TotalCount = queue.TotalMessageCount
            };
        }
    }

    public async Task<string[]> GetMessages(string queueName, int count = 5)
    {
        ServiceBusReceiver? receiver = null;
        try
        {
            receiver = _client.CreateReceiver(queueName, _receiverOptions);

            var messages = await receiver.PeekMessagesAsync(count);

            var payloads = new List<string>();
            foreach (var message in messages)
            {
                var json = JsonSerializer.Serialize(message.Body, options: _serializerOptions);
                payloads.Add(json);

                await receiver.AbandonMessageAsync(message);
            }

            return [.. payloads];
        }
        finally
        {
            if (receiver is not null)
                await receiver.CloseAsync();
        }
    }
}
