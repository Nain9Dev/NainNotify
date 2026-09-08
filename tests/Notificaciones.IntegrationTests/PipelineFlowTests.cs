using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Notificaciones.IntegrationTests;

/// <summary>
/// End-to-end coverage of the pipeline: a request published by the gateway must be consumed by the
/// in-process worker and produce the full sequence of lifecycle stages in the read model.
///
/// This is the behaviour the live console renders, so if the ordering or the stage set ever
/// regresses, the console silently lies. That is what these tests prevent.
/// </summary>
public class PipelineFlowTests
{
    private static readonly TimeSpan SettleTimeout = TimeSpan.FromSeconds(15);

    private static object DemoPayload(string subject) => new
    {
        targetRecipient = "evaluator@demo-test.com",
        subject,
        content = "Contenido de verificacion del pipeline completo.",
        eventType = "WelcomeMessage",
        priority = "High"
    };

    [Fact]
    public async Task Dispatch_ProducesEveryStageFromAcceptedToDispatched()
    {
        using var factory = new ApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/notifications/demo", DemoPayload("Flujo completo"));
        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);

        var accepted = await response.Content.ReadFromJsonAsync<JsonElement>();
        var trackingId = accepted.GetProperty("assignedTrackingId").GetGuid();

        var stages = await WaitForStagesAsync(client, trackingId, expectedCount: 5);

        Assert.Equal(
            new[] { "Accepted", "Queued", "Received", "Rendered", "Dispatched" },
            stages);
    }

    [Fact]
    public async Task Dispatch_RecordsTheEffectiveDeliveryChannelWithoutClaimingARealDelivery()
    {
        using var factory = new ApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/notifications/demo", DemoPayload("Canal efectivo"));
        var accepted = await response.Content.ReadFromJsonAsync<JsonElement>();
        var trackingId = accepted.GetProperty("assignedTrackingId").GetGuid();

        await WaitForStagesAsync(client, trackingId, expectedCount: 5);

        var metrics = await client.GetFromJsonAsync<JsonElement>("/api/v1/notifications/metrics");
        var byChannel = metrics.GetProperty("snapshot").GetProperty("byChannel");

        // No SMTP credentials are configured, so the honest answer is "simulation", never "smtp".
        Assert.True(byChannel.TryGetProperty("simulation", out var simulated));
        Assert.True(simulated.GetInt64() >= 1);
        Assert.False(byChannel.TryGetProperty("smtp", out _));
    }

    [Fact]
    public async Task Dispatch_MasksTheRecipientInEveryTelemetryFrame()
    {
        using var factory = new ApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/notifications/demo", DemoPayload("Privacidad"));
        var accepted = await response.Content.ReadFromJsonAsync<JsonElement>();
        var trackingId = accepted.GetProperty("assignedTrackingId").GetGuid();

        await WaitForStagesAsync(client, trackingId, expectedCount: 5);

        var activity = await client.GetFromJsonAsync<JsonElement>("/api/v1/notifications/activity?limit=50");

        var recipients = activity.EnumerateArray()
            .Where(e => e.GetProperty("notificationId").GetGuid() == trackingId)
            .Select(e => e.GetProperty("recipient").GetString()!)
            .ToList();

        Assert.NotEmpty(recipients);
        Assert.All(recipients, recipient => Assert.Contains("*", recipient));
    }

    [Fact]
    public async Task Metrics_ReflectTheDispatchInTheAggregatedCounters()
    {
        using var factory = new ApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/notifications/demo", DemoPayload("Contadores"));
        var accepted = await response.Content.ReadFromJsonAsync<JsonElement>();
        var trackingId = accepted.GetProperty("assignedTrackingId").GetGuid();

        await WaitForStagesAsync(client, trackingId, expectedCount: 5);

        var metrics = await client.GetFromJsonAsync<JsonElement>("/api/v1/notifications/metrics");
        var snapshot = metrics.GetProperty("snapshot");

        Assert.Equal(1, snapshot.GetProperty("accepted").GetInt64());
        Assert.Equal(1, snapshot.GetProperty("dispatched").GetInt64());
        Assert.Equal(0, snapshot.GetProperty("failed").GetInt64());
        Assert.Equal(0, snapshot.GetProperty("inFlight").GetInt32());
        Assert.Equal(100d, snapshot.GetProperty("successRate").GetDouble());
        Assert.True(snapshot.GetProperty("renderedBytes").GetInt64() > 0);
    }

    /// <summary>
    /// Polls the activity feed until the notification has produced the expected number of stages.
    /// The worker runs asynchronously, so a fixed delay would either be flaky or needlessly slow.
    /// </summary>
    private static async Task<string[]> WaitForStagesAsync(HttpClient client, Guid trackingId, int expectedCount)
    {
        var deadline = DateTime.UtcNow + SettleTimeout;
        var stages = Array.Empty<string>();

        while (DateTime.UtcNow < deadline)
        {
            var activity = await client.GetFromJsonAsync<JsonElement>("/api/v1/notifications/activity?limit=100");

            stages = activity.EnumerateArray()
                .Where(e => e.GetProperty("notificationId").GetGuid() == trackingId)
                .Select(e => e.GetProperty("stage").GetString()!)
                .Reverse() // The feed is newest first; the pipeline reads oldest first.
                .ToArray();

            if (stages.Length >= expectedCount)
            {
                return stages;
            }

            await Task.Delay(100);
        }

        Assert.Fail(
            $"El pipeline no alcanzo {expectedCount} etapas en {SettleTimeout.TotalSeconds:0} s. " +
            $"Observadas: [{string.Join(", ", stages)}]");

        return stages;
    }
}
