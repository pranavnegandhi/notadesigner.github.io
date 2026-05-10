#Requires -Version 5.1
# Renders every .puml file under hugo-site/content/ into a sibling .svg.
#
# Uses the public PlantUML web service at plantuml.com because local
# rendering needs Java 11+ and this machine has Java 8. The wire format
# is `https://www.plantuml.com/plantuml/svg/~h<hex>` where <hex> is the
# UTF-8 bytes of the source as lowercase hex. No compression or custom
# base64 encoding required - PlantUML's `~h` prefix means "hex".
#
# Idempotent: skips files where the .svg is newer than the .puml unless
# -Force is passed. Run after editing diagrams; commit both .puml and
# .svg so the deploy pipeline (which doesn't run Java) has the SVG.

[CmdletBinding()]
param(
    [string]$Root = 'D:\projects\notadesigner.com\hugo-site\content',
    [switch]$Force
)

$ErrorActionPreference = 'Stop'

function Convert-PumlToHex {
    param([string]$Text)
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($Text)
    return -join ($bytes | ForEach-Object { $_.ToString('x2') })
}

$pumls = Get-ChildItem -Path $Root -Recurse -Filter '*.puml' -File
if ($pumls.Count -eq 0) {
    Write-Host "No .puml files found under $Root."
    return
}

$rendered = 0
$skipped = 0

foreach ($puml in $pumls) {
    $svgPath = [System.IO.Path]::ChangeExtension($puml.FullName, '.svg')
    $relPath = $puml.FullName.Substring($Root.Length).TrimStart('\', '/')

    if (-not $Force -and (Test-Path $svgPath)) {
        $svgItem = Get-Item $svgPath
        if ($svgItem.LastWriteTimeUtc -ge $puml.LastWriteTimeUtc) {
            Write-Host "skip   $relPath  (.svg up to date)"
            $skipped++
            continue
        }
    }

    $text = [System.IO.File]::ReadAllText($puml.FullName, [System.Text.Encoding]::UTF8)
    $hex = Convert-PumlToHex -Text $text
    $url = "https://www.plantuml.com/plantuml/svg/~h$hex"

    if ($url.Length -gt 8000) {
        Write-Warning "skip   $relPath  (encoded URL is $($url.Length) chars; too long for plantuml.com)"
        continue
    }

    try {
        Invoke-WebRequest -Uri $url -OutFile $svgPath -UseBasicParsing -ErrorAction Stop
    } catch {
        Write-Warning "FAIL   $relPath  ($($_.Exception.Message))"
        continue
    }

    $size = (Get-Item $svgPath).Length
    Write-Host ("render {0,-70}  {1,7} bytes" -f $relPath, $size)
    $rendered++
}

Write-Host ""
Write-Host "Rendered $rendered, skipped $skipped (out of $($pumls.Count) .puml files)."
