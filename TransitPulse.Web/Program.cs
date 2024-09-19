using Microsoft.Extensions.Azure;
using TransitPulse.Web.Components;
using TransitPulse.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Configuration.AddEnvironmentVariables(); // No idea if this is needed

builder.Services.AddAzureClients(acBuilder =>
{
    acBuilder.AddServiceBusAdministrationClient(builder.Configuration["ConnectionStrings:MassTransit"]);    // ServiceBusAdministrationClient
    acBuilder.AddServiceBusClient(builder.Configuration["ConnectionStrings:MassTransit"]);                  // ServiceBusClient
});

// Add services for components adopting SSR
builder.Services.AddScoped<IServiceBusService, ServiceBusService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
