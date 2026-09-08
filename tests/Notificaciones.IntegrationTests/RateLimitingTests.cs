using System.Net;
using System.Net.Http.Json;

namespace Notificaciones.IntegrationTests;

/// <summary>
/// The gateway is publicly reachable, so the per-client limit is a security control rather than a
/// nicety.
///
/// Every test here builds its own host: inside the test server all requests report the same
/// client, so a shared limiter would make these assertions depend on execution order.
/// </summary>
public class RateLimitingTests
{
    private const int ContactPermitLimit = 5;

    private static object ContactPayload(int index) => new
    {
        senderName = "Marta Ruiz",
        senderEmail = "marta@example.com",
        subject = $"Consulta numero {index}",
        message = "Mensaje de prueba con longitud suficiente para superar la validacion."
    };

    [Fact]
    public async Task Contact_RejectsRequestsOnceThePermitLimitIsExhausted()
    {
        using var factory = new ApiFactory();
        using var client = factory.CreateClient();

        var statuses = new List<HttpStatusCode>();

        for (var i = 0; i < ContactPermitLimit + 2; i++)
        {
            var response = await client.PostAsJsonAsync("/api/v1/notifications/contact", ContactPayload(i));
            statuses.Add(response.StatusCode);
        }

        Assert.All(statuses.Take(ContactPermitLimit), status => Assert.Equal(HttpStatusCode.Accepted, status));
        Assert.All(statuses.Skip(ContactPermitLimit), status => Assert.Equal(HttpStatusCode.TooManyRequests, status));
    }

    [Fact]
    public async Task RejectedRequests_ExplainTheLimitInTheResponseBody()
    {
        using var factory = new ApiFactory();
        using var client = factory.CreateClient();

        HttpResponseMessage? rejected = null;

        for (var i = 0; i < ContactPermitLimit + 3 && rejected is null; i++)
        {
            var response = await client.PostAsJsonAsync("/api/v1/notifications/contact", ContactPayload(i));

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                rejected = response;
            }
        }

        Assert.NotNull(rejected);
        Assert.Equal("application/json", rejected.Content.Headers.ContentType?.MediaType);

        var body = await rejected.Content.ReadAsStringAsync();
        Assert.Contains("Rate limit exceeded", body);
    }

    [Fact]
    public async Task ForwardedForHeader_CannotBeUsedToResetThePartition()
    {
        using var factory = new ApiFactory();
        using var client = factory.CreateClient();

        var statuses = new List<HttpStatusCode>();

        // Rotating the header would bypass every limit if the gateway trusted it blindly.
        for (var i = 0; i < ContactPermitLimit + 2; i++)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/notifications/contact")
            {
                Content = JsonContent.Create(ContactPayload(i))
            };
            request.Headers.Add("X-Forwarded-For", $"10.0.0.{i + 1}");

            var response = await client.SendAsync(request);
            statuses.Add(response.StatusCode);
        }

        Assert.Contains(HttpStatusCode.TooManyRequests, statuses);
    }
}
