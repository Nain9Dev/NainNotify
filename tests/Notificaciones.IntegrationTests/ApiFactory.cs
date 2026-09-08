using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Notificaciones.IntegrationTests;

/// <summary>
/// Boots the real gateway on the MassTransit in-memory transport, which also hosts the worker
/// consumer in process. That makes the full pipeline exercisable end to end without a broker.
///
/// Each test class receives its own instance, so the per-IP rate limiters never leak state
/// between classes: inside the test host every request reports the same client, and a shared
/// factory would make the limits order dependent.
/// </summary>
public class ApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        // Pinned explicitly rather than inherited from appsettings.Development.json, so the tests
        // keep passing if that file ever changes.
        builder.UseSetting("RabbitMq:UseInMemoryTransport", "true");
        builder.UseSetting("DemoPolicy:RedirectUnknownRecipients", "true");
        builder.UseSetting("DemoPolicy:SinkAddress", "demo-sink@naindev.com");
        builder.UseSetting("NotificationSettings:EnableDemoSimulationMode", "true");
        builder.UseSetting("NotificationSettings:Username", string.Empty);
    }
}
