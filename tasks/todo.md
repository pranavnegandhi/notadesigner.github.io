# Migration: WordPress → Hugo on GitHub Pages

## Goal

Replace the current `Markdown → paste into WP → Simply Static → zip → GitHub` pipeline with `Markdown → git push → GitHub Actions → Pages`. Drop WordPress entirely. Preserve every existing URL, every featured image, every code block, and the year-grouped Archives page.

## Audit results (from `notadesigner.WordPress.2026-05-06.xml`)

- 103 posts, 146 attachments, 2 pages
- Permalink structure: `/%postname%/` (slug only, no date)
- Shortcodes in use: `[plantuml]` × 13, `[swf]` × 3, `[table]` × 1
- Gutenberg blocks: core blocks + `wp:syntaxhighlighter/code`
- WXR was exported from local laptop (`http://pranav-laptop:81`) — internal links need rewriting to `notadesigner.com`

## Phase 1 — Converter (WXR → Hugo page bundles) ✅ COMPLETE

- [x] Scaffold C# console app (`dotnet new console`); NuGet: `AngleSharp`, `ReverseMarkdown`
- [x] Parse WXR, iterate over `wp:post_type = post` items (skip drafts/trash)
- [x] For each post, create `content/posts/<slug>/index.md` with frontmatter:
  - `title`, `date`, `slug`, `categories[]`, `tags[]`, `aliases[]`, `featured_image`, `images[]`, `wp_post_id`
  - OG/Twitter overrides from `_webdados_fb_open_graph_specific_*` postmeta
  - `_wp_old_slug` postmeta merged into `aliases[]` for URL parity
- [x] Featured image: resolve `_thumbnail_id` → attachment URL → download as `featured.<ext>`
- [x] Inline images: download into bundle, rewrite `src` to relative
- [x] Body conversion: HTML → Markdown via `ReverseMarkdown`, with custom rules:
  - [x] `wp:syntaxhighlighter/code` → fenced code block with language tag (base64 marker round-trip protects code from MD escaping)
  - [x] `[plantuml]…[/plantuml]` → save `diagram-N.puml` in bundle + `![](diagram-N.svg)` reference (SVG generation deferred to Phase 6 render script)
  - [x] `[swf …]` → blockquote TODO marker + slug logged to `flash-todo.tsv`
  - [x] `[table …]` — only instance was in a draft, no action needed
  - [x] `[menu name="…"]` resolved against `nav_menu_item` items → inline `<ul>` of links; "Coming Soon!" fallback for empty/placeholder menus
  - [x] Strip `<!-- wp:* -->` block comments and `<p>` autop noise
  - [x] Empty `>` blockquote-line cleanup (collapse leading/trailing/duplicate)
- [x] URL rewrite: `pranav-laptop:81` and `localhost:81` → `www.notadesigner.com` (centralized in downloader)
- [x] Internal links: rewrite absolute `notadesigner.com/<path>` → root-relative `/<path>/`
- [x] `wp-content/uploads` linked assets (zips, etc.) downloaded into bundle, links rewritten to relative
- [x] HTML5 `<video>` tags: download `src` and `poster`, rewrite to relative; `media-todo.tsv` for unreachable URLs
- [x] HTML entity decode (iterative, handles double-encoding) on title and body
- [x] NBSP (U+00A0) → regular space
- [x] cp437→UTF-8 mojibake fix (`ΓÇô` → `–` etc.)
- [x] About page (and any other content pages) → `content/<slug>/index.md`; Archives page skipped (Hugo regenerates)
- [x] Aligned-figure audit: `aligned-figures.tsv` lists every `alignleft`/`alignright` figure for theme review
- [x] Log warnings, image failures, flash TODOs, media TODOs to `wxr-audit/` artifacts
- [x] Iterative dry runs across years; final state: 87 posts converted, 0 warnings, 0 image failures

### Converter is frozen — post-conversion fixups live in `scripts/`

Decision 2026-05-10: the converter (`tools/wxr-to-hugo/`) is treated as bootstrap-only.
The migrated markdown is now the source of truth. Two mechanical issues turned up
post-conversion and were fixed by standalone PowerShell scripts rather than by
modifying and re-running the converter:

- `scripts/fix-figure-captions.ps1` — wraps the WP-era `image\ncaption` markdown
  pattern in `<figure>/<figcaption>` HTML. Idempotent. Scope: 56 occurrences across
  16 posts.
- `scripts/scan-thumbnails.ps1` — scans posts for size-suffix thumbnails
  (`*-WxH.ext`) where a larger variant exists in the WP archive. Read-only.
- `scripts/upgrade-thumbnails.ps1` — applies a hand-curated list of thumbnail-to-
  full-size swaps, copying the full-size from the WP archive and deleting the
  orphaned thumbnail. Scope: 20 swaps across 8 posts.

Order if you ever bootstrap from WXR again: converter → fix-figure-captions →
scan-thumbnails (review output) → curate the upgrade list in upgrade-thumbnails →
upgrade-thumbnails → hand-replace `[swf]` TODO markers with `{{< flash >}}` shortcodes.

## Phase 2 — Hugo site scaffold ✅ COMPLETE (with carry-overs)

- [x] Hugo site scaffolded at `hugo-site/`, content from converter wired in
- [x] `hugo.toml` with baseURL, taxonomies (categories/tags), pagination, RSS, English locale, image processing defaults
- [x] Permalinks: `posts → /:slug/`, `categories → /category/:slug/`, `tags → /tag/:slug/` (matches WP)
- [x] Chroma syntax highlighting via Hugo's built-in pipeline; **GitHub** stylesheet generated to `static/css/chroma.css` and injected via Hugo Book's `_partials/docs/inject/head.html`
- [x] Goldmark `unsafe = true` so raw HTML (e.g. `<figure>`) passes through
- [x] Hugo extended portable binary at `tools/hugo/hugo.exe` (v0.161.1) — pinned, no global install
- [x] **Theme decision (revised):** Hugo Book (`themes/hugo-book/`) cloned as a git checkout. Original "from-zero" decision changed mid-Phase-2 in favour of typography handled by a real theme. Custom layouts in `layouts/` override only what's needed.
- [x] Custom layouts/templates added on top of Hugo Book:
  - `_default/archives.html` — year-grouped archive with 60×60 thumbnail crops (via Hugo image processing, smart-fill crop), placeholder squares for posts with no featured image, explicit flex/text-align so layout is consistent regardless of thumbnail presence
  - `feed/section.rss.xml` — custom RSS feed at `/feed/index.xml` using `site.RegularPages` (30 most recent)
  - `shortcodes/recent-posts.html` — full-content article listing on the home page
  - `shortcodes/video.html` — `Page.Resources`-based video embed (works in any rendering context)
  - `_partials/docs/inject/head.html` — adds `chroma.css` link
- [x] Home page (`content/_index.md`) — intro + "Recent posts" with full bodies + "Archives" section linking to `/archives/`. Right-side TOC auto-includes both headings for visibility.
- [x] Archives page (`content/archives/_index.md` with `layout: archives`) — Hugo regenerates; not a converted WP page.
- [x] **Robust media URL resolution** (durable beyond migration):
  - Markdown `![alt](file.jpg)` — Hugo Book's render-image hook + `BookPortableLinks = "warning"` resolves relative paths against `Page.Resources` so URLs are absolute regardless of rendering context
  - Videos — converter emits `{{< video >}}` shortcode that resolves via `Page.Resources.GetMatch`; works on home, RSS, single page identically
  - Same plumbing handles future-authored posts unchanged
- [x] OG/Twitter Card meta tags via Hugo internal templates (already wired by Hugo Book + frontmatter)
- [x] Smoke-tested URLs (200): home, archives, sample post, about, RSS, category, tag

### Carry-overs from Phase 2 (small)

- [x] **`/feed/` URL** — fixed. Hugo emits the same RSS body to both `/feed/index.xml` (application/xml mime, canonical) and `/feed/index.html` (text/html mime, makes bare `/feed/` resolve on GitHub Pages). RSS readers content-sniff the XML body and accept either. Autodiscovery `<link rel="alternate">` points at the canonical `/feed/index.xml`. Hugo's per-section auto-RSS disabled site-wide so there's exactly one feed.
- [x] **Sidebar order** — chronological (theme default). An alphabetical override was tried 2026-05-10 and reverted the same day. No project override; theme's `menu-filetree.html` is used as-is.
- [x] **Featured image on post pages** — `layouts/posts/single.html` overrides theme template; renders `featured.*` resource between meta line and body, conditionally (no placeholder when absent). Image processed to 1200px wide WebP via Hugo image pipeline.

### Notes for editorial review (carried forward)

- Aligned-figure list (`wxr-audit/aligned-figures.tsv`): 41 figures across 10 posts where original WP layout used `alignleft`/`alignright` text wrapping; current theme renders them centred. User accepted this trade-off.
- Flash TODOs (3 markers in 2 posts) still need screenshots from the live site (Phase 5).
- PlantUML SVG render still pending (Phase 6); 10 `.puml` source files in bundles, but no rendered SVGs yet — those references will 404 until the render-puml script runs.

## Phase 3 — URL parity audit ✅ COMPLETE

Run via 4 parallel agents; outputs in `tasks/phase3/`:
- `live-urls.txt` (206 URLs from `wp-sitemap.xml`)
- `hugo-urls.txt` (222 URLs from `public/` walk)
- `expected-urls.txt` (87 canonical posts + 97 frontmatter aliases)
- `special-urls-audit.md` (8 PASS / 1 NEEDS REVIEW → robots.txt)
- `parity-report.md` (consolidated diff + close-out decisions)

- [x] Crawl live `notadesigner.com` (sitemap-driven, 206 URLs)
- [x] Build the Hugo site locally; enumerate every output path (222 URLs)
- [x] Diff the two lists
- [x] **Posts:** 87/87 canonical URLs match slug-for-slug, no aliases needed
- [x] **Pages:** `/`, `/about/`, `/archives/` all match
- [x] **Feed:** `/feed/` matches (dual XML+HTML emission verified)
- [x] **Aliases:** all 9 legacy slug redirects emit (`/100/`, `/29/`, `/531/`, `/libraries-or-bust/`, `/nothing-so-simple-that-it-cannot-be-difficult/`, `/practical-design-patterns-in-c-strategy/`, `/practicing-programmin/`, `/symptoms-of-competence-1/`, `/users-dont-read-error-dialogs/`)
- [x] **`robots.txt`:** authored `layouts/robots.txt` preserving live policy verbatim (Content-Signal preamble, EU Directive 2019/790 reservation, Cloudflare-managed AI-crawler blocklist: Amazonbot, Applebot-Extended, Bytespider, CCBot, ClaudeBot, CloudflareBrowserRenderingCrawler, Google-Extended, GPTBot, meta-externalagent); appended `Sitemap:` line pointing to `/sitemap.xml`. Required `[outputs] home = ["HTML", "ROBOTS"]` in `hugo.toml` (the explicit outputs block was overriding `enableRobotsTXT`).
- [x] **Author archive `/author/pranavnegandhi/`:** built as a section listing all 87 posts paginated 10/page (9 pages); `content/author/pranavnegandhi/_index.md` + `layouts/author/list.html` (mirrors the archive layout — 60×60 thumbnails, date, numeric pager).

### Accepted gaps (parity-report.md §3, §4, §5, §6, §8 — explicit decisions 2026-05-08)

- [x] **Category nesting** — WP `/category/software-engineering/<child>/` vs Hugo flat `/category/<child>/`. Accepted as-is.
- [x] **Tag slug typography** — WP `asp-net`/`asp-net-mvc`/`net` vs Hugo `asp.net`/`asp.net-mvc`/`.net`. Accepted as-is.
- [x] **Home pagination** — WP `/page/2..18/` vs Hugo `/posts/page/2..9/`. Accepted; sitemap covers all posts.
- [x] **Date archives** — 47 monthly URLs not emitted. Not required.
- [x] **`/comments/feed/`** — not emitted (comments dropped per Phase 1 decision).

## Phase 4 — Flash triage + cutover

- [x] **Flash via Ruffle (decision 2026-05-09):** instead of static screenshots, the 3 SWFs are re-embedded interactively via self-hosted Ruffle. Renders better in browser (WebGL) than the native Windows Vulkan path. Commitment > screenshot.
  - [x] `scripts/update-ruffle.ps1` — fetches latest Ruffle nightly web-selfhosted bundle from GitHub, extracts to `static/ruffle/`, writes `.version` tag. Re-runnable for upgrades.
  - [x] `static/ruffle/` populated with `nightly-2026-05-09`
  - [x] SWFs staged into post bundles: `line.swf`, `wave.swf`, `underwater.swf`
  - [x] `underwater.swf`'s baked-in `../media/fish.jpg` reference satisfied by mirroring `wp-content/uploads/2012/media/fish.jpg` inside its post bundle
  - [x] `layouts/shortcodes/flash.html` — Page.Resources-resolved SWF URL, click-to-play via Ruffle's `autoplay:auto`, optional `baseRel` param to override SWF base URL for legacy relative-path resolution. `safeJS` filter on jsonify outputs (Hugo's `<script>` context double-escapes otherwise). Ruffle `ruffle.js` script tag emitted exactly once per page via `Page.Scratch`.
  - [x] 3 TODO markers replaced with `{{< flash >}}` invocations across 2 posts
  - [x] Smoke-test: all three play in Firefox at localhost:1313
- [x] `[table]` shortcode — only instance was in a draft (Phase 1 audit), no action needed
- [ ] Final QA: spot-check 20+ posts across years for rendering glitches
- [ ] Cutover: push to `main`, verify Pages deploy, confirm live site
- [ ] Decommission: shut down WP laptop, archive WXR + `wp-content/uploads/` to cold storage

## Phase 5 — Deploy via GitHub Actions

- [ ] Reuse existing `notadesigner.com` repo with new branch structure: `main` = source, `gh-pages` = built output)
- [ ] `.github/workflows/deploy.yml`: checkout → setup-hugo → `hugo --minify` → deploy to Pages
- [ ] Test deploy to a staging path before flipping production

## Phase 6 — Steady-state authoring loop

- [ ] Document the new flow in `CLAUDE.md`:
  - `mkdir content/posts/<slug>/`, drop `index.md` + `featured.jpg` + any `.puml`
  - For PlantUML: run `scripts/render-puml.ps1` (writes `.svg` next to `.puml`)
  - `hugo server` for local preview
  - `git push` to publish
- [ ] Write `scripts/render-puml.ps1` (wraps `plantuml.jar -tsvg` over all `.puml` files in `content/`)
- [ ] Write `scripts/new-post.ps1` (scaffolds bundle directory with frontmatter template)

## Decisions (locked)

- **Converter language:** C# console app (`AngleSharp` + `ReverseMarkdown` + `XDocument`)
- **Hugo theme:** built from zero, no parent
- **Comments:** disabled — drop entirely, no preservation needed
- **Search:** none
- **RSS:** `/feed/` URL must stay byte-identical (subscribers depend on it)

## Known unknowns to resolve during Phase 1

- The 1 `[table]` shortcode — needs eyeball
- The `wp:syntaxhighlighter/code` block's exact serialization in the WXR — may need conversion tweaks
- Long-tail HTML weirdness in older posts (classic editor era) — discover via dry-run warnings log
