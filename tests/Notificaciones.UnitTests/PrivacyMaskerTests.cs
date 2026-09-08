using Notificaciones.Application.Services;

namespace Notificaciones.UnitTests;

/// <summary>
/// Telemetry from this platform is rendered on a publicly reachable console, so no raw address
/// may ever leave the processing boundary. These tests pin that guarantee.
/// </summary>
public class PrivacyMaskerTests
{
    [Theory]
    [InlineData("alberto@naindev.com", "al******@naindev.com")]
    [InlineData("contact@naindev.com", "co******@naindev.com")]
    [InlineData("ana@x.io", "an******@x.io")]
    public void MaskEmail_KeepsDomainAndHidesLocalPart(string input, string expected)
    {
        Assert.Equal(expected, PrivacyMasker.MaskEmail(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void MaskEmail_ReturnsPlaceholderForMissingValues(string? input)
    {
        Assert.Equal("unknown", PrivacyMasker.MaskEmail(input));
    }

    [Fact]
    public void MaskEmail_RedactsValuesWithoutDomain()
    {
        var masked = PrivacyMasker.MaskEmail("plaintextidentifier");

        Assert.StartsWith("pl", masked);
        Assert.DoesNotContain("aintextidentifier", masked);
    }

    [Fact]
    public void MaskEmail_HidesShortLocalPartsCompletely()
    {
        // Two characters leave no room for a prefix without revealing the whole local part.
        Assert.Equal("******@naindev.com", PrivacyMasker.MaskEmail("ab@naindev.com"));
    }

    [Fact]
    public void MaskEmail_DoesNotLeakLocalPartLength()
    {
        // A variable-length mask would let an observer infer the original address length.
        var shortAddress = PrivacyMasker.MaskEmail("alberto@naindev.com");
        var longAddress = PrivacyMasker.MaskEmail("albertoduplicadolarguisimo@naindev.com");

        Assert.Equal(shortAddress.Length, longAddress.Length);
    }

    [Fact]
    public void MaskEmail_TrimsSurroundingWhitespace()
    {
        Assert.Equal("al******@naindev.com", PrivacyMasker.MaskEmail("  alberto@naindev.com  "));
    }
}
