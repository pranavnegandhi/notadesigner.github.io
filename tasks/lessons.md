# Lessons

Patterns to remember from corrections and friction encountered while working on this repo.

## Hugo: `jsonify` inside `<script>` context double-escapes

**Symptom:** values emitted via `{{ $foo | jsonify }}` inside an inline `<script>` tag come out wrapped in literal `\"…\"` — so `getElementById({{ $id | jsonify }})` becomes `getElementById("\"ruffle-…\"")` and silently fails to match.

**Why:** Hugo's HTML/template engine detects JS context and re-escapes the string `jsonify` already produced. `jsonify` outputs a quoted JSON literal; Hugo then JS-escapes the quotes again.

**Fix:** chain `safeJS` after `jsonify`:
```
document.getElementById({{ $id | jsonify | safeJS }})
player.load({ url: {{ $url | jsonify | safeJS }} })
```

**Where this hit:** `layouts/shortcodes/flash.html`, Phase 4 Ruffle embed work.

## Hugo dev server: stale state after shortcode edits

**Symptom:** edited `layouts/shortcodes/foo.html`, but the served page still reflects the old shortcode output. Static `hugo` build is correct; only the running `hugo server` is wrong.

**Why:** LiveReload's incremental rebuild can miss shortcode changes that affect Page.Scratch state or template-context detection. (Mirrors the existing memory about restarting after `rm -rf content`.)

**Fix:** kill and restart `hugo server` after non-trivial shortcode edits, before debugging the rendered output.

## Don't infer SWF asset paths from desktop Ruffle's resolved-URL log

**Symptom:** desktop Ruffle traces a `FetchError` for `file:///D:/.../wp-content/uploads/2012/media/fish.jpg`. Reading that as the SWF's literal request leads to staging the asset at the full WP path inside the bundle. The web Ruffle then requests something completely different (`/media/fish.jpg`) and the file is in the wrong place.

**Why:** the SWF's actual internal request was `../media/fish.jpg` (relative). The log shows the *resolved* URL, which depends on where the SWF lives. On desktop the SWF was at `…/2012/10/`, so `..` walked up to `…/2012/`. On the web bundle the SWF is at the post root, so `..` walks up out of the bundle entirely.

**Fix:** to debug a SWF's relative loads, either decompile with JPEXS to read the literal `URLRequest` strings, or use Ruffle's `base` config to set the SWF's effective directory so its internal relatives resolve where you've staged the assets. The `flash.html` shortcode supports this via `baseRel="…"`.
