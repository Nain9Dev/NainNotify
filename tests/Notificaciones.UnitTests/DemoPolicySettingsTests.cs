using Notificaciones.Api.Settings;

namespace Notificaciones.UnitTests;

/// <summary>
/// The demo endpoint is publicly reachable and accepts a recipient from the caller. Without this
/// guard the gateway would become an open mail relay the moment real SMTP credentials exist,
/// so every branch of the rule is covered here.
/// </summary>
public class DemoPolicySettingsTests
{
    private const string Sink = "demo-sink@naindev.com";

    private static DemoPolicySettings PolicyWith(params string[] allowedDomains) => new()
    {
        RedirectUnknownRecipients = true,
        SinkAddress = Sink,
        AllowedRecipientDomains = allowedDomains
    };

    [Fact]
    public void ResolveRecipient_RedirectsUnknownDomainToSink()
    {
        var (recipient, wasRedirected) = PolicyWith("naindev.com").ResolveRecipient("attacker@evil.example");

        Assert.Equal(Sink, recipient);
        Assert.True(wasRedirected);
    }

    [Fact]
    public void ResolveRecipient_AllowsListedDomain()
    {
        var (recipient, wasRedirected) = PolicyWith("naindev.com").ResolveRecipient("aitor@naindev.com");

        Assert.Equal("aitor@naindev.com", recipient);
        Assert.False(wasRedirected);
    }

    [Theory]
    [InlineData("aitor@NAINDEV.COM")]
    [InlineData("aitor@NainDev.com")]
    public void ResolveRecipient_ComparesDomainCaseInsensitively(string recipient)
    {
        var (resolved, wasRedirected) = PolicyWith("naindev.com").ResolveRecipient(recipient);

        Assert.Equal(recipient, resolved);
        Assert.False(wasRedirected);
    }

    [Fact]
    public void ResolveRecipient_RedirectsEverythingWhenNoDomainIsAllowed()
    {
        // Default configuration: the allow list is empty, so nothing reaches a real inbox.
        var (recipient, wasRedirected) = PolicyWith().ResolveRecipient("aitor@naindev.com");

        Assert.Equal(Sink, recipient);
        Assert.True(wasRedirected);
    }

    [Theory]
    [InlineData("sin-arroba")]
    [InlineData("trailing@")]
    public void ResolveRecipient_RedirectsMalformedAddresses(string recipient)
    {
        var (resolved, wasRedirected) = PolicyWith("naindev.com").ResolveRecipient(recipient);

        Assert.Equal(Sink, resolved);
        Assert.True(wasRedirected);
    }

    [Fact]
    public void ResolveRecipient_UsesLastAtSoSubaddressingCannotSpoofTheDomain()
    {
        // "victim@naindev.com@evil.example" must resolve against evil.example, not naindev.com.
        var (recipient, wasRedirected) = PolicyWith("naindev.com")
            .ResolveRecipient("victim@naindev.com@evil.example");

        Assert.Equal(Sink, recipient);
        Assert.True(wasRedirected);
    }

    [Fact]
    public void ResolveRecipient_HonoursDisabledRedirection()
    {
        var policy = new DemoPolicySettings
        {
            RedirectUnknownRecipients = false,
            SinkAddress = Sink,
            AllowedRecipientDomains = []
        };

        var (recipient, wasRedirected) = policy.ResolveRecipient("anyone@anywhere.example");

        Assert.Equal("anyone@anywhere.example", recipient);
        Assert.False(wasRedirected);
    }

    [Fact]
    public void ResolveRecipient_FallsBackToSinkWhenRecipientIsEmpty()
    {
        var policy = new DemoPolicySettings
        {
            RedirectUnknownRecipients = false,
            SinkAddress = Sink,
            AllowedRecipientDomains = []
        };

        var (recipient, _) = policy.ResolveRecipient("   ");

        Assert.Equal(Sink, recipient);
    }
}
