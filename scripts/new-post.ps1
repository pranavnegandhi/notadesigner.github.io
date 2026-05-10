#Requires -Version 5.1
# Scaffolds a new post bundle at hugo-site/content/posts/<slug>/index.md
# with the project's frontmatter conventions.
#
# Usage:
#   .\scripts\new-post.ps1 -Slug "my-new-post" -Title "My New Post"
#   .\scripts\new-post.ps1 -Slug my-post -Title "My Post" -Categories "Software Engineering" -Tags ".net","c#"
#
# Drafts default to draft: true so they don't leak into the live site
# until you flip it. Drop featured.jpg + any .puml files into the bundle
# directory after scaffolding.

[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidatePattern('^[a-z0-9]+(-[a-z0-9]+)*$')]
    [string]$Slug,

    [Parameter(Mandatory)]
    [string]$Title,

    [string[]]$Categories = @(),

    [string[]]$Tags = @(),

    [string]$PostsRoot = 'D:\projects\notadesigner.com\hugo-site\content\posts'
)

$ErrorActionPreference = 'Stop'

$bundle = Join-Path $PostsRoot $Slug
if (Test-Path $bundle) {
    throw "Bundle already exists at $bundle. Pick a different slug or delete the existing directory."
}

New-Item -ItemType Directory -Force $bundle | Out-Null

$date = (Get-Date).ToString('yyyy-MM-ddTHH:mm:sszzz')
$titleEscaped = $Title -replace '"', '\"'

$catLines = if ($Categories.Count -gt 0) {
    "categories:`n" + ($Categories | ForEach-Object { "  - `"$_`"" } | Out-String).TrimEnd()
} else {
    'categories: []'
}
$tagLines = if ($Tags.Count -gt 0) {
    "tags:`n" + ($Tags | ForEach-Object { "  - `"$_`"" } | Out-String).TrimEnd()
} else {
    'tags: []'
}

$frontmatter = @"
---
title: "$titleEscaped"
date: $date
slug: $Slug
draft: true
$catLines
$tagLines
---

"@

$indexPath = Join-Path $bundle 'index.md'
$utf8NoBom = [System.Text.UTF8Encoding]::new($false)
[System.IO.File]::WriteAllText($indexPath, $frontmatter, $utf8NoBom)

Write-Host "Created $indexPath"
Write-Host ""
Write-Host "Next steps:"
Write-Host "  1. Drop featured.jpg into $bundle (optional but recommended for OG/Twitter cards)"
Write-Host "  2. Drop any diagram-N.puml files in, then run scripts\render-puml.ps1"
Write-Host "  3. Write the body in $indexPath"
Write-Host "  4. Flip 'draft: true' to 'false' (or remove the line) when ready to publish"
Write-Host "  5. Commit + push - the deploy workflow takes care of the rest"
