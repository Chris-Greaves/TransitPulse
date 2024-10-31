using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using Microsoft.FluentUI.AspNetCore.Components;
using TransitPulse.Web.Components;
using TransitPulse.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddAzureClients(acBuilder =>
{
    acBuilder.AddServiceBusAdministrationClient(builder.Configuration["ConnectionStrings:MassTransit"]);    // ServiceBusAdministrationClient
    acBuilder.AddServiceBusClient(builder.Configuration["ConnectionStrings:MassTransit"]);                  // ServiceBusClient
});

builder.Services.AddHttpClient();
builder.Services.AddFluentUIComponents();

builder.Services.AddDbContextFactory<QueueDbContext>(options => options.UseSqlite($"Filename={QueueDbContext.SqliteDbFilename}"));

builder.Services.AddTransient<IServiceBusService, ServiceBusService>();

//builder.Services.AddHostedService<DataSynchronizer>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

await app.RunAsync();
