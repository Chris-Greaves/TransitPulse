namespace TransitPulse.Web.Models;

public class Queue
{
    public required string Name { get; set; }
    public long ActiveCount { get; internal set; }
    public long TotalCount { get; internal set; }
}
