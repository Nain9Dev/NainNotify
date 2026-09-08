using Notificaciones.Application.Services;
using Notificaciones.Domain.Events;

namespace Notificaciones.UnitTests;

/// <summary>
/// The template engine renders attacker-controlled text (contact form body, subject, sender name)
/// into an HTML document. Escaping is the only thing standing between a visitor and injecting
/// markup into the operator's inbox, so it is pinned per field and per template.
/// </summary>
public class EmailTemplateEngineTests
{
    private readonly EmailTemplateEngine _engine = new();

    private static NotificationEvent EventWith(
        NotificationType type = NotificationType.ContactFormSubmission,
        string subject = "Asunto de prueba",
        string body = "Cuerpo de prueba",
        string senderName = "Marta Ruiz",
        string senderEmail = "marta@example.com") => new()
        {
            NotificationId = Guid.Parse("11111111-2222-3333-4444-555555555555"),
            Recipient = "contact@naindev.com",
            SenderName = senderName,
            SenderEmail = senderEmail,
            Subject = subject,
            Body = body,
            Type = type,
            Priority = "High",
            Timestamp = DateTimeOffset.Parse("2026-03-01T10:30:00Z")
        };

    [Theory]
    [InlineData(NotificationType.ContactFormSubmission)]
    [InlineData(NotificationType.WelcomeMessage)]
    [InlineData(NotificationType.SystemAlert)]
    [InlineData(NotificationType.TestDemo)]
    public void GenerateHtmlTemplate_EscapesScriptTagsInBodyForEveryTemplate(NotificationType type)
    {
        var html = _engine.GenerateHtmlTemplate(
            EventWith(type, body: "<script>alert('xss')</script>"));

        Assert.DoesNotContain("<script>", html, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("&lt;script&gt;", html);
    }

    [Fact]
    public void GenerateHtmlTemplate_EscapesSubject()
    {
        var html = _engine.GenerateHtmlTemplate(EventWith(subject: "<img src=x onerror=alert(1)>"));

        Assert.DoesNotContain("<img src=x", html);
        Assert.Contains("&lt;img src=x", html);
    }

    [Fact]
    public void GenerateHtmlTemplate_EscapesSenderName()
    {
        var html = _engine.GenerateHtmlTemplate(EventWith(senderName: "\"><b>injected</b>"));

        Assert.DoesNotContain("<b>injected</b>", html);
        Assert.Contains("&lt;b&gt;injected", html);
    }

    [Fact]
    public void GenerateHtmlTemplate_EscapesSenderEmailUsedInsideTheMailtoAttribute()
    {
        var html = _engine.GenerateHtmlTemplate(EventWith(senderEmail: "a@b.com\" onmouseover=\"alert(1)"));

        Assert.DoesNotContain("onmouseover=\"alert(1)\"", html);
    }

    [Theory]
    [InlineData(NotificationType.ContactFormSubmission)]
    [InlineData(NotificationType.WelcomeMessage)]
    [InlineData(NotificationType.SystemAlert)]
    [InlineData(NotificationType.TestDemo)]
    public void GenerateHtmlTemplate_ProducesACompleteDocument(NotificationType type)
    {
        var html = _engine.GenerateHtmlTemplate(EventWith(type));

        Assert.StartsWith("<!DOCTYPE html>", html.TrimStart());
        Assert.Contains("</html>", html);
        Assert.Contains("NAIN", html);
    }

    [Fact]
    public void GenerateHtmlTemplate_IncludesAHiddenPreheaderForTheInboxPreview()
    {
        var html = _engine.GenerateHtmlTemplate(EventWith());

        Assert.Contains("mso-hide:all", html);
    }

    [Fact]
    public void GenerateHtmlTemplate_TruncatesOverlongSubjects()
    {
        var html = _engine.GenerateHtmlTemplate(EventWith(subject: new string('A', 400)));

        // 150 characters is the cap; the 400 character run must not survive intact.
        Assert.DoesNotContain(new string('A', 200), html);
    }

    [Fact]
    public void GenerateHtmlTemplate_RendersMetadataOnTheGenericTemplate()
    {
        var notification = EventWith(NotificationType.TestDemo) with
        {
            Metadata = new Dictionary<string, string> { ["Origin"] = "console", ["Attempt"] = "1" }
        };

        var html = _engine.GenerateHtmlTemplate(notification);

        Assert.Contains("Origin", html);
        Assert.Contains("console", html);
    }

    [Fact]
    public void GenerateHtmlTemplate_EscapesMetadataValues()
    {
        var notification = EventWith(NotificationType.TestDemo) with
        {
            Metadata = new Dictionary<string, string> { ["UserAgent"] = "<script>bad()</script>" }
        };

        var html = _engine.GenerateHtmlTemplate(notification);

        Assert.DoesNotContain("<script>", html, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GeneratePlainTextTemplate_KeepsTheBodyReadableWithoutMarkup()
    {
        var text = _engine.GeneratePlainTextTemplate(EventWith(body: "Primera linea\nSegunda linea"));

        Assert.Contains("Primera linea", text);
        Assert.Contains("Segunda linea", text);

        // No markup: the angle brackets that remain belong to the "Name <address>" convention.
        Assert.DoesNotContain("<!DOCTYPE", text);
        Assert.DoesNotContain("<div", text);
        Assert.DoesNotContain("<table", text);
        Assert.DoesNotContain("style=", text);
    }

    [Fact]
    public void GeneratePlainTextTemplate_CarriesTheTrackingIdentifier()
    {
        var notification = EventWith();

        var text = _engine.GeneratePlainTextTemplate(notification);

        Assert.Contains(notification.NotificationId.ToString(), text);
    }

    [Fact]
    public void GenerateHtmlTemplate_UsesADistinctAccentPerNotificationType()
    {
        var contact = _engine.GenerateHtmlTemplate(EventWith(NotificationType.ContactFormSubmission));
        var welcome = _engine.GenerateHtmlTemplate(EventWith(NotificationType.WelcomeMessage));
        var alert = _engine.GenerateHtmlTemplate(EventWith(NotificationType.SystemAlert));

        Assert.Contains("#38BDF8", contact);
        Assert.Contains("#10B981", welcome);
        Assert.Contains("#F43F5E", alert);
    }

    [Fact]
    public void GenerateHtmlTemplate_RejectsNull()
    {
        Assert.Throws<ArgumentNullException>(() => _engine.GenerateHtmlTemplate(null!));
    }
}
