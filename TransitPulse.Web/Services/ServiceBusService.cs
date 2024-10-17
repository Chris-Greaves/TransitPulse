﻿using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using TransitPulse.Web.Models;

namespace TransitPulse.Web.Services;

public class ServiceBusService : IServiceBusService
{
    private readonly ILogger<ServiceBusService> _logger;
    private readonly ServiceBusAdministrationClient _adminClient;
    private readonly ServiceBusClient _client;

    private static readonly ServiceBusReceiverOptions _receiverOptions = new()
    {
        Identifier = "transitpulse.web"
    };

    public ServiceBusService(ILogger<ServiceBusService> logger, ServiceBusAdministrationClient adminClient, ServiceBusClient client)
    {
        _logger = logger;
        _adminClient = adminClient;
        _client = client;
    }

    public async Task<List<QueueState>> GetQueues()
    {
        var pager = _adminClient.GetQueuesRuntimePropertiesAsync();
        var page = await pager.AsPages().FirstAsync();

        var items = new List<QueueState>();
        foreach (var queue in page.Values)
        {
            items.Add(new QueueState
            {
                Name = queue.Name,
                ActiveCount = queue.ActiveMessageCount,
                TotalCount = queue.TotalMessageCount
            });
        }

        return items;
    }

    public async Task<QueueState> GetQueue(string queueName)
    {
        var queue = await _adminClient.GetQueueRuntimePropertiesAsync(queueName);

        return new QueueState
        {
            Name = queue.Value.Name,
            ActiveCount = queue.Value.ActiveMessageCount,
            TotalCount = queue.Value.TotalMessageCount
        };
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
                payloads.Add(message.Body.ToString());
            }

            return [.. payloads];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to read messages from Queue {QueueName}", queueName);

            return [];
        }
        finally
        {
            if (receiver is not null)
                await receiver.CloseAsync();
        }
    }
}
