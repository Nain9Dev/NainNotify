using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Notificaciones.IntegrationTests;

/// <summary>
/// The contact endpoint is limited to five requests per minute and per client, and inside the
/// test host every request looks like the same client. This class therefore stays under that
/// budget on purpose; the limit itself is exercised in <see cref="RateLimitingTests"/>.
/// </summary>
public class ContactEndpointTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;

    public ContactEndpointTests(ApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    private static object ContactPayload(string? website = null) => new
    {
        senderName = "Marta Ruiz",
        senderEmail = "marta@example.com",
        subject = "Consulta sobre el microservicio",
        message = "Mensaje de prueba con longitud suficiente para superar la validacion.",
        website
    };

    [Fact]
    public async Task Contact_AcceptsAValidSubmissionAndReturnsATrackingId()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/notifications/contact", ContactPayload());

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal("Queued", body.GetProperty("status").GetString());
        Assert.NotEqual(Guid.Empty, body.GetProperty("trackingId").GetGuid());
    }

    [Fact]
    public async Task Contact_DiscardsHoneypotSubmissionsWithoutRevealingTheRule()
    {
        var before = await ReadAcceptedCounterAsync();

        var response = await _client.PostAsJsonAsync(
            "/api/v1/notifications/contact",
            ContactPayload(website: "http://spam.example"));

        // The bot must not be able to tell a discarded message from an accepted one.
        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Queued", body.GetProperty("status").GetString());

        // But nothing entered the pipeline.
        var after = await ReadAcceptedCounterAsync();
        Assert.Equal(before, after);
    }

    [Fact]
    public async Task Contact_RejectsAMessageShorterThanTheMinimum()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/notifications/contact", new
        {
            senderName = "Marta",
            senderEmail = "marta@example.com",
            subject = "Hola",
            message = "corto"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private async Task<long> ReadAcceptedCounterAsync()
    {
        var response = await _client.GetAsync("/api/v1/notifications/metrics");
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        return body.GetProperty("snapshot").GetProperty("accepted").GetInt64();
    }
}
