using TransitPulse.Web.Models;

namespace TransitPulse.Web.Services;

public interface IServiceBusService
{
    Task<QueueState[]> GetQueues();
}