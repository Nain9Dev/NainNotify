using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Notificaciones.UnitTests.Client;

/// <summary>
/// REQ-NOTIF-013: the public page declares one canonical URL, a description of sensible length,
/// Open Graph, Twitter card and JSON-LD metadata, and the discovery files exist in the client's
/// <c>public/</c> folder, which Vite copies verbatim to the root of the deployed site.
/// </summary>
public class SeoDiscoverabilityTests
{
    private const string Canonical = "https://notify.naindev.com/";
    private const string AuthorId = "https://www.naindev.com/#person";
    private const string Repository = "https://github.com/Nain9Dev/NainNotify";

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

    private static string PublicFile(string name) => File.ReadAllText(Path.Combine(ClientRoot(), "public", name));

    private static List<string> MetaContents(string html, string attribute, string key) =>
        Regex.Matches(html, "<meta\\s+[^>]*>", RegexOptions.IgnoreCase, RegexTimeout)
            .Select(m => m.Value)
            .Where(tag => Regex.IsMatch(tag, $"{attribute}=\"{Regex.Escape(key)}\"", RegexOptions.IgnoreCase, RegexTimeout))
            .Select(tag => Regex.Match(tag, "content=\"([^\"]*)\"", RegexOptions.IgnoreCase, RegexTimeout).Groups[1].Value)
            .ToList();

    private static string Meta(string html, string attribute, string key)
    {
        var values = MetaContents(html, attribute, key);
        Assert.True(values.Count == 1, $"Expected exactly one <meta {attribute}=\"{key}\">, found {values.Count}.");
        return values[0];
    }

    [Fact]
    public void The_page_declares_exactly_one_canonical_link_equal_to_the_host_root()
    {
        var links = Regex.Matches(Html(), "<link\\s+[^>]*rel=\"canonical\"[^>]*>", RegexOptions.IgnoreCase, RegexTimeout);

        Assert.Single(links);
        Assert.Contains($"href=\"{Canonical}\"", links[0].Value, StringComparison.Ordinal);
    }

    [Fact]
    public void The_description_is_a_single_tag_of_70_to_160_characters()
    {
        Assert.InRange(Meta(Html(), "name", "description").Length, 70, 160);
    }

    [Fact]
    public void Open_Graph_and_Twitter_cards_describe_the_page_and_agree_with_the_canonical_url()
    {
        var html = Html();

        Assert.Equal("website", Meta(html, "property", "og:type"));
        Assert.Equal("NainNotify", Meta(html, "property", "og:site_name"));
        Assert.False(string.IsNullOrWhiteSpace(Meta(html, "property", "og:title")));
        Assert.False(string.IsNullOrWhiteSpace(Meta(html, "property", "og:description")));
        Assert.Equal(Canonical, Meta(html, "property", "og:url"));
        Assert.StartsWith("https://", Meta(html, "property", "og:image"), StringComparison.Ordinal);
        Assert.Equal("1200", Meta(html, "property", "og:image:width"));
        Assert.Equal("630", Meta(html, "property", "og:image:height"));

        Assert.Equal("summary_large_image", Meta(html, "name", "twitter:card"));
        Assert.False(string.IsNullOrWhiteSpace(Meta(html, "name", "twitter:title")));
        Assert.False(string.IsNullOrWhiteSpace(Meta(html, "name", "twitter:description")));
        Assert.Equal(Meta(html, "property", "og:image"), Meta(html, "name", "twitter:image"));
    }

    [Fact]
    public void The_page_links_its_author()
    {
        Assert.Matches("<link\\s+rel=\"author\"\\s+href=\"https://www.naindev.com/\"", Html());
    }

    [Fact]
    public void The_JSON_LD_describes_the_application_and_names_the_author_by_id()
    {
        var blocks = Regex.Matches(Html(), "<script type=\"application/ld\\+json\">(.*?)</script>",
            RegexOptions.Singleline, RegexTimeout);
        Assert.Single(blocks);

        var data = JsonDocument.Parse(blocks[0].Groups[1].Value).RootElement;

        Assert.Equal("SoftwareApplication", data.GetProperty("@type").GetString());
        Assert.Equal("NainNotify", data.GetProperty("name").GetString());
        Assert.Equal(Canonical, data.GetProperty("url").GetString());
        Assert.Equal("Web", data.GetProperty("operatingSystem").GetString());
        Assert.True(data.GetProperty("isAccessibleForFree").GetBoolean());
        Assert.Equal(Meta(Html(), "name", "description"), data.GetProperty("description").GetString());
        Assert.Equal(AuthorId, data.GetProperty("author").GetProperty("@id").GetString());
        Assert.Contains(Repository, data.GetProperty("sameAs").EnumerateArray().Select(e => e.GetString()));
    }

    [Fact]
    public void The_discovery_files_ship_in_the_public_folder_and_agree_with_the_canonical_url()
    {
        var robots = PublicFile("robots.txt");
        Assert.Contains("User-agent: *", robots, StringComparison.Ordinal);
        Assert.Contains("Allow: /", robots, StringComparison.Ordinal);
        Assert.DoesNotContain("Disallow: /", robots, StringComparison.Ordinal);
        Assert.Contains($"Sitemap: {Canonical}sitemap.xml", robots, StringComparison.Ordinal);

        var sitemap = XDocument.Parse(PublicFile("sitemap.xml"));
        Assert.Equal(Canonical, sitemap.Descendants().Single(e => e.Name.LocalName == "loc").Value);

        var llms = PublicFile("llms.txt");
        Assert.StartsWith("# NainNotify", llms, StringComparison.Ordinal);
        Assert.Contains(Repository, llms, StringComparison.Ordinal);
        Assert.Contains("https://www.naindev.com/", llms, StringComparison.Ordinal);
    }
}
