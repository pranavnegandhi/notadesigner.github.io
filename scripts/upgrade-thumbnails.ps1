#Requires -Version 5.1
# Replaces 175x175 / 450x600 / etc. thumbnail references in post markdown
# with their full-size counterparts pulled from the WP archive.
#
# For each (post, thumbnail) pair listed below:
#   1. Copy the full-size image from $WpRoot into the post bundle
#   2. Replace every reference to <thumbnail>.<ext> with <full>.<ext> in the
#      post's index.md (handles both markdown ![]() and raw <img src="">)
#   3. Delete the orphaned thumbnail file from the bundle

[CmdletBinding()]
param(
    [string]$PostsRoot = 'D:\projects\notadesigner.com\hugo-site\content\posts',
    [string]$WpRoot    = 'D:\projects\notadesigner.github.io\wp-content\uploads'
)

$ErrorActionPreference = 'Stop'

# Format: PostSlug, Thumbnail, FullSize
$upgrades = @(
    @('a-bicycle-like-no-other',              'navigator-brakes-175x175.jpg',         'navigator-brakes.jpg'),
    @('a-bicycle-like-no-other',              'navigator-computer-175x175.jpg',       'navigator-computer.jpg'),
    @('a-bicycle-like-no-other',              'navigator-front-shifter-175x175.jpg',  'navigator-front-shifter.jpg'),
    @('a-bicycle-like-no-other',              'navigator-rear-shifter-175x175.jpg',   'navigator-rear-shifter.jpg'),
    @('a-bicycle-like-no-other',              'navigator-full-view-800x532.jpg',      'navigator-full-view.jpg'),
    @('ride-to-lions-point',                  'lions-point-climb-175x175.jpg',        'lions-point-climb.jpg'),
    @('ride-to-lions-point',                  'lions-point-view-422x600.jpg',         'lions-point-view.jpg'),
    @('tour-of-hampi-belgaum-to-hospet',      'kasivisvanath-temple-doorway-175x175.jpg',   'kasivisvanath-temple-doorway.jpg'),
    @('tour-of-hampi-belgaum-to-hospet',      'kasivisvanath-temple-exteriors-175x175.jpg', 'kasivisvanath-temple-exteriors.jpg'),
    @('tour-of-hampi-belgaum-to-hospet',      'brahma-jinalaya-shikhara-450x600.jpg', 'brahma-jinalaya-shikhara.jpg'),
    @('tour-of-hampi-pune-to-belgaum',        'contemplative-175x175.jpg',            'contemplative.jpg'),
    @('tour-of-hampi-pune-to-belgaum',        'smiles-afire-450x600.jpg',             'smiles-afire.jpg'),
    @('tour-of-hampi-riding-around-belgaum',  'its-showtime-175x175.jpg',             'its-showtime.jpg'),
    @('tour-of-hampi-riding-around-belgaum',  'morning-glory-175x175.jpg',            'morning-glory.jpg'),
    @('tour-of-hampi-riding-around-belgaum',  'stealing-my-thunder-450x600.jpg',      'stealing-my-thunder.jpg'),
    @('tour-of-hampi-first-day-at-hampi',     'purandara-dasa-mandapa-450x600.jpg',   'purandara-dasa-mandapa.jpg'),
    @('tour-of-hampi-second-day-at-hampi',    'hazara-rama-temple-175x175.jpg',       'hazara-rama-temple.jpg'),
    @('tour-of-hampi-second-day-at-hampi',    'riding-through-hampi-175x175.jpg',     'riding-through-hampi.jpg'),
    @('tour-of-hampi-second-day-at-hampi',    'krishna-temple-dasha-avatar-450x600.jpg', 'krishna-temple-dasha-avatar.jpg'),
    @('tour-of-hampi-second-day-at-hampi',    'pillar-of-exquisite-beauty-450x600.jpg',  'pillar-of-exquisite-beauty.jpg')
)

# Build a quick lookup of WP archive paths.
Write-Host "Indexing WP archive..."
$wpIndex = @{}
Get-ChildItem -Path $WpRoot -Recurse -File -Include *.jpg, *.jpeg, *.png, *.gif | ForEach-Object {
    $key = $_.Name.ToLower()
    if (-not $wpIndex.ContainsKey($key)) {
        $wpIndex[$key] = $_.FullName
    }
}

$utf8NoBom = [System.Text.UTF8Encoding]::new($false)
$applied = 0

foreach ($u in $upgrades) {
    $slug = $u[0]; $thumb = $u[1]; $full = $u[2]
    $bundle = Join-Path $PostsRoot $slug
    $indexMd = Join-Path $bundle 'index.md'
    $thumbPath = Join-Path $bundle $thumb
    $fullDest = Join-Path $bundle $full

    if (-not (Test-Path $indexMd)) {
        Write-Warning "$slug : index.md not found, skipping"
        continue
    }

    $wpKey = $full.ToLower()
    if (-not $wpIndex.ContainsKey($wpKey)) {
        Write-Warning "$slug : full-size $full not found in WP archive, skipping"
        continue
    }

    Copy-Item -Path $wpIndex[$wpKey] -Destination $fullDest -Force

    # Replace thumb references with full in index.md
    $body = [System.IO.File]::ReadAllText($indexMd)
    $newBody = $body.Replace($thumb, $full)
    if ($newBody -eq $body) {
        Write-Warning "$slug : reference to $thumb not found in index.md (file copied anyway)"
    } else {
        [System.IO.File]::WriteAllText($indexMd, $newBody, $utf8NoBom)
    }

    if (Test-Path $thumbPath) {
        Remove-Item -Path $thumbPath -Force
    }

    '{0,-40}  {1}  ->  {2}' -f $slug, $thumb, $full
    $applied++
}

''
"Upgraded $applied thumbnails."
