namespace Application.MenuDesigns;

/// <summary>
/// Maps admin form field names to live preview <c>data-prev-part</c> identifiers.
/// </summary>
public static class MenuDesignPreviewTargets
{
    public static IReadOnlyDictionary<string, string[]> All { get; } = new Dictionary<string, string[]>(StringComparer.Ordinal)
    {
        ["PrimaryColor"] = ["hero", "headerStat", "categoryArrow", "categoryPlaceholder", "productPlaceholder"],
        ["PrimaryDarkColor"] = ["hero"],
        ["AccentColor"] = ["productBadge"],
        ["BackgroundColor"] = ["pageBackground"],
        ["SurfaceColor"] = ["categoryPanel", "productPanel"],
        ["TextColor"] = ["productTitle"],
        ["MutedColor"] = ["mutedText"],
        ["BorderRadius"] = ["categoryCard", "productCard", "detailButton"],
        ["ButtonStyle"] = ["detailButton"],
        ["BackgroundGradient"] = ["hero"],
        ["BackgroundImageUrl"] = ["pageBackground"],
        ["HeaderBackgroundColor"] = ["hero"],
        ["HeaderTextColor"] = ["headerTitle", "headerStat"],
        ["CardBorderColor"] = ["categoryCard", "productCard"],
        ["CardBorderWidth"] = ["categoryCard", "productCard"],
        ["CardBackgroundColor"] = ["categoryCard", "productCard"],
        ["CardShadow"] = ["categoryCard", "productCard"],
        ["CardImageBorderRadius"] = ["categoryPlaceholder", "productPlaceholder"],
        ["FontFamily"] = ["productTitle", "mutedText"],
        ["BodyFontSize"] = ["productTitle", "mutedText"],
        ["HeadingFontFamily"] = ["sectionTitle", "headerTitle"],
        ["HeadingFontSize"] = ["headerTitle"],
        ["SubheadingFontSize"] = ["sectionTitle"],
        ["HeadingTextColor"] = ["sectionTitle", "headerTitle"],
        ["HeadingBorderWidth"] = ["sectionTitle"],
        ["HeadingBorderColor"] = ["sectionTitle"],
        ["HeadingFontWeight"] = ["sectionTitle", "headerTitle"],
        ["LinkColor"] = ["phoneLink"],
        ["PriceColor"] = ["productPrice"],
        ["ButtonTextColor"] = ["detailButton"],
        ["ButtonBorderColor"] = ["detailButton"],
        ["ButtonBorderRadius"] = ["detailButton"],
        ["SurfaceOpacity"] = ["categoryPanel", "productPanel"],
        ["CardOpacity"] = ["categoryCard", "productCard"],
        ["HeaderSecondaryTextOpacity"] = ["headerTitle"],
        ["HeaderAccentTextOpacity"] = ["headerAccent"],
        ["PageBackgroundOverlayOpacity"] = ["pageBackground"],
        ["MutedTextOpacity"] = ["mutedText"],
        ["PanelOpacity"] = ["businessInfo"],
        ["HeaderChipOpacity"] = ["headerStat"],
        ["CountPillBackgroundColor"] = ["countPill"],
        ["CountPillTextColor"] = ["countPill"],
        ["CountPillBorderColor"] = ["countPill"],
        ["CountPillOpacity"] = ["countPill"],
        ["PhoneLinkColor"] = ["phoneLink"],
        ["SocialIconColor"] = ["socialLink"],
        ["SocialIconBackgroundColor"] = ["socialLink"],
        ["SocialIconBorderColor"] = ["socialLink"],
        ["SocialIconOpacity"] = ["socialLink"]
    };

    public static string[]? Get(string fieldName) =>
        All.TryGetValue(fieldName, out var parts) ? parts : null;
}
