#Requires -Version 5.1
# Scans every post bundle for image references that use WordPress-style
# size-suffix thumbnails (e.g. foo-175x175.jpg) and reports cases where
# a larger or full-size variant of the same image exists in the WP
# archive (D:\projects\notadesigner.github.io\wp-content\uploads\).
# Output is grouped per post so you can decide swaps in batches.

[CmdletBinding()]
param(
    [string]$PostsRoot = 'D:\projects\notadesigner.com\hugo-site\content\posts',
    [string]$WpRoot    = 'D:\projects\notadesigner.github.io\wp-content\uploads'
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

function Get-ImageDims($path) {
    try {
        $img = [System.Drawing.Image]::FromFile($path)
        $r = [PSCustomObject]@{ Width = $img.Width; Height = $img.Height }
        $img.Dispose()
        return $r
    } catch { return $null }
}

# Build an index of every full-size (no -WxH suffix) image in the WP archive,
# keyed by basename.ext (lowercased) so lookups are O(1).
Write-Host "Indexing WP archive..."
$wpIndex = @{}
$thumbRe = '-\d+x\d+(?=\.[A-Za-z]+$)'
Get-ChildItem -Path $WpRoot -Recurse -File -Include *.jpg, *.jpeg, *.png, *.gif | ForEach-Object {
    if ($_.Name -notmatch $thumbRe) {
        $key = $_.Name.ToLower()
        if (-not $wpIndex.ContainsKey($key)) {
            $wpIndex[$key] = $_.FullName
        }
    }
}
Write-Host "Indexed $($wpIndex.Count) full-size images."
Write-Host ""

$posts = Get-ChildItem -Path $PostsRoot -Filter index.md -Recurse
$totalUpgradable = 0
$postsWithUpgrades = 0

foreach ($post in $posts) {
    $body = Get-Content -Raw -Path $post.FullName
    # Match image references: markdown ![alt](src) AND raw <img src="...">
    $srcs = @()
    $srcs += [regex]::Matches($body, '!\[[^\]]*\]\(([^)]+)\)') | ForEach-Object { $_.Groups[1].Value }
    $srcs += [regex]::Matches($body, '<img[^>]+src="([^"]+)"') | ForEach-Object { $_.Groups[1].Value }
    # Filter to bundle-relative thumbnail names (no slashes, has -WxH suffix)
    $thumbs = $srcs | Where-Object { $_ -notmatch '/' -and $_ -match '-(\d+)x(\d+)\.[A-Za-z]+$' } | Sort-Object -Unique
    if (-not $thumbs) { continue }

    $postUpgrades = @()
    foreach ($thumb in $thumbs) {
        if ($thumb -match '^(.+?)-(\d+)x(\d+)(\.[A-Za-z]+)$') {
            $base = $Matches[1]
            $tw = [int]$Matches[2]
            $th = [int]$Matches[3]
            $ext = $Matches[4]
            $fullName = "$base$ext"
            $wpKey = $fullName.ToLower()

            if ($wpIndex.ContainsKey($wpKey)) {
                $wpPath = $wpIndex[$wpKey]
                $dims = Get-ImageDims $wpPath
                $sizeKB = [math]::Round((Get-Item $wpPath).Length / 1KB, 1)
                if ($dims -and ($dims.Width -gt $tw -or $dims.Height -gt $th)) {
                    $postUpgrades += [PSCustomObject]@{
                        Thumb   = $thumb
                        ThumbWH = "$($tw)x$($th)"
                        Full    = $fullName
                        FullWH  = "$($dims.Width)x$($dims.Height)"
                        SizeKB  = $sizeKB
                    }
                }
            }
        }
    }

    if ($postUpgrades.Count -gt 0) {
        $postsWithUpgrades++
        $totalUpgradable += $postUpgrades.Count
        ""
        "=== $($post.Directory.Name) ($($postUpgrades.Count)) ==="
        $postUpgrades | Format-Table -AutoSize | Out-String -Stream | Where-Object { $_ -ne '' }
    }
}

""
"---"
"Total upgradable thumbnails: $totalUpgradable across $postsWithUpgrades posts"
