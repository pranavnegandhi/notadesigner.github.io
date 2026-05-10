#Requires -Version 5.1
# Fetches the latest Ruffle nightly web-selfhosted bundle and extracts it to
# hugo-site/static/ruffle/. Re-running upgrades in place. The downloaded
# version tag is written to hugo-site/static/ruffle/.version for traceability.

[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$target = Join-Path $repoRoot 'hugo-site\static\ruffle'

Write-Host "Fetching latest Ruffle release metadata..."
$releases = Invoke-RestMethod `
    -Uri 'https://api.github.com/repos/ruffle-rs/ruffle/releases?per_page=1' `
    -Headers @{ 'User-Agent' = 'notadesigner-update-ruffle' }
if (-not $releases -or $releases.Count -eq 0) {
    throw 'No Ruffle releases returned from GitHub API.'
}
$release = $releases[0]
$tag = $release.tag_name

$asset = $release.assets | Where-Object { $_.name -like '*web-selfhosted.zip' } | Select-Object -First 1
if (-not $asset) {
    throw "Release $tag has no *web-selfhosted.zip asset."
}

Write-Host "Latest release: $tag"
Write-Host "Asset:          $($asset.name) ($([math]::Round($asset.size/1MB, 2)) MB)"

$tmp = Join-Path ([System.IO.Path]::GetTempPath()) "ruffle-$tag.zip"
Write-Host "Downloading to $tmp..."
Invoke-WebRequest -Uri $asset.browser_download_url -OutFile $tmp -UseBasicParsing

if (Test-Path $target) {
    Write-Host "Removing existing $target..."
    Remove-Item -Recurse -Force $target
}
New-Item -ItemType Directory -Force -Path $target | Out-Null

Write-Host "Extracting to $target..."
Expand-Archive -Path $tmp -DestinationPath $target -Force
Remove-Item $tmp

Set-Content -Path (Join-Path $target '.version') -Value $tag -Encoding utf8

Write-Host ""
Write-Host "Ruffle $tag installed at $target"
Write-Host "Reference from Hugo as: <script src=`"/ruffle/ruffle.js`"></script>"
