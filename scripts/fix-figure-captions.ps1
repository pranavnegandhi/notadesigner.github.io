#Requires -Version 5.1
# One-shot fix: wraps `image\ncaption` patterns into <figure>/<figcaption>
# HTML across all hugo-site/content/posts/*/index.md.
#
# Pattern: a line `![alt](src)` immediately followed by a non-empty line that
# isn't another image/heading/list/blockquote/code-fence/table/shortcode.
# That next line is treated as the caption.
#
# Captions are HTML-escaped (& < >) and `alt` text gets " escaped. No markdown
# parsing is done inside the figcaption (the scan confirmed zero captions
# contain markdown formatting).

[CmdletBinding()]
param(
    [string]$Root = 'D:\projects\notadesigner.com\hugo-site\content\posts'
)

$ErrorActionPreference = 'Stop'
$utf8NoBom = [System.Text.UTF8Encoding]::new($false)

$posts = Get-ChildItem -Path $Root -Filter index.md -Recurse
$totalChanges = 0
$postsChanged = 0

foreach ($p in $posts) {
    $lines = [System.IO.File]::ReadAllLines($p.FullName)
    $out = New-Object System.Collections.Generic.List[string]
    $i = 0
    $changed = 0

    while ($i -lt $lines.Count) {
        $line = $lines[$i]
        if ($i -lt $lines.Count - 1 -and $line -match '^!\[([^\]]*)\]\(([^)]+)\)\s*$') {
            $alt = $Matches[1]
            $src = $Matches[2]
            $next = $lines[$i + 1]
            $isCandidate = (
                $next -ne '' -and
                $next -notmatch '^!\[' -and
                $next -notmatch '^[#>\-*]' -and
                $next -notmatch '^\s*$' -and
                $next -notmatch '^\{\{' -and
                $next -notmatch '^```' -and
                $next -notmatch '^\|'
            )
            if ($isCandidate) {
                $caption = $next.Trim()
                $altEscaped = ($alt -replace '"', '&quot;')
                $captionEscaped = $caption.Replace('&', '&amp;').Replace('<', '&lt;').Replace('>', '&gt;')
                $out.Add('<figure>')
                $out.Add('  <img src="' + $src + '" alt="' + $altEscaped + '">')
                $out.Add('  <figcaption>' + $captionEscaped + '</figcaption>')
                $out.Add('</figure>')
                $i += 2
                $changed++
                continue
            }
        }
        $out.Add($line)
        $i++
    }

    if ($changed -gt 0) {
        [System.IO.File]::WriteAllLines($p.FullName, $out, $utf8NoBom)
        $totalChanges += $changed
        $postsChanged++
        '{0,3} fixes  {1}' -f $changed, $p.Directory.Name
    }
}

''
"Total: $totalChanges captions wrapped in <figure>/<figcaption> across $postsChanged posts"
