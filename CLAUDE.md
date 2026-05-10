# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a collection of programming articles written in Markdown. The articles explore CS fundamentals (algorithms, data structures, leetcode-style problems) and web development, aimed at senior/staff+ engineers who may have gaps in formal CS education.

The author is a self-taught programmer with 25 years of experience and no formal CS background. The project is both a learning exercise and a teaching resource — solving problems, then writing about the journey.

## Article Specifications

- **Site:** Hugo + Hugo Book theme; deployed to `notadesigner.com` via GitHub Actions on every push to `main`
- **Format:** Hugo page bundle — each post is a directory `hugo-site/content/posts/<slug>/` containing `index.md` + any per-post assets (`featured.jpg`, images, `diagram-N.puml`/`.svg`, code zips, etc.)
- **Slug:** lowercase, hyphenated, no date prefix. URL: `notadesigner.com/<slug>/`. Permalinks are slug-only by deliberate design — they preserve the original WordPress URL structure so old links don't break.
- **Length:** 1500–2500 words
- **Code:** C# snippets, moderate density — illustrative, not exhaustive
- **Visuals:** ASCII/text diagrams only when they genuinely clarify what prose and code cannot. PlantUML for class/sequence diagrams (rendered via `scripts/render-puml.ps1`).
- **Structure:** Most articles anchored to a specific problem, but some are concept explorations

## Writing Voice

**This is the most important section. Follow it precisely.**

- **Tone**: Rich, vivid, highly engaging. Never dry or clinical.
- **Hooks**: Open with historical origin stories or war stories from real engineering work. Grab attention in the first paragraph.
- **POV**: First person (I/my) for personal stories and reflections. Second person (you) when teaching or challenging the reader.
- **Energy**: Curiosity-driven discovery — "wait, how DOES this work?" — not lecturing. The reader figures it out alongside the author.
- **Inspiration**: Julia Evans (jvns.ca) — genuine curiosity, making hard things approachable without dumbing them down.
- **Anti-pattern**: Do not produce textbook prose, Wikipedia-style summaries, or dry technical documentation. Every section should have momentum.

## Workflow

1. Pranav provides rough notes, solutions, or scattered thoughts on a topic
2. Claude shapes the raw material into a polished, publication-ready article
3. Iterate on voice, structure, and technical accuracy together

## Content Principles

- Assume the reader is technically experienced but may be encountering this specific concept for the first time
- Show the thinking process, not just the answer — wrong turns and "aha" moments are valuable
- Ground abstract concepts in real-world consequences (performance, production incidents, system design)
- Code examples should be clean and idiomatic C#, with brief inline comments only where the logic isn't obvious

## Publishing flow

Once a draft is polished:

1. **Scaffold the bundle:** `.\scripts\new-post.ps1 -Slug "two-pointer-technique" -Title "The Two-Pointer Technique"`. Creates `hugo-site/content/posts/two-pointer-technique/index.md` with frontmatter (`draft: true` by default).
2. **Drop assets into the bundle:** `featured.jpg` (used for OG/Twitter cards and the archives thumbnail), inline images referenced from the body, any code zips. All paths in the markdown are relative to the bundle.
3. **Diagrams (optional):** drop `diagram-N.puml` files into the bundle, then run `.\scripts\render-puml.ps1` to produce sibling `.svg` files. Reference them in the markdown as `![](diagram-1.svg)`. The render script uses the public plantuml.com web service (local rendering would need Java 11+; this machine has Java 8). Both `.puml` and `.svg` are committed — the deploy pipeline doesn't re-render.
4. **Local preview:** `.\tools\hugo\hugo.exe server` from `hugo-site/` (or just `hugo server` if you have it on PATH). Visit `http://localhost:1313/<slug>/`. Hugo's hot reload picks up most edits; restart the server after touching shortcodes or template files.
5. **Publish:** flip `draft: true` → remove the line (or set to `false`), `git add` + `git commit` + `git push origin main`. The `.github/workflows/deploy.yml` workflow rebuilds and redeploys to `notadesigner.com`.
6. **Verify the live site:** `.\scripts\qa-live.ps1` — automated smoke test against `https://notadesigner.com`. Three passes: every post URL + auxiliary page returns 200; every alias redirects; a curated sample of posts has every linked asset (images, SVGs, code zips, Ruffle bundle) returning 200; structural spot checks confirm the recent feature work is still wired up. Run it after any push that touches content, infrastructure, or the deploy workflow — it catches issues like a missing CNAME or a disabled Pages site that wouldn't show up in the workflow's own green checkmark.

### Repo layout (recap)

- `hugo-site/content/posts/<slug>/` — published articles, each as a page bundle
- `hugo-site/layouts/` — project-local theme overrides (archives, RSS, custom shortcodes)
- `hugo-site/static/` — site-wide assets (CSS, the self-hosted Ruffle bundle for Flash posts)
- `hugo-site/themes/hugo-book/` — git submodule, do not edit; layer overrides via `hugo-site/layouts/`
- `tools/wxr-to-hugo/` — original WordPress migration converter; **frozen**, do not re-run unless you want to overwrite hand edits
- `scripts/` — operational PowerShell: `new-post.ps1`, `render-puml.ps1`, `qa-live.ps1`, `update-hugo.ps1`, `update-ruffle.ps1`, plus the one-off migration fixers (`fix-figure-captions.ps1`, `scan-thumbnails.ps1`, `upgrade-thumbnails.ps1`) kept for documentation
- `tasks/` — `todo.md` (migration status / open items), `lessons.md` (gotchas worth remembering), `phase3/` (URL parity audit artefacts)

### Setting up a fresh clone

```powershell
git clone --recurse-submodules git@github.com:pranavnegandhi/notadesigner.github.io.git
cd notadesigner.github.io
.\scripts\update-hugo.ps1     # fetches Hugo extended into tools\hugo\
.\scripts\update-ruffle.ps1   # fetches Ruffle into hugo-site\static\ruffle\
.\tools\hugo\hugo.exe server  # from hugo-site/, or supply -s hugo-site
```

If you forgot `--recurse-submodules`, run `git submodule update --init --recursive` after cloning.
