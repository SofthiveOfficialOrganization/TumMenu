# Dot-source only: ortak blog markdown yardimcilari (ValidateBlogMarkdown / BlogDraftMetrics).
$ErrorActionPreference = "Stop"

function Get-FrontMatterValue {
    param(
        [hashtable] $FrontMatter,
        [string] $Key
    )

    if (-not $FrontMatter.ContainsKey($Key)) {
        return $null
    }

    return $FrontMatter[$Key]
}

function Parse-FrontMatter {
    param([string] $Content)

    $result = @{
        FrontMatter = @{}
        Body        = $Content
        HasFrontMatter = $false
    }

    if ($Content -notmatch "(?s)^---\r?\n(.*?)\r?\n---\r?\n?") {
        return $result
    }

    $result.HasFrontMatter = $true
    $frontMatterText = $Matches[1]
    $result.Body = $Content.Substring($Matches[0].Length)

    foreach ($line in ($frontMatterText -split "\r?\n")) {
        if ([string]::IsNullOrWhiteSpace($line)) {
            continue
        }

        if ($line -cmatch "^\s*([A-Za-z][A-Za-z0-9_]*)\s*:\s*(.*)\s*$") {
            $key = $Matches[1]
            $value = $Matches[2].Trim()

            if (($value.StartsWith('"') -and $value.EndsWith('"')) -or ($value.StartsWith("'") -and $value.EndsWith("'"))) {
                $value = $value.Substring(1, $value.Length - 2)
            }

            $result.FrontMatter[$key] = $value
        }
    }

    return $result
}

function Get-PlainText {
    param([string] $Markdown)

    $text = $Markdown
    $text = [regex]::Replace($text, '(?s)```.*?```', " ")
    $text = [regex]::Replace($text, '`[^`]+`', " ")
    $text = [regex]::Replace($text, "!\[[^\]]*\]\([^)]+\)", " ")
    $text = [regex]::Replace($text, "\[[^\]]+\]\([^)]+\)", " ")
    $text = [regex]::Replace($text, "^\s{0,3}#{1,6}\s+", " ", "Multiline")
    $text = [regex]::Replace($text, "^\s{0,3}>\s?", " ", "Multiline")
    $text = [regex]::Replace($text, "^\s*[-*+]\s+(\[[ xX]\]\s*)?", " ", "Multiline")
    $text = [regex]::Replace($text, "\|", " ")
    $text = [regex]::Replace($text, "[*_~>#-]", " ")

    return $text
}

function Get-BlogWordCount {
    param([string] $Markdown)

    $plainText = Get-PlainText $Markdown
    $matches = [regex]::Matches($plainText, "[\p{L}\p{N}]+(?:['’][\p{L}\p{N}]+)?")
    return $matches.Count
}

function Get-SummaryLengthReport {
    param(
        [string] $Summary,
        [int] $MinChars = 140,
        [int] $MaxChars = 160
    )

    if ([string]::IsNullOrWhiteSpace($Summary)) {
        return @{
            Length = 0
            Ok     = $false
            Hint   = "summary bos"
        }
    }

    $len = $Summary.Length
    $ok = ($len -ge $MinChars -and $len -le $MaxChars)
    $hint = if ($ok) { "aralikta" } elseif ($len -lt $MinChars) { "eksik: +$($MinChars - $len)" } else { "fazla: -$($len - $MaxChars)" }

    return @{
        Length = $len
        Ok     = $ok
        Hint   = $hint
    }
}
