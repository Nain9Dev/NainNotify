using System.Buffers.Binary;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Notificaciones.UnitTests.Client;

/// <summary>
/// REQ-NOTIF-014: the public page can be installed on Android Chrome and iOS Safari. The manifest and
/// the install icons ship in the client's <c>public/</c> folder, which Vite copies verbatim to the root
/// of the deployed site, and the page links them with relative paths like the rest of its assets.
/// </summary>
public class InstallabilityTests
{
    private static readonly TimeSpan RegexTimeout = TimeSpan.FromSeconds(1);

    private static string ClientRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "MicroservicioNotificaciones.sln")))
            directory = directory.Parent;

        Assert.NotNull(directory);
        return Path.Combine(directory!.FullName, "Notificaciones.Client");
    }

    private static string Html() => File.ReadAllText(Path.Combine(ClientRoot(), "index.html"));

    private static string PublicPath(string relative) => Path.Combine(ClientRoot(), "public", relative);

    private static JsonElement Manifest() =>
        JsonDocument.Parse(File.ReadAllText(PublicPath("manifest.webmanifest"))).RootElement;

    private static string Meta(string html, string name)
    {
        var values = Regex.Matches(html, "<meta\\s+[^>]*>", RegexOptions.IgnoreCase, RegexTimeout)
            .Select(m => m.Value)
            .Where(tag => Regex.IsMatch(tag, $"name=\"{Regex.Escape(name)}\"", RegexOptions.IgnoreCase, RegexTimeout))
            .Select(tag => Regex.Match(tag, "content=\"([^\"]*)\"", RegexOptions.IgnoreCase, RegexTimeout).Groups[1].Value)
            .ToList();
        Assert.True(values.Count == 1, $"Expected exactly one <meta name=\"{name}\">, found {values.Count}.");
        return values[0];
    }

    private static string LinkHref(string html, string rel)
    {
        var links = Regex.Matches(html, $"<link\\s+[^>]*rel=\"{Regex.Escape(rel)}\"[^>]*>", RegexOptions.IgnoreCase, RegexTimeout);
        Assert.True(links.Count == 1, $"Expected exactly one <link rel=\"{rel}\">, found {links.Count}.");
        return Regex.Match(links[0].Value, "href=\"([^\"]*)\"", RegexOptions.IgnoreCase, RegexTimeout).Groups[1].Value;
    }

    // Width, height and colour type are fixed offsets of the PNG IHDR chunk; no image library is needed.
    private static (int Width, int Height, byte ColorType) PngHeader(string path)
    {
        var bytes = File.ReadAllBytes(path);
        Assert.True(bytes.Length > 33 && bytes[1] == 'P' && bytes[2] == 'N' && bytes[3] == 'G', $"{path} is not a PNG file.");
        return (BinaryPrimitives.ReadInt32BigEndian(bytes.AsSpan(16)), BinaryPrimitives.ReadInt32BigEndian(bytes.AsSpan(20)), bytes[25]);
    }

    // Inflates the IDAT stream and undoes the scanline filters of an 8-bit RGBA PNG to read every alpha byte.
    private static bool IsFullyOpaque(string path)
    {
        var bytes = File.ReadAllBytes(path);
        var (width, height, colorType) = PngHeader(path);
        Assert.True(colorType == 6 && bytes[24] == 8, $"{path}: expected 8-bit RGBA, got depth {bytes[24]} colour type {colorType}.");

        using var idat = new MemoryStream();
        for (var offset = 8; offset < bytes.Length;)
        {
            var length = BinaryPrimitives.ReadInt32BigEndian(bytes.AsSpan(offset));
            if (bytes[offset + 4] == 'I' && bytes[offset + 5] == 'D' && bytes[offset + 6] == 'A' && bytes[offset + 7] == 'T')
                idat.Write(bytes, offset + 8, length);
            offset += 12 + length;
        }

        idat.Position = 0;
        using var raw = new MemoryStream();
        using (var inflate = new System.IO.Compression.ZLibStream(idat, System.IO.Compression.CompressionMode.Decompress))
            inflate.CopyTo(raw);

        var data = raw.ToArray();
        var stride = width * 4;
        var previous = new byte[stride];
        for (var row = 0; row < height; row++)
        {
            var filter = data[row * (stride + 1)];
            var line = data.AsSpan(row * (stride + 1) + 1, stride);
            for (var i = 0; i < stride; i++)
            {
                int left = i >= 4 ? line[i - 4] : 0, up = previous[i], upLeft = i >= 4 ? previous[i - 4] : 0;
                var estimate = left + up - upLeft;
                var (pa, pb, pc) = (Math.Abs(estimate - left), Math.Abs(estimate - up), Math.Abs(estimate - upLeft));
                line[i] += filter switch
                {
                    1 => (byte)left,
                    2 => (byte)up,
                    3 => (byte)((left + up) / 2),
                    4 => (byte)(pa <= pb && pa <= pc ? left : pb <= pc ? up : upLeft),
                    _ => (byte)0,
                };
                if (i % 4 == 3 && line[i] != 255) return false;
            }

            previous = line.ToArray();
        }

        return true;
    }

    [Fact]
    public void The_manifest_names_the_app_and_opens_it_standalone_from_the_page_location()
    {
        var manifest = Manifest();

        Assert.Equal("NainNotify", manifest.GetProperty("name").GetString());
        Assert.Equal("NainNotify", manifest.GetProperty("short_name").GetString());
        Assert.Equal(Meta(Html(), "description"), manifest.GetProperty("description").GetString());
        Assert.Equal("es", manifest.GetProperty("lang").GetString());
        Assert.Equal("ltr", manifest.GetProperty("dir").GetString());
        Assert.Equal("./", manifest.GetProperty("start_url").GetString());
        Assert.Equal(manifest.GetProperty("start_url").GetString(), manifest.GetProperty("id").GetString());
        Assert.Equal("./", manifest.GetProperty("scope").GetString());
        Assert.Equal("standalone", manifest.GetProperty("display").GetString());
        Assert.Equal(Meta(Html(), "theme-color"), manifest.GetProperty("theme_color").GetString());
        Assert.Matches("^#[0-9a-f]{6}$", manifest.GetProperty("background_color").GetString()!);
        Assert.False(manifest.TryGetProperty("prefer_related_applications", out _));
    }

    [Fact]
    public void The_manifest_declares_two_any_icons_and_one_maskable_icon_at_their_real_size()
    {
        var icons = Manifest().GetProperty("icons").EnumerateArray().ToList();

        Assert.Equal(
            ["./icons/icon-192.png 192x192 any", "./icons/icon-512.png 512x512 any", "./icons/icon-maskable-512.png 512x512 maskable"],
            icons.Select(i => $"{i.GetProperty("src").GetString()} {i.GetProperty("sizes").GetString()} {i.GetProperty("purpose").GetString()}"));

        foreach (var icon in icons)
        {
            Assert.Equal("image/png", icon.GetProperty("type").GetString());
            var header = PngHeader(PublicPath(icon.GetProperty("src").GetString()!));
            Assert.Equal(icon.GetProperty("sizes").GetString(), $"{header.Width}x{header.Height}");
        }
    }

    [Fact]
    public void The_install_icons_are_opaque_where_the_platform_needs_them_to_be()
    {
        var apple = PngHeader(PublicPath("icons/apple-touch-icon.png"));

        Assert.Equal((180, 180), (apple.Width, apple.Height));
        Assert.True(IsFullyOpaque(PublicPath("icons/icon-maskable-512.png")), "A launcher would fill transparent pixels of the maskable icon.");
        Assert.True(IsFullyOpaque(PublicPath("icons/apple-touch-icon.png")), "iOS paints transparent pixels of the touch icon black.");
    }

    [Fact]
    public void The_page_links_the_manifest_and_carries_the_iOS_tags()
    {
        var html = Html();

        Assert.Equal("./manifest.webmanifest", LinkHref(html, "manifest"));
        Assert.Equal("./icons/apple-touch-icon.png", LinkHref(html, "apple-touch-icon"));
        Assert.Equal("yes", Meta(html, "mobile-web-app-capable"));
        Assert.Equal("yes", Meta(html, "apple-mobile-web-app-capable"));
        Assert.Equal("NainNotify", Meta(html, "apple-mobile-web-app-title"));
        // The page is dark, so the iOS status bar is black.
        Assert.Equal("black", Meta(html, "apple-mobile-web-app-status-bar-style"));
    }
}
