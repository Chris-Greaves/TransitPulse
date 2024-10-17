using TransitPulse.Web.Models;

namespace TransitPulse.Web.Services;

public interface IServiceBusService
{
    Task<string[]> GetMessages(string queueName, int count = 5);
    IAsyncEnumerable<QueueState> GetQueues();
}