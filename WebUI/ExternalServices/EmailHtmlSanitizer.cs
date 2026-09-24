using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace WebUI.ExternalServices;

public sealed class EmailHtmlSanitizer
{
    private static readonly HashSet<string> AllowedTags = new(StringComparer.OrdinalIgnoreCase)
    {
        "a", "abbr", "b", "blockquote", "br", "caption", "code", "div", "em", "font", "h1", "h2", "h3",
        "h4", "h5", "h6", "hr", "i", "img", "li", "ol", "p", "pre", "s", "small", "span", "strong",
        "sub", "sup", "table", "tbody", "td", "th", "thead", "tr", "u", "ul"
    };

    private static readonly HashSet<string> AllowedAttributes = new(StringComparer.OrdinalIgnoreCase)
    {
        "alt", "align", "class", "color", "colspan", "height", "href", "id", "rowspan", "src", "target",
        "title", "valign", "width"
    };

    private static readonly Regex DangerousBlocks = new(
        "<!--.*?-->|<\\s*(script|style|iframe|object|embed|form|link|meta|base|svg|math)\\b[^>]*>.*?<\\s*/\\s*\\1\\s*>",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

    private static readonly Regex TagRegex = new(
        "<\\s*(/?)\\s*([a-z][a-z0-9:-]*)([^>]*)>",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex AttributeRegex = new(
        "(?<name>[a-z][a-z0-9:-]*)\\s*=\\s*(?:\\\"(?<value>[^\\\"]*)\\\"|'(?<value>[^']*)'|(?<value>[^\\s>]+))",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public string Sanitize(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return string.Empty;
        }

        var input = DangerousBlocks.Replace(html, string.Empty);
        var output = new StringBuilder(input.Length);
        var cursor = 0;

        foreach (Match match in TagRegex.Matches(input))
        {
            output.Append(WebUtility.HtmlEncode(input[cursor..match.Index]));
            output.Append(SanitizeTag(match));
            cursor = match.Index + match.Length;
        }

        output.Append(WebUtility.HtmlEncode(input[cursor..]));
        return output.ToString();
    }

    private static string SanitizeTag(Match tagMatch)
    {
        var isClosing = tagMatch.Groups[1].Success && tagMatch.Groups[1].Value == "/";
        var tagName = tagMatch.Groups[2].Value.ToLowerInvariant();

        if (!AllowedTags.Contains(tagName))
        {
            return string.Empty;
        }

        if (isClosing)
        {
            return $"</{tagName}>";
        }

        var attributes = new StringBuilder();
        foreach (Match attributeMatch in AttributeRegex.Matches(tagMatch.Groups[3].Value))
        {
            var name = attributeMatch.Groups["name"].Value.ToLowerInvariant();
            if (!AllowedAttributes.Contains(name) || name.StartsWith("on", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var value = WebUtility.HtmlDecode(attributeMatch.Groups["value"].Value).Trim();
            if (name is "href" or "src")
            {
                if (!IsSafeUrl(value))
                {
                    continue;
                }
            }

            attributes.Append(' ')
                .Append(name)
                .Append("=\"")
                .Append(WebUtility.HtmlEncode(value))
                .Append('"');
        }

        if (tagName == "a"
            && attributes.ToString().Contains("target=\"_blank\"", StringComparison.OrdinalIgnoreCase))
        {
            attributes.Append(" rel=\"noopener noreferrer nofollow\"");
        }

        var selfClosing = tagMatch.Value.TrimEnd().EndsWith("/>", StringComparison.Ordinal);
        return selfClosing || tagName is "br" or "hr" or "img"
            ? $"<{tagName}{attributes}/>"
            : $"<{tagName}{attributes}>";
    }

    private static bool IsSafeUrl(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        if (value.StartsWith("//", StringComparison.Ordinal)
            || value.Contains('\u0000')
            || value.Contains("javascript:", StringComparison.OrdinalIgnoreCase)
            || value.Contains("data:", StringComparison.OrdinalIgnoreCase)
            || value.Contains("vbscript:", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!Uri.TryCreate(value, UriKind.RelativeOrAbsolute, out var uri))
        {
            return false;
        }

        return !uri.IsAbsoluteUri
            || uri.Scheme is "http" or "https" or "mailto";
    }
}
