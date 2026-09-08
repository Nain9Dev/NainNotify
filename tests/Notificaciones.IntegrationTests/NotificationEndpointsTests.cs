using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Notificaciones.IntegrationTests;

/// <summary>
/// Covers the endpoints that do not consume the contact form rate limit budget.
/// </summary>
public class NotificationEndpointsTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;

    public NotificationEndpointsTests(ApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    private static object DemoPayload(
        string subject = "Prueba de integracion",
        string content = "Contenido suficientemente largo para la prueba",
        string eventType = "WelcomeMessage",
        string recipient = "evaluator@demo-test.com",
        string priority = "High") => new
        {
            targetRecipient = recipient,
            subject,
            content,
            eventType,
            priority
        };

    [Fact]
    public async Task Health_ReportsTheEffectiveTransportAndDependencies()
    {
        var response = await _client.GetAsync("/api/v1/notifications/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal("Healthy", body.GetProperty("status").GetString());
        Assert.Equal("in-memory", body.GetProperty("transport").GetString());
        Assert.True(body.GetProperty("workerInProcess").GetBoolean());
        Assert.True(body.GetProperty("dependencies").EnumerateObject().Any());
    }

    [Fact]
    public async Task Preview_RendersTheTemplateWithoutTouchingTheBroker()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/notifications/preview", DemoPayload());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var html = body.GetProperty("html").GetString()!;

        Assert.StartsWith("<!DOCTYPE html>", html.TrimStart());
        Assert.True(body.GetProperty("bytes").GetInt32() > 1000);
        Assert.False(string.IsNullOrWhiteSpace(body.GetProperty("plainText").GetString()));
    }

    [Fact]
    public async Task Preview_EscapesMarkupComingFromTheRequest()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/notifications/preview",
            DemoPayload(content: "<script>alert('xss')</script>"));

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var html = body.GetProperty("html").GetString()!;

        Assert.DoesNotContain("<script>", html, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("&lt;script&gt;", html);
    }

    [Fact]
    public async Task Demo_RedirectsUnlistedRecipientsToTheSandboxInbox()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/notifications/demo",
            DemoPayload(recipient: "attacker@evil.example"));

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.True(body.GetProperty("sandboxed").GetBoolean());
        Assert.False(string.IsNullOrWhiteSpace(body.GetProperty("sandboxNotice").GetString()));
    }

    [Fact]
    public async Task Demo_NeverEchoesTheRecipientInClearText()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/notifications/demo", DemoPayload());
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        var effectiveRecipient = body.GetProperty("effectiveRecipient").GetString()!;

        Assert.Contains("*", effectiveRecipient);
        Assert.DoesNotContain("demo-sink@", effectiveRecipient);
    }

    [Theory]
    [InlineData("no-es-un-email", "Asunto", "Contenido valido", "WelcomeMessage", "High")]
    [InlineData("a@b.com", "", "Contenido valido", "WelcomeMessage", "High")]
    [InlineData("a@b.com", "Asunto", "Contenido valido", "WelcomeMessage", "Urgente")]
    public async Task Demo_RejectsInvalidPayloadsBeforeReachingTheBroker(
        string recipient, string subject, string content, string eventType, string priority)
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/notifications/demo",
            DemoPayload(subject, content, eventType, recipient, priority));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Metrics_ExposesASixtySecondThroughputWindow()
    {
        var response = await _client.GetAsync("/api/v1/notifications/metrics");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var series = body.GetProperty("snapshot").GetProperty("throughputSeries");

        Assert.Equal(60, series.GetArrayLength());
    }

    [Fact]
    public async Task Activity_ClampsTheRequestedLimit()
    {
        var response = await _client.GetAsync("/api/v1/notifications/activity?limit=100000");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.True(body.GetArrayLength() <= 200);
    }

    [Fact]
    public async Task Console_IsServedFromTheSiteRoot()
    {
        var response = await _client.GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/html", response.Content.Headers.ContentType?.MediaType);

        var html = await response.Content.ReadAsStringAsync();

        Assert.Contains("Cloud Notification Console", html);
        Assert.Contains("assets/js/console.js", html);
    }

    [Theory]
    [InlineData("/assets/css/console.css", "text/css")]
    [InlineData("/assets/js/console.js", "text/javascript")]
    public async Task Console_ServesItsStaticAssets(string path, string expectedMediaType)
    {
        var response = await _client.GetAsync(path);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(expectedMediaType, response.Content.Headers.ContentType?.MediaType);
    }
}
