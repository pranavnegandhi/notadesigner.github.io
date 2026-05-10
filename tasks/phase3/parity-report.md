# Phase 3 — URL parity report (final)

Consolidated diff between live `notadesigner.com` (206 URLs) and the local Hugo build (post-fix: 230 URLs). Source data:

- `live-urls.txt` — sitemap-driven enumeration of WP
- `hugo-urls.txt` — walk of `hugo-site/public/` after a clean build (pre-fix snapshot; 222 URLs)
- `expected-urls.txt` — slugs + aliases inventoried from frontmatter
- `special-urls-audit.md` — config audit

**Status: closed.** Decisions taken 2026-05-08 are recorded inline below.

## Headline result

| Bucket | Live | Hugo (final) | Decision |
|---|---|---|---|
| Posts | 87 | 87 | ✅ exact match (slug-for-slug) |
| Pages (`/`, `/about/`, `/archives/`) | 3 | 3 | ✅ |
| Categories | 7 | 7 | ⚠ different URLs — **accepted as-is** (§3) |
| Tags | 40 | 41 | ⚠ 3 slug mismatches — **accepted as-is** (§4) |
| Feeds | 2 | 1 | ⚠ `/comments/feed/` dropped — **accepted** (§8) |
| Pagination (home) | 17 (`/page/2..18/`) | 0 | ❌ **accepted** — sitemap covers all posts (§5) |
| Date archives | 47 monthly | 0 | ❌ **accepted** — not required (§6) |
| Author archive | 1 | 9 (paginated) | ✅ **implemented** (§7) |
| Aliases (legacy slugs) | n/a | 9 | ✅ all covered |
| `robots.txt` | full policy | full policy + `Sitemap:` | ✅ **implemented** (§9) |

87 of 87 canonical post URLs resolve identically. The two surface-area items the project committed to fix (author archive, robots.txt) are done; everything else was explicitly accepted as a permanent gap.

---

## 1. Posts — clean ✅

Set difference live∖hugo and hugo∖live on the POSTS section is empty. Every WP post URL has a matching Hugo URL with identical slug. `expected-urls.txt` confirms 87/87 with no dir-name vs slug drift.

## 2. Pages — clean ✅

Live `/`, `/about/`, `/archives/` all resolve in Hugo.

## 3. Categories — accepted as-is ⚠

WordPress emitted nested category URLs reflecting the parent/child taxonomy. Hugo flattens.

| Live URL | Hugo URL |
|---|---|
| `/category/miscellaneous/` | `/category/miscellaneous/` ✅ |
| `/category/software-engineering/` | `/category/software-engineering/` ✅ |
| `/category/travel-diaries/` | `/category/travel-diaries/` ✅ |
| `/category/software-engineering/construction/` | `/category/construction/` ❌ |
| `/category/software-engineering/demonstration/` | `/category/demonstration/` ❌ |
| `/category/software-engineering/foundation/` | `/category/foundation/` ❌ |
| `/category/software-engineering/technique/` | `/category/technique/` ❌ |

**Decision (2026-05-08):** accept the structural difference. The 4 nested URLs will 404 post-cutover; SEO impact is bounded (category index pages, not canonical content), and the flat structure is the cleaner long-term shape.

## 4. Tags — accepted as-is ⚠

WP slugifies dots out (`asp.net` → `asp-net`); Hugo's slugifier preserves them.

| Live URL | Hugo URL |
|---|---|
| `/tag/asp-net/` | `/tag/asp.net/` |
| `/tag/asp-net-mvc/` | `/tag/asp.net-mvc/` |
| `/tag/net/` | `/tag/.net/` |

40 other tag URLs match exactly.

**Decision (2026-05-08):** accept. Hugo's slug form is more accurate (the technology *is* called `.NET`, not `net`).

## 5. Pagination — accepted as-is ❌

WP paginated the home feed at `/page/2/` … `/page/18/` (17 URLs). Hugo paginates the section index at `/posts/page/2/` … `/posts/page/9/` (8 URLs). Different prefix and roughly half the page count.

**Decision (2026-05-08):** accept. Nobody bookmarks page 12.

## 6. Date archives — accepted as-is ❌

WP emitted monthly archive pages for every month with at least one post (47 URLs from `/2006/07/` through `/2026/04/`). Hugo emits none.

**Decision (2026-05-08):** accept. Not required. The single `/archives/` page lists everything.

## 7. Author archive — implemented ✅

Live: `/author/pranavnegandhi/` (single URL). Now in Hugo as a paginated section listing all 87 posts, 10 per page, 9 pages total.

**Implementation:**
- `hugo-site/content/author/pranavnegandhi/_index.md` — section page with title "Articles by Pranav Negandhi" and explicit `url: /author/pranavnegandhi/`
- `hugo-site/layouts/author/list.html` — list template mirroring `_default/archives.html` styling (60×60 featured-image thumbnails, post title, formatted date) plus a numeric pager with prev/next links

Resulting URLs: `/author/pranavnegandhi/`, `/author/pranavnegandhi/page/2/` through `/page/9/` (last page has 7 posts; 8×10 + 7 = 87). Hugo also emits `/page/1/` as a meta-refresh alias to the canonical landing page, which is fine.

## 8. Comments feed `/comments/feed/` — accepted as-is ⚠

Comments dropped entirely in Phase 1. Per-comment feed has no readers on a no-comment site.

**Decision (2026-05-08):** accept.

## 9. `robots.txt` — implemented ✅

Live `robots.txt` (1738 bytes) carries:
- a Content-Signal preamble explaining `search` / `ai-input` / `ai-train` semantics
- an EU Directive 2019/790 Article 4 reservation-of-rights notice
- a Cloudflare-Managed-Content block setting `Content-Signal: search=yes,ai-train=no` for `*` and explicit `Disallow: /` for AI crawlers: Amazonbot, Applebot-Extended, Bytespider, CCBot, ClaudeBot, CloudflareBrowserRenderingCrawler, Google-Extended, GPTBot, meta-externalagent

The live file does **not** include a `Sitemap:` line. We add one pointing at `/sitemap.xml` (Hugo emits a real sitemap; advertising it is hygiene).

**Implementation:**
- `hugo-site/layouts/robots.txt` — the live policy verbatim, plus a trailing `Sitemap: {{ "sitemap.xml" | absURL }}` line
- `hugo-site/hugo.toml` — added `ROBOTS` to the home output formats: `home = ["HTML", "ROBOTS"]`. Required because the explicit `[outputs]` block was overriding `enableRobotsTXT = true`. Without this change, Hugo silently skipped robots.txt generation.

Verified: `curl http://127.0.0.1:1313/robots.txt` returns 200 with the expected body and `Sitemap: https://notadesigner.com/sitemap.xml`.

## 10. Special URLs (from `special-urls-audit.md`)

| Check | Status |
|---|---|
| Posts permalink `/:slug/` | ✅ |
| Category permalink `/category/:slug/` | ✅ |
| Tag permalink `/tag/:slug/` | ✅ |
| `/feed/` dual-emission (XML + HTML body) | ✅ |
| `/sitemap.xml` emitted | ✅ |
| `/404.html` emitted | ✅ |
| Pagination paths | ✅ (config) — URL prefix accepted (§5) |
| Trailing slash policy | ✅ |
| `robots.txt` | ✅ — implemented (§9) |

## 11. Aliases — sanity check ✅

Frontmatter declares 97 aliases (87 self-aliases + 10 legacy). Hugo emits 58 alias HTML files; the 39-file shortfall is Hugo skipping aliases that match the canonical URL. The 10 legacy slug redirects (`/100/`, `/29/`, `/531/`, `/libraries-or-bust/`, `/nothing-so-simple-that-it-cannot-be-difficult/`, `/practical-design-patterns-in-c-strategy/`, `/practicing-programmin/`, `/symptoms-of-competence-1/`, `/users-dont-read-error-dialogs/`) are all present.

## 12. Build warnings (carried to Phase 5 QA)

Not URL parity, but surfaced during the parity build: 17 broken cross-post link refs (`REF_NOT_FOUND`) and 1 missing image (`diagram-1.svg` in `/a-model-for-sequential-workflow-execution/`).

Several of the broken refs name posts that *do* exist by URL — likely wikilink-form mismatches in the converter's `[[ref]]` handling. A handful name attachment-page subpaths that WP had but the converter doesn't recreate (e.g. `/tour-of-hampi-belgaum-to-hospet/rustic-flavour/`, `/a-bicycle-like-no-other/navigator-full-view/`). Triage in Phase 5.

---

## Phase 3 close-out

- [x] All mandatory fixes implemented (robots.txt §9, author archive §7)
- [x] All accepted gaps documented (§3, §4, §5, §6, §8)
- [x] Build clean: 147 pages, 35 paginator pages, 148 aliases (post-fix)
- [x] Local server smoke-tested: `/`, `/about/`, `/archives/`, `/feed/`, `/author/pranavnegandhi/`, `/author/pranavnegandhi/page/9/`, `/robots.txt`, sample post — all 200

Phase 3 is closed. Proceed to Phase 4 (GitHub Actions deploy).
