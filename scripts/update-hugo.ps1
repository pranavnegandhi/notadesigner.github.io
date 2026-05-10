#Requires -Version 5.1
# Downloads the pinned Hugo extended Windows binary into tools/hugo/.
# Version is read from tools/hugo/.version (single line, e.g. "0.161.1").
# Bumping Hugo: edit .version, re-run this script, commit the .version change.

[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$target = Join-Path $repoRoot 'tools\hugo'
$versionFile = Join-Path $target '.version'

if (-not (Test-Path $versionFile)) {
    throw "Missing $versionFile. Create it with the desired Hugo version (e.g. ""0.161.1"") and re-run."
}

$version = (Get-Content $versionFile -Raw).Trim()
if ($version -notmatch '^\d+\.\d+\.\d+') {
    throw "Bad version string in $versionFile : '$version'"
}

$tag = "v$version"
$assetName = "hugo_extended_${version}_windows-amd64.zip"
$url = "https://github.com/gohugoio/hugo/releases/download/$tag/$assetName"

Write-Host "Pinned Hugo version: $version"
Write-Host "Downloading $url ..."

$tmp = Join-Path ([System.IO.Path]::GetTempPath()) "hugo-$version.zip"
Invoke-WebRequest -Uri $url -OutFile $tmp -UseBasicParsing

# Wipe the existing binary (and supporting files) but preserve .version.
Get-ChildItem -Path $target -Force -Exclude '.version' | Remove-Item -Recurse -Force

Write-Host "Extracting to $target..."
Expand-Archive -Path $tmp -DestinationPath $target -Force
Remove-Item $tmp

if (-not (Test-Path (Join-Path $target 'hugo.exe'))) {
    throw "Extraction completed but hugo.exe is missing. Check the asset name for version $version."
}

Write-Host ""
Write-Host "Hugo $version installed at $target\hugo.exe"
& (Join-Path $target 'hugo.exe') version
