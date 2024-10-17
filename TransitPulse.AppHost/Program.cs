var builder = DistributedApplication.CreateBuilder(args);

var mt = builder.AddConnectionString("MassTransit", "ConnectionStrings:MassTransit");

builder.AddProject<Projects.TransitPulse_Web>("transitpulse-web")
    .WithReference(mt);

await builder.Build().RunAsync();
