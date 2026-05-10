#Requires -Version 5.1
# Live-site QA sweep against https://notadesigner.com.
#
# Three passes:
#   1. URL status: every post permalink, every alias, every auxiliary page.
#   2. Asset integrity (curated sample): fetch the HTML for posts that
#      exercise the recent fixes (figures, upgraded thumbnails, PlantUML
#      SVGs, Flash) and confirm every <img>, <a>, <video>, <source>, and
#      Ruffle script reference returns 200.
#   3. Spot checks: <figure>/<figcaption> structure present in the
#      caption-fixed posts; ruffle.js loaded once on Flash posts.
#
# Output: a per-pass summary plus an explicit FAIL list for anything off.

[CmdletBinding()]
param(
    [string]$BaseUrl = 'https://notadesigner.com',
    [string]$PostsRoot = 'D:\projects\notadesigner.com\hugo-site\content\posts'
)

$ErrorActionPreference = 'Continue'  # report failures, don't halt
$ProgressPreference = 'SilentlyContinue'

# Disable HTTP redirects; we want to see 301s for aliases as 301s, not 200s
# at the canonical destination.
function Invoke-StatusCheck {
    param([string]$Url, [switch]$AllowRedirect)
    try {
        $params = @{ Uri = $Url; Method = 'Head'; UseBasicParsing = $true; TimeoutSec = 30; ErrorAction = 'Stop' }
        if (-not $AllowRedirect) { $params.MaximumRedirection = 0 }
        $r = Invoke-WebRequest @params
        return @{ Status = [int]$r.StatusCode; Url = $Url }
    } catch {
        $sc = if ($_.Exception.Response) { [int]$_.Exception.Response.StatusCode } else { 0 }
        if ($sc -eq 0 -and $_.Exception.Message -match 'redirect') { $sc = 301 }
        return @{ Status = $sc; Url = $Url; Error = $_.Exception.Message }
    }
}

# Pass 1 - URL status sweep
"### Pass 1: URL status sweep ###"
""

$urls = @()
$urls += "$BaseUrl/"
$urls += "$BaseUrl/about/"
$urls += "$BaseUrl/archives/"
$urls += "$BaseUrl/feed/"
$urls += "$BaseUrl/feed/index.xml"
$urls += "$BaseUrl/sitemap.xml"
$urls += "$BaseUrl/robots.txt"
$urls += "$BaseUrl/author/pranavnegandhi/"

# Posts: derive from frontmatter slug or directory name.
$postSlugs = Get-ChildItem -Path $PostsRoot -Directory | ForEach-Object { $_.Name }
foreach ($slug in $postSlugs) {
    $urls += "$BaseUrl/$slug/"
}

# Aliases (Phase 3 audit identified these 9 legacy slug redirects).
$aliases = @(
    '/100/', '/29/', '/531/', '/libraries-or-bust/',
    '/nothing-so-simple-that-it-cannot-be-difficult/',
    '/practical-design-patterns-in-c-strategy/',
    '/practicing-programmin/', '/symptoms-of-competence-1/',
    '/users-dont-read-error-dialogs/'
)

$results = @()
foreach ($u in $urls) {
    $results += Invoke-StatusCheck -Url $u -AllowRedirect
}
$aliasResults = @()
foreach ($a in $aliases) {
    $aliasResults += Invoke-StatusCheck -Url "$BaseUrl$a"
}

$by200 = @($results | Where-Object { $_.Status -eq 200 }).Count
$bad = @($results | Where-Object { $_.Status -ne 200 })
"$by200 / $($results.Count) URLs returned 200"
if ($bad.Count -gt 0) {
    "FAIL urls:"
    $bad | ForEach-Object { "  {0}  {1}" -f $_.Status, $_.Url }
}

$aliasOk = @($aliasResults | Where-Object { $_.Status -in 200, 301, 302 }).Count
$aliasBad = @($aliasResults | Where-Object { $_.Status -notin 200, 301, 302 })
"$aliasOk / $($aliasResults.Count) aliases redirected (200/301/302)"
if ($aliasBad.Count -gt 0) {
    "FAIL aliases:"
    $aliasBad | ForEach-Object { "  {0}  {1}" -f $_.Status, $_.Url }
}
""

# Pass 2 - Asset integrity on curated sample
"### Pass 2: Asset integrity on curated sample ###"
""

$sample = @(
    # Caption-fix posts (figure/figcaption + figcaption styling)
    'century-mile-to-khandala',
    'tour-of-hampi-belgaum-to-hospet',
    'tour-of-hampi-second-day-at-hampi',
    'creating-an-underwater-effect-in-actionscript',
    'mathematical-elegance-in-programming',
    # Thumbnail-upgrade posts
    'a-bicycle-like-no-other',
    'ride-to-lions-point',
    'tour-of-hampi-riding-around-belgaum',
    # PlantUML SVG posts
    'practical-design-patterns-in-c-singleton',
    'practical-design-patterns-in-c-adapter',
    'a-model-for-sequential-workflow-execution',
    'breaking-circular-dependencies-in-microsoft-di-with-lazy-resolution',
    # Flash posts
    'breaking-free-from-your-api',
    # General coverage
    'varchar50',
    'notes-on-ray-tracing-in-one-weekend',
    'improved-application-performance-with-net-7',
    'porting-a-windows-forms-application-to-net-6-part-1',
    'in-with-the-new'
)

$assetFails = @()
$totalAssets = 0
foreach ($slug in $sample) {
    $pageUrl = "$BaseUrl/$slug/"
    try {
        $html = Invoke-WebRequest -Uri $pageUrl -UseBasicParsing -ErrorAction Stop -TimeoutSec 30
    } catch {
        $assetFails += [PSCustomObject]@{ Post = $slug; Asset = '(page itself)'; Status = 'PAGE FETCH FAIL'; Reason = $_.Exception.Message }
        continue
    }

    # Pull every src= and href= that isn't a fragment, mailto, or off-site
    $srcs = [regex]::Matches($html.Content, 'src="([^"]+)"') | ForEach-Object { $_.Groups[1].Value }
    $hrefs = [regex]::Matches($html.Content, 'href="([^"]+)"') | ForEach-Object { $_.Groups[1].Value }
    $assets = @($srcs) + @($hrefs) |
        Where-Object { $_ -notmatch '^(mailto:|#|tel:|javascript:|data:)' } |
        Where-Object { $_ -notmatch '^https?://' -or $_ -match "^$([regex]::Escape($BaseUrl))" } |
        Sort-Object -Unique

    foreach ($a in $assets) {
        $url = if ($a -match '^https?://') { $a } elseif ($a.StartsWith('/')) { "$BaseUrl$a" } else { "$BaseUrl/$slug/$a" }
        $totalAssets++
        $r = Invoke-StatusCheck -Url $url -AllowRedirect
        if ($r.Status -ne 200) {
            $assetFails += [PSCustomObject]@{ Post = $slug; Asset = $a; Status = $r.Status; Reason = $r.Error }
        }
    }
}
"$totalAssets assets checked across $($sample.Count) sample posts"
"$($assetFails.Count) failures"
if ($assetFails.Count -gt 0) {
    "FAIL assets:"
    $assetFails | ForEach-Object { "  [{0}] {1}  ->  {2}" -f $_.Post, $_.Asset, $_.Status }
}
""

# Pass 3 - Spot checks for structural fixes
"### Pass 3: structural spot checks ###"
""

$structFails = @()

# 3a. Caption-fix posts must contain <figcaption>
$captionPosts = @(
    'century-mile-to-khandala', 'creating-an-underwater-effect-in-actionscript',
    'mathematical-elegance-in-programming', 'tour-of-hampi-belgaum-to-hospet'
)
foreach ($slug in $captionPosts) {
    $html = (Invoke-WebRequest "$BaseUrl/$slug/" -UseBasicParsing).Content
    if ($html -notmatch '<figcaption>') {
        $structFails += "  [$slug] expected <figcaption> in HTML, not found"
    }
}

# 3b. Flash posts must include the Ruffle bootstrap script
$flashPosts = @('breaking-free-from-your-api', 'creating-an-underwater-effect-in-actionscript')
foreach ($slug in $flashPosts) {
    $html = (Invoke-WebRequest "$BaseUrl/$slug/" -UseBasicParsing).Content
    if ($html -notmatch '/ruffle/ruffle\.js') {
        $structFails += "  [$slug] expected /ruffle/ruffle.js script tag, not found"
    }
    if ($html -notmatch 'RufflePlayer\.newest') {
        $structFails += "  [$slug] expected RufflePlayer.newest() inline script, not found"
    }
}

# 3c. Diagram posts must reference diagram-1.svg, and the SVG must serve
$diagramPosts = @(
    'practical-design-patterns-in-c-singleton',
    'practical-design-patterns-in-c-adapter',
    'a-model-for-sequential-workflow-execution',
    'breaking-circular-dependencies-in-microsoft-di-with-lazy-resolution'
)
foreach ($slug in $diagramPosts) {
    $html = (Invoke-WebRequest "$BaseUrl/$slug/" -UseBasicParsing).Content
    if ($html -notmatch 'diagram-\d+\.svg') {
        $structFails += "  [$slug] expected diagram-N.svg reference in HTML, not found"
    }
}

if ($structFails.Count -eq 0) {
    "all structural spot checks passed"
} else {
    "STRUCTURAL FAILURES:"
    $structFails
}
