# Special URLs Audit — WordPress → Hugo

Audit target: `D:\projects\notadesigner.com\hugo-site\`
Build artifacts under `hugo-site/public/` are present and were used for spot-checks.

Summary: **8 PASS / 0 FAIL / 1 NEEDS REVIEW**

---

## 1. Permalink structure for posts — **PASS**

WP rule: `/%postname%/` (slug only). Hugo config matches:

`hugo-site/hugo.toml:17-19`
```
[permalinks]
  [permalinks.page]
    posts = "/:slug/"
```

Spot-check: post bundles render at `/<slug>/` (e.g. `hugo-site/public/varchar50/index.html`, `hugo-site/public/notes-on-ray-tracing-in-one-weekend/index.html`) — no `/posts/<slug>/` and no date prefix.

---

## 2. Category URLs — **PASS**

WP rule: `/category/<slug>/`. Hugo config matches:

`hugo-site/hugo.toml:20-22`
```
[permalinks.term]
  categories = "/category/:slug/"
  tags       = "/tag/:slug/"
```

Spot-check (build output): `hugo-site/public/category/foundation/index.html`, `…/construction/index.html`, `…/software-engineering/index.html`, `…/travel-diaries/index.html`, etc. Pagination exists under `category/<slug>/page/N/`.

---

## 3. Tag URLs — **PASS**

Same `[permalinks.term]` block at `hugo-site/hugo.toml:22` (`tags = "/tag/:slug/"`).

Spot-check (build output): `hugo-site/public/tag/.net/index.html`, `tag/asp.net-mvc/index.html`, `tag/practical-design-patterns/index.html`, etc., plus `tag/<slug>/page/N/` pagination.

---

## 4. Feed URL `/feed/` — **PASS**

Dual-emission strategy is in place:

- Section content file forces both outputs at `/feed/`:
  `hugo-site/content/feed/_index.md:3-4`
  ```
  url: "/feed/"
  outputs: ["RSS", "HTML"]
  ```
- Layouts route both formats to the same RSS body:
  - `hugo-site/layouts/feed/section.rss.xml:1` — `{{- partial "feed-body.html" . -}}`
  - `hugo-site/layouts/feed/section.html:1` — `{{- partial "feed-body.html" . -}}`
  - Shared partial: `hugo-site/layouts/_partials/feed-body.html`
- Build artifacts confirm both files exist:
  - `hugo-site/public/feed/index.xml` (RSS body, valid `<rss version="2.0">`)
  - `hugo-site/public/feed/index.html` (same RSS body, served as HTML mime so content-sniffing readers accept it)
- Autodiscovery link points at `/feed/index.xml`:
  `hugo-site/layouts/_partials/docs/inject/head.html:2`
  ```
  <link rel="alternate" type="application/rss+xml" title="{{ .Site.Title }}" href="{{ "/feed/index.xml" | absURL }}">
  ```
- Per-section auto-RSS is suppressed site-wide:
  `hugo-site/hugo.toml:42-48`
  ```
  [outputs]
    home     = ["HTML"]
    section  = ["HTML"]
    taxonomy = ["HTML"]
    term     = ["HTML"]
  ```
  This guarantees `/feed/` is the sole canonical RSS; `posts/index.xml`, `category/<x>/index.xml`, etc. are not generated.

---

## 5. Sitemap — **PASS**

No `sitemap` entry in `disableKinds` (`hugo-site/hugo.toml:14` is `disableKinds = []`). Built artifact present at `hugo-site/public/sitemap.xml`. Hugo's default emission is therefore active.

---

## 6. robots.txt — **NEEDS REVIEW**

Hugo config: `enableRobotsTXT = true` (`hugo-site/hugo.toml:9`). However there is no custom `layouts/robots.txt` template and no built `hugo-site/public/robots.txt` artifact (Glob returned no match). With `enableRobotsTXT = true` Hugo will emit a default permissive `User-agent: *` / `Disallow:` plus `Sitemap:` line on the next build, but it has not been generated yet (or was skipped).

The live WordPress robots.txt at `https://notadesigner.com/robots.txt` is non-trivial:
- Sets `Content-Signal: search=yes, ai-train=no`
- Disallows multiple AI crawlers (ClaudeBot, GPTBot, Amazonbot, etc.)
- Disallows WP admin paths except `admin-ajax.php`
- Cites EU Directive 2019/790 reservation
- Points `Sitemap:` at `/wp-sitemap.xml`

Action required:
- Decide whether to preserve the AI-crawler block list and `Content-Signal` header on the Hugo site. If yes, author `hugo-site/layouts/robots.txt` (or a static `static/robots.txt`) carrying those directives plus the new `Sitemap: https://notadesigner.com/sitemap.xml`.
- The `wp-sitemap.xml` URL will go away post-cutover; the new sitemap line should reference `/sitemap.xml`.

Marking NEEDS REVIEW because the file is not yet emitted and the WP version contains intentional policy that should not be silently dropped.

---

## 7. 404 page — **PASS**

Built artifact present: `hugo-site/public/404.html`. Hugo's default 404 is active (theme-provided); no `disableKinds` suppression.

---

## 8. Pagination URLs — **PASS**

WP convention: `/page/N/`. Hugo's default `paginatePath` is `page`, and is not overridden in `hugo-site/hugo.toml` (the only pagination config is `[pagination] pagerSize = 10` at lines 28-29).

Spot-check:
- `hugo-site/public/posts/page/2/index.html` … `posts/page/9/index.html`
- `hugo-site/public/category/construction/page/2/index.html` … `page/4/index.html`
- `hugo-site/public/tag/c/page/2/…/page/5/index.html`

Note: a `page/1/` directory is also emitted by Hugo (e.g. `category/foundation/page/1/index.html`). Inspection shows it canonicals back to the un-paginated section URL (`<link rel="canonical" href=".../category/foundation/">`), so duplicates are handled correctly for SEO and there is no `/page/1/` URL on WP to mismatch against.

---

## 9. Trailing slash policy — **PASS**

- `canonifyURLs = false` (`hugo-site/hugo.toml:13`) — Hugo defaults stand.
- No `uglyURLs` setting anywhere in `hugo.toml`; no per-content-type override.
- All built sections and posts emit `…/index.html` under directory-style paths, which servers expose as trailing-slash URLs (`/<slug>/`, `/category/<slug>/`, `/tag/<slug>/`, `/posts/page/N/`). Matches WP's trailing-slash convention.
- The post permalink `posts = "/:slug/"` itself terminates with a slash.

---

## Tally

| Check | Result |
|---|---|
| 1. Post permalink structure | PASS |
| 2. Category URLs | PASS |
| 3. Tag URLs | PASS |
| 4. /feed/ URL (dual emission + autodiscovery + per-section disable) | PASS |
| 5. Sitemap | PASS |
| 6. robots.txt | NEEDS REVIEW |
| 7. 404 page | PASS |
| 8. Pagination URLs | PASS |
| 9. Trailing slash policy | PASS |
