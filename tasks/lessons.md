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

## Git push: Windows OpenSSH agent vs Git for Windows MSYS ssh

**Symptom:** `ssh -T git@github.com` greets you successfully, but `git push` to the same host fails with `Permission denied (publickey)`. Adding the key to ssh-agent (the Windows service) doesn't help.

**Why:** Git for Windows ships its own MSYS2 OpenSSH at `C:\Program Files\Git\usr\bin\ssh.exe` and uses it by default. That binary connects to a different ssh-agent (MSYS2's, started via `eval $(ssh-agent)`) — *not* the Windows OpenSSH service that `ssh-agent` system-service manages. So the key loaded into Windows ssh-agent is invisible to git's MSYS ssh.

**Fixes (pick one):**

- One-shot (this PowerShell session only):
  ```powershell
  $env:GIT_SSH = "C:\Windows\System32\OpenSSH\ssh.exe"
  git push …
  ```
- Persistent for the user (set once in System → Environment Variables, or `[Environment]::SetEnvironmentVariable("GIT_SSH", "C:\Windows\System32\OpenSSH\ssh.exe", "User")`).
- Or: load the key into MSYS2's ssh-agent instead, in a Git Bash session: `eval $(ssh-agent); ssh-add ~/.ssh/id_ed25519`. (Different agent per shell session — annoying.)

The `GIT_SSH` route makes git use the same Windows OpenSSH binary that `ssh -T` uses, so it sees the same agent and the same loaded key.

## Don't sweep loose root-level files into a bootstrap commit

**Symptom:** during `git init`, `git add .` happily stages every file in the project root — including pre-existing scratch (drafts not yet placed under `content/`, saved HTML pages used for one-off diff comparisons, half-finished notes). They get baked into the initial commit and have to be cleaned up separately afterward.

**Why:** the user's "track source only" answer to the gitignore-scope question describes a *concept*, not a per-file approval. Scratch and source can look identical at the filesystem level — a `2026-04-12-foo.md` could be a draft destined for `content/posts/` or could be working notes that should never ship. Same for `*.html` files that could be templates, components, or just saved-page reference dumps.

**Fix:** before the bootstrap `git add`, do a quick `git status --short | grep -v hugo-site/` (or equivalent) to surface anything at the repo root or in unexpected directories. Read the first 2-3 lines of unfamiliar files. Confirm with the user before adding them. Cheap to ask, expensive to retroactively rewrite history if the cleanup becomes important (e.g. accidentally committing a draft you later want never to have shipped).
