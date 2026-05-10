using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

if (args.Length == 0)
{
    Console.Error.WriteLine("Usage: WxrToHugo <wxr-path> [--out <dir>] [--limit N] [--audit-only]");
    return 1;
}

string wxrPath = args[0];
string? outDir = null;
int limit = int.MaxValue;
bool auditOnly = false;
for (int i = 1; i < args.Length; i++)
{
    switch (args[i])
    {
        case "--out": outDir = args[++i]; break;
        case "--limit": limit = int.Parse(args[++i]); break;
        case "--audit-only": auditOnly = true; break;
    }
}

if (!File.Exists(wxrPath))
{
    Console.Error.WriteLine($"File not found: {wxrPath}");
    return 1;
}

XNamespace wp = "http://wordpress.org/export/1.2/";
XNamespace content = "http://purl.org/rss/1.0/modules/content/";

var doc = XDocument.Load(wxrPath);
var items = doc.Descendants("item").ToList();

var attachmentsById = items
    .Where(i => (string?)i.Element(wp + "post_type") == "attachment")
    .ToDictionary(
        i => (string?)i.Element(wp + "post_id") ?? "",
        i => (string?)i.Element(wp + "attachment_url") ?? "");

var publishedPosts = items
    .Where(i => (string?)i.Element(wp + "post_type") == "post"
             && (string?)i.Element(wp + "status") == "publish")
    .OrderBy(p => (string?)p.Element(wp + "post_date_gmt"))
    .ToList();

// post_id → slug lookup (published posts only, for menu resolution)
var slugByPostId = publishedPosts.ToDictionary(
    p => (string?)p.Element(wp + "post_id") ?? "",
    p => (string?)p.Element(wp + "post_name") ?? "");

// Build menus: name → ordered list of (label, href). Each WP nav_menu_item
// resolves to either a target post (linked by id) or a custom URL.
var menus = new Dictionary<string, List<(int order, string label, string href)>>(StringComparer.OrdinalIgnoreCase);
foreach (var item in items.Where(i => (string?)i.Element(wp + "post_type") == "nav_menu_item"))
{
    var menuName = item.Elements("category")
        .FirstOrDefault(c => (string?)c.Attribute("domain") == "nav_menu")?.Value;
    if (string.IsNullOrEmpty(menuName)) continue;

    string? metaVal(string key) => item.Elements(wp + "postmeta")
        .FirstOrDefault(m => (string?)m.Element(wp + "meta_key") == key)
        ?.Element(wp + "meta_value")?.Value;

    string label = ((string?)item.Element("title") ?? "").Trim();
    string href = "";
    var type = metaVal("_menu_item_type");
    if (type == "post_type" || type == "post")
    {
        var targetId = metaVal("_menu_item_object_id");
        if (targetId != null && slugByPostId.TryGetValue(targetId, out var s) && !string.IsNullOrEmpty(s))
            href = $"/{s}/";
    }
    else if (type == "custom")
    {
        href = metaVal("_menu_item_url") ?? "";
        href = Regex.Replace(href, @"https?://(www\.)?notadesigner\.com", "");
    }
    if (string.IsNullOrEmpty(href)) continue;
    if (string.IsNullOrEmpty(label))
    {
        // Fall back to the linked post's title if menu item has no label
        var targetId = metaVal("_menu_item_object_id");
        if (targetId != null)
        {
            var target = publishedPosts.FirstOrDefault(p => (string?)p.Element(wp + "post_id") == targetId);
            label = ((string?)target?.Element("title") ?? "").Trim();
        }
    }
    int order = int.TryParse((string?)item.Element(wp + "menu_order"), out var o) ? o : 0;

    if (!menus.TryGetValue(menuName, out var list))
        menus[menuName] = list = new List<(int, string, string)>();
    list.Add((order, label, href));
}
// Sort each menu's items by menu_order (WP admin ordering).
foreach (var k in menus.Keys.ToList())
    menus[k] = menus[k].OrderBy(t => t.order).ToList();

Console.WriteLine($"Items: {items.Count}");
Console.WriteLine($"Published posts: {publishedPosts.Count}");
Console.WriteLine($"Attachments: {attachmentsById.Count}");
Console.WriteLine();

string auditDir = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(wxrPath))!, "wxr-audit");
Directory.CreateDirectory(auditDir);

if (auditOnly || outDir is null)
{
    WriteAuditManifest(publishedPosts, attachmentsById, wp, content, auditDir);
    Console.WriteLine($"Audit written to {auditDir}. Pass --out <dir> to convert.");
    return 0;
}

Directory.CreateDirectory(outDir);
var warnings = new List<string>();
var flashTodo = new List<string>();
var imageFailures = new List<string>();
var alignedFigures = new List<string>();
var mediaTodo = new List<string>();

using var http = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.All });
http.Timeout = TimeSpan.FromSeconds(30);
http.DefaultRequestHeaders.UserAgent.ParseAdd("WxrToHugo/1.0");

var converter = new ReverseMarkdown.Converter(new ReverseMarkdown.Config
{
    UnknownTags = ReverseMarkdown.Config.UnknownTagsOption.PassThrough,
    GithubFlavored = true,
    RemoveComments = true,
    SmartHrefHandling = true,
});

// Convert published pages with a real body — skipping ones that depend on a
// custom PHP template (e.g. the WP Archives page, which Hugo will regenerate
// from a template). Pages land at content/<slug>/index.md, not under posts/.
string contentRoot = Path.GetDirectoryName(outDir.TrimEnd(Path.DirectorySeparatorChar))!;
int pagesConverted = 0;
foreach (var page in items.Where(i =>
    (string?)i.Element(wp + "post_type") == "page"
    && (string?)i.Element(wp + "status") == "publish"
    && !string.IsNullOrWhiteSpace((string?)i.Element(content + "encoded"))))
{
    var pSlug = (string?)page.Element(wp + "post_name") ?? "";
    if (string.IsNullOrWhiteSpace(pSlug)) continue;
    try
    {
        ConvertPost(page, pSlug, contentRoot, http, converter, attachmentsById, menus, wp, content,
                    warnings, flashTodo, imageFailures, alignedFigures, mediaTodo);
        pagesConverted++;
    }
    catch (Exception ex)
    {
        warnings.Add($"[error] page slug={pSlug}: {ex.Message}");
    }
}
Console.WriteLine($"Pages converted: {pagesConverted}");

int converted = 0;
foreach (var post in publishedPosts.Take(limit))
{
    var slug = (string?)post.Element(wp + "post_name") ?? "";
    if (string.IsNullOrWhiteSpace(slug))
    {
        warnings.Add($"[skip] post id={(string?)post.Element(wp + "post_id")} has no slug");
        continue;
    }
    try
    {
        ConvertPost(post, slug, outDir, http, converter, attachmentsById, menus, wp, content,
                    warnings, flashTodo, imageFailures, alignedFigures, mediaTodo);
        converted++;
        if (converted % 10 == 0) Console.WriteLine($"  converted {converted}/{Math.Min(limit, publishedPosts.Count)}");
    }
    catch (Exception ex)
    {
        warnings.Add($"[error] slug={slug}: {ex.Message}");
    }
}

File.WriteAllLines(Path.Combine(auditDir, "warnings.log"), warnings);
File.WriteAllLines(Path.Combine(auditDir, "flash-todo.tsv"), flashTodo);
File.WriteAllLines(Path.Combine(auditDir, "image-failures.tsv"), imageFailures);
File.WriteAllLines(Path.Combine(auditDir, "aligned-figures.tsv"),
    new[] { "slug\talignment\timage_src" }.Concat(alignedFigures));
File.WriteAllLines(Path.Combine(auditDir, "media-todo.tsv"),
    new[] { "slug\tattribute\turl" }.Concat(mediaTodo));

Console.WriteLine();
Console.WriteLine($"Converted: {converted}");
Console.WriteLine($"Warnings: {warnings.Count} → wxr-audit/warnings.log");
Console.WriteLine($"Flash TODOs: {flashTodo.Count} → wxr-audit/flash-todo.tsv");
Console.WriteLine($"Image failures: {imageFailures.Count} → wxr-audit/image-failures.tsv");
Console.WriteLine($"Aligned figures (alignleft/alignright): {alignedFigures.Count} → wxr-audit/aligned-figures.tsv");
Console.WriteLine($"Media TODO (videos to retrieve from local WP): {mediaTodo.Count} → wxr-audit/media-todo.tsv");
return 0;

// ----- conversion -----

static void ConvertPost(
    XElement post, string slug, string outDir, HttpClient http,
    ReverseMarkdown.Converter converter,
    Dictionary<string, string> attachmentsById,
    Dictionary<string, List<(int order, string label, string href)>> menus,
    XNamespace wp, XNamespace content,
    List<string> warnings, List<string> flashTodo, List<string> imageFailures,
    List<string> alignedFigures, List<string> mediaTodo)
{
    string title = FixMojibake(WebUtility.HtmlDecode(((string?)post.Element("title") ?? "").Trim()));
    // Normalise " - " separators in titles to en-dash for typographic consistency.
    title = Regex.Replace(title, @" - ", " – ");
    string dateGmt = (string?)post.Element(wp + "post_date_gmt") ?? "";
    string body = (string?)post.Element(content + "encoded") ?? "";
    string postId = (string?)post.Element(wp + "post_id") ?? "";

    var categories = post.Elements("category")
        .Where(c => (string?)c.Attribute("domain") == "category")
        .Select(c => (string?)c.Value ?? "")
        .Where(s => !string.IsNullOrEmpty(s))
        .ToList();
    var tags = post.Elements("category")
        .Where(c => (string?)c.Attribute("domain") == "post_tag")
        .Select(c => (string?)c.Value ?? "")
        .Where(s => !string.IsNullOrEmpty(s))
        .ToList();

    string bundleDir = Path.Combine(outDir, slug);
    Directory.CreateDirectory(bundleDir);

    string? Meta(string key) => post.Elements(wp + "postmeta")
        .FirstOrDefault(m => (string?)m.Element(wp + "meta_key") == key)
        ?.Element(wp + "meta_value")?.Value;

    var oldSlugs = post.Elements(wp + "postmeta")
        .Where(m => (string?)m.Element(wp + "meta_key") == "_wp_old_slug")
        .Select(m => m.Element(wp + "meta_value")?.Value ?? "")
        .Where(s => !string.IsNullOrEmpty(s))
        .ToList();

    // Featured image
    string? featuredFile = null;
    var thumbId = Meta("_thumbnail_id");
    if (!string.IsNullOrEmpty(thumbId) && attachmentsById.TryGetValue(thumbId, out var thumbUrl))
    {
        featuredFile = DownloadImage(thumbUrl, bundleDir, http, "featured", warnings, imageFailures);
    }

    // Open Graph / Twitter Card overrides (from Wonderm00n / webdados plugin)
    string? ogTitle = Meta("_webdados_fb_open_graph_specific_title")?.Trim();
    string? ogDescription = Meta("_webdados_fb_open_graph_specific_description")?.Trim();
    string? ogImageUrl = Meta("_webdados_fb_open_graph_specific_image")?.Trim();
    string? ogImageFile = null;
    if (!string.IsNullOrEmpty(ogImageUrl))
        ogImageFile = DownloadImage(ogImageUrl, bundleDir, http, "og-image", warnings, imageFailures);

    // 0. Aligned-figure audit: log every alignleft/alignright figure for review
    foreach (Match fig in Regex.Matches(body,
        @"<figure\b[^>]*\bclass=""[^""]*\b(?<align>alignleft|alignright)\b[^""]*""[^>]*>[\s\S]*?<img\b[^>]*\bsrc=""(?<src>[^""]+)""",
        RegexOptions.IgnoreCase))
    {
        alignedFigures.Add($"{slug}\t{fig.Groups["align"].Value}\t{fig.Groups["src"].Value}");
    }

    // 1. PlantUML pre-pass: extract source, write .puml, replace with placeholder
    int diagramIdx = 0;
    body = Regex.Replace(body, @"\[plantuml\](?<src>[\s\S]*?)\[/plantuml\]", m =>
    {
        diagramIdx++;
        string pumlName = $"diagram-{diagramIdx}.puml";
        string svgName = $"diagram-{diagramIdx}.svg";
        string source = m.Groups["src"].Value.Trim();
        File.WriteAllText(Path.Combine(bundleDir, pumlName), source);
        return $"§§MDIMG§§{svgName}§§PlantUML diagram§§";
    });

    // 2. SWF pre-pass: log + emit TODO marker
    body = Regex.Replace(body, @"\[swf\](?<args>[^\[]*?)\[/swf\]", m =>
    {
        var argsText = m.Groups["args"].Value.Trim();
        flashTodo.Add($"{slug}\t{argsText}");
        return $"§§MDFLASH§§{argsText}§§";
    });

    // 3. Syntaxhighlighter blocks → fenced code marker
    body = Regex.Replace(body,
        @"<!--\s*wp:syntaxhighlighter/code\s*(?<attrs>\{[^}]*\})?\s*-->\s*<pre[^>]*>(?<code>[\s\S]*?)</pre>\s*<!--\s*/wp:syntaxhighlighter/code\s*-->",
        m =>
        {
            string lang = "";
            var attrs = m.Groups["attrs"].Value;
            var langMatch = Regex.Match(attrs, "\"language\"\\s*:\\s*\"(?<l>[^\"]*)\"");
            if (langMatch.Success) lang = MapLang(langMatch.Groups["l"].Value);
            string code = HtmlDecodeText(m.Groups["code"].Value);
            code = StraightenQuotes(code);
            return $"§§MDCODE§§{lang}§§{Convert.ToBase64String(Encoding.UTF8.GetBytes(code))}§§";
        });

    // 3b. Fix cp437→UTF-8 mojibake (the WXR contains some titles where UTF-8
    //     en-dash bytes were once interpreted as cp437 then re-encoded).
    body = FixMojibake(body);

    // 3c. Resolve [menu name="X"] shortcodes from a WP menu-shortcode plugin.
    //     The menu definitions live in nav_menu_item items in the WXR; we expand
    //     them to inline HTML lists so the post becomes self-contained.
    body = Regex.Replace(body, @"\[menu\s+name=""(?<n>[^""]+)""\s*\]", m =>
    {
        var name = m.Groups["n"].Value;
        if (!menus.TryGetValue(name, out var list) || list.Count == 0)
            return "<p><em>Coming Soon!</em></p>";
        var sb = new StringBuilder("<ul>");
        foreach (var (_, label, href) in list)
            sb.Append($"<li><a href=\"{WebUtility.HtmlEncode(href)}\">{WebUtility.HtmlEncode(label)}</a></li>");
        sb.Append("</ul>");
        return sb.ToString();
    });

    // 4. URL rewrite — local-WP variants → live host (must precede image download)
    body = Regex.Replace(body, @"https?://(pranav-laptop:81|localhost:81)", "https://www.notadesigner.com");

    // 5. Strip Gutenberg block comments
    body = Regex.Replace(body, @"<!--\s*/?wp:[a-z0-9/_-]+(\s+\{[^}]*\})?\s*/?-->", "");

    // 6. Inline images: download, rewrite src to relative path
    body = Regex.Replace(body, @"<img\b[^>]*\bsrc\s*=\s*""(?<src>[^""]+)""[^>]*>", m =>
    {
        var src = m.Groups["src"].Value;
        if (src.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            string? local = DownloadImage(src, bundleDir, http, null, warnings, imageFailures);
            if (local != null)
                return m.Value.Replace(src, local);
        }
        return m.Value;
    });

    // 6b. Video tags: download src and poster, then replace the entire <video>
    //     element (and any wrapping <figure class="wp-block-video">) with a
    //     {{< video >}} Hugo shortcode. The shortcode resolves filenames via
    //     Page.Resources at render time, so URLs work in any rendering context
    //     (single page, home feed, RSS). Unreachable URLs go to media-todo.
    string ResolveMedia(string url)
    {
        if (!url.StartsWith("http", StringComparison.OrdinalIgnoreCase)) return url;
        string? local = DownloadImage(url, bundleDir, http, null, warnings, imageFailures);
        if (local == null) { mediaTodo.Add($"{slug}\t?\t{url}"); return url; }
        return local;
    }
    body = Regex.Replace(body,
        @"(?:<figure[^>]*class=""[^""]*wp-block-video[^""]*""[^>]*>)?\s*<video\b(?<attrs>[^>]*)>(?<inner>[\s\S]*?)</video>\s*(?:<figcaption[^>]*>(?<cap>[\s\S]*?)</figcaption>)?\s*(?:</figure>)?",
        m =>
        {
            string attrs = m.Groups["attrs"].Value;
            string srcRaw = Regex.Match(attrs, @"\bsrc=""(?<u>[^""]+)""").Groups["u"].Value;
            string posterRaw = Regex.Match(attrs, @"\bposter=""(?<u>[^""]+)""").Groups["u"].Value;
            string src = string.IsNullOrEmpty(srcRaw) ? "" : ResolveMedia(srcRaw);
            string poster = string.IsNullOrEmpty(posterRaw) ? "" : ResolveMedia(posterRaw);
            string caption = m.Groups["cap"].Value.Trim();
            var sc = new StringBuilder("{{< video src=\"");
            sc.Append(src).Append('"');
            if (!string.IsNullOrEmpty(poster)) sc.Append(" poster=\"").Append(poster).Append('"');
            if (!string.IsNullOrEmpty(caption)) sc.Append(" caption=\"").Append(caption.Replace("\"", "\\\"")).Append('"');
            sc.Append(" >}}");
            return sc.ToString();
        });

    // 6c. Anchors pointing into wp-content/uploads: download asset into the
    //     bundle, rewrite href to relative filename.
    body = Regex.Replace(body, @"<a\b[^>]*\bhref=""(?<u>https?://(www\.)?notadesigner\.com/wp-content/uploads/[^""]+)""", m =>
    {
        string url = m.Groups["u"].Value;
        string? local = DownloadImage(url, bundleDir, http, null, warnings, imageFailures);
        return local != null ? m.Value.Replace(url, local) : m.Value;
    });

    // 6d. Other anchors pointing to this site: strip scheme+host so links become
    //     root-relative. Hugo aliases on each post handle any old-slug redirects.
    body = Regex.Replace(body,
        @"<a\b(?<pre>[^>]*?)\bhref=""https?://(www\.)?notadesigner\.com(?<path>/[^""]*)""",
        m => $"<a{m.Groups["pre"].Value}href=\"{m.Groups["path"].Value}\"");

    // 8. HTML → Markdown
    string md = converter.Convert(body);

    // 8b. Decode lingering HTML entities in body prose (e.g. &lt;TParameter&gt; in
    //     paragraph text that ReverseMarkdown passed through verbatim). Markers
    //     are ASCII-only so they survive untouched.
    md = WebUtility.HtmlDecode(md);

    // 8c. Replace U+00A0 (non-breaking space) with regular space — WP autop
    //     scatters these into prose and they break grep + word wrapping.
    md = md.Replace(' ', ' ');

    // 9. Restore markers
    md = Regex.Replace(md, @"§§MDIMG§§(?<file>[^§]+)§§(?<alt>[^§]*)§§",
        m => $"![{m.Groups["alt"].Value}]({m.Groups["file"].Value})");
    md = Regex.Replace(md, @"§§MDFLASH§§(?<args>[^§]*)§§",
        m => $"\n> **[TODO: Flash embed — replace with screenshot]** Original SWF: `{m.Groups["args"].Value}`\n");
    md = Regex.Replace(md, @"§§MDCODE§§(?<lang>[^§]*)§§(?<b64>[^§]+)§§", m =>
    {
        string code = Encoding.UTF8.GetString(Convert.FromBase64String(m.Groups["b64"].Value));
        string lang = m.Groups["lang"].Value;
        return $"\n```{lang}\n{code.TrimEnd()}\n```\n";
    });

    // 10. Tidy whitespace
    md = Regex.Replace(md, @"\r\n", "\n");
    md = Regex.Replace(md, @"\n{3,}", "\n\n").Trim() + "\n";

    // 10a. Straighten smart quotes inside inline code spans (`...`). Code in
    //      fenced blocks is already straightened pre-base64 in step 3.
    md = Regex.Replace(md, @"(?<!`)`(?<inner>[^`\r\n]+)`(?!`)",
        m => "`" + StraightenQuotes(m.Groups["inner"].Value) + "`");

    // 10b. Blockquote cleanup: drop leading/trailing/consecutive empty `>` lines
    //      (artifacts of WP autop padding empty <p></p> inside <blockquote>).
    md = CleanBlockquotes(md);

    // 10c. Per-post editorial hand edits the converter cannot reproduce
    //      mechanically (idempotent — Replace is no-op when source already changed).
    if (slug == "how-to-write-unmaintainable-code-php-redux")
        md = ApplyPhpReduxEdits(md);

    // 11. Frontmatter
    var sb = new StringBuilder();
    sb.AppendLine("---");
    sb.AppendLine($"title: {YamlScalar(title)}");
    if (DateTime.TryParse(dateGmt, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var dt))
        sb.AppendLine($"date: {dt:yyyy-MM-ddTHH:mm:ssZ}");
    sb.AppendLine($"slug: {slug}");
    var aliases = new List<string> { $"/{slug}/" };
    foreach (var os in oldSlugs) aliases.Add($"/{os}/");
    sb.AppendLine($"aliases: [{string.Join(", ", aliases.Select(a => $"\"{a}\""))}]");
    if (!string.IsNullOrEmpty(ogTitle))
        sb.AppendLine($"og_title: {YamlScalar(ogTitle)}");
    if (!string.IsNullOrEmpty(ogDescription))
        sb.AppendLine($"description: {YamlScalar(ogDescription)}");
    string? socialImage = ogImageFile ?? featuredFile;
    if (socialImage != null)
        sb.AppendLine($"images: [\"{socialImage}\"]");
    if (categories.Count > 0)
    {
        sb.AppendLine("categories:");
        foreach (var c in categories) sb.AppendLine($"  - {YamlScalar(c)}");
    }
    if (tags.Count > 0)
    {
        sb.AppendLine("tags:");
        foreach (var t in tags) sb.AppendLine($"  - {YamlScalar(t)}");
    }
    if (featuredFile != null)
        sb.AppendLine($"featured_image: {featuredFile}");
    sb.AppendLine($"wp_post_id: {postId}");
    sb.AppendLine("---");
    sb.AppendLine();
    sb.Append(md);

    File.WriteAllText(Path.Combine(bundleDir, "index.md"), sb.ToString(), new UTF8Encoding(false));
}

// Replace WP-textured smart quotes with straight ASCII for code contexts.
// FixMojibake's body-wide pass doesn't reach syntaxhighlighter content because
// it's extracted to a base64 marker before that step runs, so the ellipsis
// replacement also has to live here.
static string StraightenQuotes(string s) => s
    .Replace('“', '"').Replace('”', '"')
    .Replace('‘', '\'').Replace('’', '\'')
    .Replace("…", "...")
    .Replace("┬á", " ");   // cp437 view of UTF-8 NBSP — survives inside captured code blocks

// Editorial hand edits for the PHP-redux post. The original WP block stuffed
// five PHP tag styles + // comments into one fenced block, which broke Chroma.
// We split into one block per style with prose lead-ins, and switch the two
// mixed PHP+HTML blocks from the `php` lexer to `html` (Chroma handles short
// tags better in HTML host). Replaces are no-ops if the source isn't present.
static string ApplyPhpReduxEdits(string md)
{
    const string oldB1 =
"""
```php
<? echo "Hello, World!"; > // What is referred to as the "old style".

<? echo "Hello, World!"; ?> // What is referred to as the "first new style". Note the added question mark in the end tag.

<?php echo "Hello, World!"; ?> // The "second new style" is similar to the first new style and most recommended

<script language="php">
    echo "Hello, World!"; > // What else? The "third new style" of tags
</script>

<% echo "Hello, World!"; %> // And finally, a fourth way that uses ASP-style tags
```
""";
    const string newB1 =
"""
The "old style":

```php
<? echo "Hello, World!"; >
```

The "first new style". Note the added question mark in the end tag:

```php
<? echo "Hello, World!"; ?>
```

The "second new style" is similar to the first new style and most recommended:

```php
<?php echo "Hello, World!"; ?>
```

What else? The "third new style" of tags:

```html
<script language="php">
    echo "Hello, World!";
</script>
```

And finally, a fourth way that uses ASP-style tags:

```php
<% echo "Hello, World!"; %>
```
""";
    md = md.Replace(oldB1, newB1);
    md = md.Replace("```php\n<?php if (foo == null):", "```html\n<?php if (foo == null):");
    md = md.Replace("```php\n<div class=\"query_output\">", "```html\n<div class=\"query_output\">");
    return md;
}

// Targeted fix for cp437→UTF-8 round-trip mojibake found in the WXR.
static string FixMojibake(string s) => s
    .Replace("ΓÇô", "–")
    .Replace("ΓÇö", "—")
    .Replace("ΓÇ£", "“")
    .Replace("ΓÇ¥", "”")
    .Replace("ΓÇÖ", "’")
    .Replace("ΓÇÿ", "‘")
    .Replace("ΓÇ¦", "…")
    .Replace("┬á", " ")    // cp437 view of UTF-8 NBSP (0xC2 0xA0) → regular space
    .Replace("…", "...");  // typographic ellipsis → three dots (author preference)

static string CleanBlockquotes(string md)
{
    var lines = md.Split('\n').ToList();
    bool isBareQuote(string l) => Regex.IsMatch(l, @"^>\s*$");
    bool isQuote(string l) => l.StartsWith(">");

    // Walk runs of consecutive blockquote lines; trim leading/trailing bare `>`
    // and collapse internal multiple bare `>` into a single one.
    int i = 0;
    var result = new List<string>();
    while (i < lines.Count)
    {
        if (!isQuote(lines[i])) { result.Add(lines[i++]); continue; }
        int start = i;
        while (i < lines.Count && isQuote(lines[i])) i++;
        var run = lines.GetRange(start, i - start);
        // trim leading bare
        while (run.Count > 0 && isBareQuote(run[0])) run.RemoveAt(0);
        // trim trailing bare
        while (run.Count > 0 && isBareQuote(run[^1])) run.RemoveAt(run.Count - 1);
        // collapse internal runs of bare `>` to a single one
        for (int j = run.Count - 1; j > 0; j--)
            if (isBareQuote(run[j]) && isBareQuote(run[j - 1])) run.RemoveAt(j);
        result.AddRange(run);
    }
    return string.Join("\n", result);
}

static string YamlScalar(string s)
{
    if (string.IsNullOrEmpty(s)) return "\"\"";
    s = s.Replace("\"", "\\\"");
    return $"\"{s}\"";
}

static string MapLang(string wpLang) => wpLang switch
{
    "as3" => "actionscript",
    "csharp" or "cs" => "csharp",
    "js" or "jscript" => "javascript",
    "" => "",
    _ => wpLang,
};

static string HtmlDecodeText(string s)
{
    // Iterate to handle double-encoding (e.g. &amp;lt; → &lt; → <), capped to
    // avoid runaway on pathological input.
    for (int i = 0; i < 3; i++)
    {
        var next = WebUtility.HtmlDecode(s);
        if (next == s) break;
        s = next;
    }
    return s;
}

static string? DownloadImage(string url, string bundleDir, HttpClient http,
    string? overrideStem, List<string> warnings, List<string> imageFailures)
{
    try
    {
        url = Regex.Replace(url, @"https?://(pranav-laptop:81|localhost:81)", "https://www.notadesigner.com");
        Uri uri = new(url);
        string remoteName = Path.GetFileName(uri.LocalPath);
        if (string.IsNullOrEmpty(remoteName)) remoteName = "image.bin";
        string ext = Path.GetExtension(remoteName);
        string stem = overrideStem ?? Path.GetFileNameWithoutExtension(remoteName);
        string filename = stem + ext;

        string target = Path.Combine(bundleDir, filename);
        if (!File.Exists(target))
        {
            using var resp = http.GetAsync(url).Result;
            if (!resp.IsSuccessStatusCode)
            {
                imageFailures.Add($"{url}\t{(int)resp.StatusCode}");
                return null;
            }
            var bytes = resp.Content.ReadAsByteArrayAsync().Result;
            File.WriteAllBytes(target, bytes);
        }
        return filename;
    }
    catch (Exception ex)
    {
        imageFailures.Add($"{url}\t{ex.Message}");
        return null;
    }
}

static void WriteAuditManifest(List<XElement> posts, Dictionary<string, string> attachments,
    XNamespace wp, XNamespace content, string auditDir)
{
    var manifest = new List<string> { "post_id\tdate\tslug\tcategories\ttags\tfeatured\tcontent_chars\tplantuml\tswf\ttable" };
    foreach (var post in posts)
    {
        var postId = (string?)post.Element(wp + "post_id") ?? "";
        var slug = (string?)post.Element(wp + "post_name") ?? "";
        var dateStr = (string?)post.Element(wp + "post_date_gmt") ?? "";
        var body = (string?)post.Element(content + "encoded") ?? "";
        var cats = string.Join(",", post.Elements("category")
            .Where(c => (string?)c.Attribute("domain") == "category")
            .Select(c => (string?)c.Attribute("nicename") ?? ""));
        var tags = string.Join(",", post.Elements("category")
            .Where(c => (string?)c.Attribute("domain") == "post_tag")
            .Select(c => (string?)c.Attribute("nicename") ?? ""));
        var thumbId = post.Elements(wp + "postmeta")
            .FirstOrDefault(m => (string?)m.Element(wp + "meta_key") == "_thumbnail_id")
            ?.Element(wp + "meta_value")?.Value ?? "";
        string featured = !string.IsNullOrEmpty(thumbId) && attachments.TryGetValue(thumbId, out var u) ? u : "";
        manifest.Add(string.Join("\t",
            postId, dateStr, slug, cats, tags, featured,
            body.Length.ToString(CultureInfo.InvariantCulture),
            CountOccurrences(body, "[plantuml"),
            CountOccurrences(body, "[swf"),
            CountOccurrences(body, "[table")));
    }
    File.WriteAllLines(Path.Combine(auditDir, "manifest.tsv"), manifest);
}

static int CountOccurrences(string h, string n)
{
    if (string.IsNullOrEmpty(h) || string.IsNullOrEmpty(n)) return 0;
    int c = 0, i = 0;
    while ((i = h.IndexOf(n, i, StringComparison.Ordinal)) != -1) { c++; i += n.Length; }
    return c;
}
