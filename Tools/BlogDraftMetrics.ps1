# Tek blog dosyasi icin hizli metrik: summary uzunlugu (validator ile ayni .Length),
# kelime sayisi, slug-dosya uyumu. Taslak yazarken tekrar tekrar tam klasor taramasi yapmaya gerek kalmaz.
#
# Ornek:
#   powershell -ExecutionPolicy Bypass -File Tools\BlogDraftMetrics.ps1 -Path Blogs\yazi-slug.md
#   powershell -ExecutionPolicy Bypass -File Tools\BlogDraftMetrics.ps1 -Summary "Ozet metni..."

param(
    [string] $Path,
    [string] $Summary,
    [int] $MinWords = 1500,
    [int] $MaxWords = 2000,
    [int] $SummaryMin = 140,
    [int] $SummaryMax = 160
)

$ErrorActionPreference = "Stop"
. (Join-Path $PSScriptRoot "BlogMarkdown.Core.ps1")

function Write-MetricLine {
    param(
        [string] $Label,
        [string] $Value,
        [bool] $Ok,
        [string] $Extra = ""
    )

    $color = if ($Ok) { "Green" } else { "Yellow" }
    $mark = if ($Ok) { "OK" } else { "!" }
    Write-Host "[$mark] $Label : $Value" -ForegroundColor $color -NoNewline
    if (-not [string]::IsNullOrWhiteSpace($Extra)) {
        Write-Host "  ($Extra)" -ForegroundColor DarkGray
    }
    else {
        Write-Host ""
    }
}

if (-not [string]::IsNullOrWhiteSpace($Summary)) {
    $rep = Get-SummaryLengthReport -Summary $Summary -MinChars $SummaryMin -MaxChars $SummaryMax
    Write-MetricLine -Label "summary (yalnizca metin)" -Value "$($rep.Length) karakter" -Ok $rep.Ok -Extra $rep.Hint
    exit 0
}

if ([string]::IsNullOrWhiteSpace($Path)) {
    Write-Host "Path veya Summary gerekli." -ForegroundColor Red
    Write-Host "  -Path Blogs\slug.md   veya   -Summary '...'" -ForegroundColor DarkGray
    exit 1
}

$resolved = Resolve-Path -LiteralPath $Path -ErrorAction SilentlyContinue
if (-not $resolved) {
    Write-Host "Dosya bulunamadi: $Path" -ForegroundColor Red
    exit 1
}

$file = Get-Item -LiteralPath $resolved.Path
$content = Get-Content -LiteralPath $file.FullName -Raw -Encoding UTF8
$parsed = Parse-FrontMatter $content

if (-not $parsed.HasFrontMatter) {
    Write-Host "Front matter yok veya --- bloklari hatali." -ForegroundColor Red
    exit 1
}

$slug = Get-FrontMatterValue $parsed.FrontMatter "slug"
$summaryText = Get-FrontMatterValue $parsed.FrontMatter "summary"
$rep = Get-SummaryLengthReport -Summary $summaryText -MinChars $SummaryMin -MaxChars $SummaryMax

Write-Host "Dosya: $($file.FullName)" -ForegroundColor Cyan
Write-MetricLine -Label "summary" -Value "$($rep.Length) karakter (hedef $SummaryMin-$SummaryMax)" -Ok $rep.Ok -Extra $rep.Hint

$slugOk = (-not [string]::IsNullOrWhiteSpace($slug)) -and ($file.BaseName -eq $slug)
Write-MetricLine -Label "slug == dosya adi" -Value "dosya=$($file.BaseName), slug=$slug" -Ok $slugOk -Extra $(if (-not $slugOk) { "dosya adini slug ile esitle" } else { "" })

$body = $parsed.Body.Trim()
$wc = Get-BlogWordCount $body
$wcOk = ($wc -ge $MinWords -and $wc -le $MaxWords)
Write-MetricLine -Label "govde kelime" -Value "$wc (hedef $MinWords-$MaxWords)" -Ok $wcOk -Extra $(if (-not $wcOk) { "taslagida genislet veya kisalt" } else { "" })

$h1 = [regex]::Matches($body, "(?m)^#\s+\S").Count
$h1Ok = ($h1 -eq 1)
Write-MetricLine -Label "H1 sayisi" -Value $h1 -Ok $h1Ok -Extra $(if (-not $h1Ok) { "tam 1 H1 olmali" } else { "" })

$tabloOk = $body -match "(?m)^\|.+\|\s*$"
Write-MetricLine -Label "markdown tablo" -Value $(if ($tabloOk) { "var" } else { "yok" }) -Ok $tabloOk

$cbOk = $body -match "(?m)^-\s+\[[ xX]\]\s+"
Write-MetricLine -Label "checkbox liste" -Value $(if ($cbOk) { "var" } else { "yok" }) -Ok $cbOk

$sikOk = $body -match "(?mi)^##\s+.*s[\u0131i]k yap[\u0131i]lan hata"
Write-MetricLine -Label "Sik yapilan hatalar H2" -Value $(if ($sikOk) { "var" } else { "yok" }) -Ok $sikOk

exit 0
