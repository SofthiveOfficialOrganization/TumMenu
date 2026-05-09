param(
    [string] $Path = "Blogs",
    [string] $SingleFile,
    [int] $MinWords = 1500,
    [int] $MaxWords = 2000
)

$ErrorActionPreference = "Stop"

. (Join-Path $PSScriptRoot "BlogMarkdown.Core.ps1")

function Add-Issue {
    param(
        [System.Collections.Generic.List[string]] $Issues,
        [string] $Message
    )

    [void] $Issues.Add($Message)
}

$requiredFrontMatterFields = @(
    "title",
    "slug",
    "summary",
    "tags",
    "publishedAt",
    "isPublished",
    "author",
    "language"
)

$forbiddenPhrases = @(
    "Günümüzde",
    "dijitalleşen dünyada",
    "büyük önem taşır",
    "etkili çözüm sunar",
    "sonuç olarak",
    "işletmenizi bir üst seviyeye taşır",
    "vazgeçilmez hale gelmiştir"
)

[System.IO.FileInfo[]] $files = @()

if (-not [string]::IsNullOrWhiteSpace($SingleFile)) {
    $resolvedSingle = Resolve-Path -LiteralPath $SingleFile -ErrorAction SilentlyContinue
    if (-not $resolvedSingle) {
        Write-Host "Dosya bulunamadi: $SingleFile" -ForegroundColor Red
        Write-Host "Ornek: powershell -ExecutionPolicy Bypass -File Tools\ValidateBlogMarkdown.ps1 -SingleFile Blogs\yazi.md"
        exit 1
    }
    $files = @(Get-Item -LiteralPath $resolvedSingle.Path)
}
else {
    $resolvedPath = Resolve-Path -LiteralPath $Path -ErrorAction SilentlyContinue
    if (-not $resolvedPath) {
        Write-Host "Blog markdown klasoru bulunamadi: $Path" -ForegroundColor Red
        Write-Host "Ornek kullanim: powershell -ExecutionPolicy Bypass -File Tools\ValidateBlogMarkdown.ps1 -Path Blogs"
        exit 1
    }

    $files = @(Get-ChildItem -LiteralPath $resolvedPath -Filter "*.md" -File -Recurse | Sort-Object FullName)
}

if ($files.Count -eq 0) {
    Write-Host "Kontrol edilecek .md dosyasi bulunamadi." -ForegroundColor Yellow
    exit 0
}

$failedFiles = 0

foreach ($file in $files) {
    $content = Get-Content -LiteralPath $file.FullName -Raw -Encoding UTF8
    $parsed = Parse-FrontMatter $content
    $issues = [System.Collections.Generic.List[string]]::new()

    if (-not $parsed.HasFrontMatter) {
        Add-Issue $issues "Front matter yok veya --- bloklari hatali."
    }

    foreach ($field in $requiredFrontMatterFields) {
        $value = Get-FrontMatterValue $parsed.FrontMatter $field
        if ([string]::IsNullOrWhiteSpace($value)) {
            Add-Issue $issues "Front matter alani eksik: $field"
        }
    }

    $slug = Get-FrontMatterValue $parsed.FrontMatter "slug"
    if (-not [string]::IsNullOrWhiteSpace($slug)) {
        if ($slug -notmatch "^[a-z0-9]+(?:-[a-z0-9]+)*$") {
            Add-Issue $issues "Slug kucuk harf, rakam ve tire disinda karakter iceriyor: $slug"
        }

        if ($file.BaseName -ne $slug) {
            Add-Issue $issues "Dosya adi slug ile ayni degil. Dosya: $($file.BaseName), slug: $slug"
        }
    }

    $summary = Get-FrontMatterValue $parsed.FrontMatter "summary"
    if (-not [string]::IsNullOrWhiteSpace($summary)) {
        if ($summary.Length -lt 140 -or $summary.Length -gt 160) {
            Add-Issue $issues "Summary 140-160 karakter araliginda degil. Uzunluk: $($summary.Length)"
        }
    }

    $tags = Get-FrontMatterValue $parsed.FrontMatter "tags"
    if (-not [string]::IsNullOrWhiteSpace($tags)) {
        $tagList = $tags.Split(",", [System.StringSplitOptions]::RemoveEmptyEntries).ForEach({ $_.Trim() })
        if ($tagList.Count -lt 3 -or $tagList.Count -gt 6) {
            Add-Issue $issues "Tags 3-6 etiket icermeli. Adet: $($tagList.Count)"
        }

        foreach ($tag in $tagList) {
            if ($tag -cne $tag.ToLowerInvariant() -or $tag -match "\s") {
                Add-Issue $issues "Etiketler kucuk harfli ve bosluksuz olmali: $tag"
            }
        }
    }

    $publishedAt = Get-FrontMatterValue $parsed.FrontMatter "publishedAt"
    if (-not [string]::IsNullOrWhiteSpace($publishedAt) -and $publishedAt -notmatch "^\d{4}-\d{2}-\d{2}$") {
        Add-Issue $issues "publishedAt YYYY-MM-DD formatinda olmali: $publishedAt"
    }

    $isPublished = Get-FrontMatterValue $parsed.FrontMatter "isPublished"
    if (-not [string]::IsNullOrWhiteSpace($isPublished) -and $isPublished -ne "true") {
        Add-Issue $issues "isPublished true olmali."
    }

    $language = Get-FrontMatterValue $parsed.FrontMatter "language"
    if (-not [string]::IsNullOrWhiteSpace($language) -and $language -ne "tr-TR") {
        Add-Issue $issues "language tr-TR olmali."
    }

    $body = $parsed.Body.Trim()
    $h1Matches = [regex]::Matches($body, "(?m)^#\s+\S")
    if ($h1Matches.Count -ne 1) {
        Add-Issue $issues "Govdede tam 1 adet H1 olmali. Bulunan: $($h1Matches.Count)"
    }

    $wordCount = Get-BlogWordCount $body
    if ($wordCount -lt $MinWords -or $wordCount -gt $MaxWords) {
        Add-Issue $issues "Kelime sayisi $MinWords-$MaxWords araliginda degil. Bulunan: $wordCount"
    }

    if ($body -notmatch "(?m)^\|.+\|\s*$") {
        Add-Issue $issues "Markdown tablo bulunamadi."
    }

    if ($body -notmatch "(?m)^-\s+\[[ xX]\]\s+") {
        Add-Issue $issues "Checkbox kontrol listesi bulunamadi."
    }

    if ($body -notmatch "(?mi)^##\s+.*s[\u0131i]k yap[\u0131i]lan hata") {
        Add-Issue $issues "'Sik yapilan hatalar' H2 bolumu bulunamadi."
    }

    foreach ($phrase in $forbiddenPhrases) {
        if ($body.IndexOf($phrase, [System.StringComparison]::CurrentCultureIgnoreCase) -ge 0) {
            Add-Issue $issues "Yasakli kalip kullanilmis: $phrase"
        }
    }

    if ($issues.Count -gt 0) {
        $failedFiles++
        Write-Host "`nFAIL $($file.FullName)" -ForegroundColor Red
        foreach ($issue in $issues) {
            Write-Host "  - $issue" -ForegroundColor Red
        }
    }
    else {
        Write-Host "OK   $($file.FullName) ($wordCount kelime)" -ForegroundColor Green
    }
}

Write-Host ""
Write-Host "Kontrol edilen dosya: $($files.Count)"
Write-Host "Hatali dosya: $failedFiles"

if ($failedFiles -gt 0) {
    exit 1
}

exit 0
