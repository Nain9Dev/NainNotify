namespace Notificaciones.Application.Services;

/// <summary>
/// Masks personally identifiable values before they are broadcast to observability channels.
/// Telemetry from this platform is rendered on a publicly reachable dashboard, so raw
/// e-mail addresses must never leave the processing boundary.
/// </summary>
public static class PrivacyMasker
{
    private const int VisiblePrefixLength = 2;

    /// <summary>
    /// Fixed width of the redacted segment. A mask proportional to the original would leak the
    /// length of the address, which is exactly the kind of detail a public dashboard must not
    /// disclose.
    /// </summary>
    private const int MaskedSegmentLength = 6;

    /// <summary>
    /// Converts an address into a partially redacted form, keeping just enough characters
    /// to correlate it visually with the original: "alberto@naindev.com" becomes "al******@naindev.com".
    /// </summary>
    public static string MaskEmail(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "unknown";
        }

        var trimmed = value.Trim();
        var atIndex = trimmed.IndexOf('@');

        if (atIndex <= 0)
        {
            return Redact(trimmed);
        }

        var localPart = trimmed[..atIndex];
        var domainPart = trimmed[atIndex..];

        return string.Concat(Redact(localPart), domainPart);
    }

    private static string Redact(string value)
    {
        var mask = new string('*', MaskedSegmentLength);

        // Too short to show a prefix without revealing the whole value: hide it entirely.
        if (value.Length <= VisiblePrefixLength)
        {
            return mask;
        }

        return string.Concat(value.AsSpan(0, VisiblePrefixLength), mask);
    }
}
