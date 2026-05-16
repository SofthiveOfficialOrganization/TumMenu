using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Routing;

namespace WebUI.TagHelpers;

[HtmlTargetElement("tummenu-pagination")]
public sealed class TumMenuPaginationTagHelper(IUrlHelperFactory urlHelperFactory) : TagHelper
{
    private static readonly HashSet<string> PageParameterAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        "page",
        "Page",
        "PageIndex",
        "Index"
    };

    [ViewContext]
    [HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; } = null!;

    public int Index { get; set; }
    public int Pages { get; set; }
    public bool HasPrevious { get; set; }
    public bool HasNext { get; set; }
    public string PageParameter { get; set; } = "page";
    public string? Action { get; set; }
    public string? Controller { get; set; }
    public string? Area { get; set; }
    [HtmlAttributeName(DictionaryAttributePrefix = "route-")]
    public IDictionary<string, string?> RouteValues { get; set; } = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
    public bool PreserveQuery { get; set; } = true;
    public bool ShowNumbers { get; set; } = true;
    public int MaxVisiblePages { get; set; } = 7;
    public bool ShowWhenSinglePage { get; set; }
    public bool ShowSummary { get; set; }
    public int Count { get; set; }
    public int ItemsCount { get; set; }
    public string PreviousText { get; set; } = "Önceki";
    public string NextText { get; set; } = "Sonraki";
    public string ContainerClass { get; set; } = "d-flex justify-content-center mt-4";
    public string ListClass { get; set; } = "pagination pagination-sm mb-0";
    public string AriaLabel { get; set; } = "Sayfa navigasyonu";

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = null;

        if (Pages <= 1 && !ShowWhenSinglePage)
        {
            output.SuppressOutput();
            return;
        }

        var currentPage = Math.Clamp(Index, 1, Math.Max(Pages, 1));
        var content = BuildPagination(currentPage);

        if (!ShowSummary && string.IsNullOrWhiteSpace(ContainerClass))
        {
            output.Content.SetHtmlContent(content);
            return;
        }

        var container = new TagBuilder("div");
        if (!string.IsNullOrWhiteSpace(ContainerClass))
            container.AddCssClass(ContainerClass);

        if (ShowSummary)
        {
            var summary = new TagBuilder("div");
            summary.AddCssClass("text-secondary small");
            summary.InnerHtml.Append($"{Count} kayıttan {ItemsCount} tanesi gösteriliyor");
            container.InnerHtml.AppendHtml(summary);
        }

        container.InnerHtml.AppendHtml(content);
        output.Content.SetHtmlContent(container);
    }

    private TagBuilder BuildPagination(int currentPage)
    {
        var nav = new TagBuilder("nav");
        nav.Attributes["aria-label"] = AriaLabel;

        var list = new TagBuilder("ul");
        foreach (var cssClass in ListClass.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            list.AddCssClass(cssClass);

        list.InnerHtml.AppendHtml(BuildPageItem(currentPage - 1, PreviousText, !HasPrevious));

        if (ShowNumbers)
        {
            var lastRenderedPage = 0;
            foreach (var page in GetVisiblePages(currentPage))
            {
                if (lastRenderedPage > 0 && page - lastRenderedPage > 1)
                    list.InnerHtml.AppendHtml(BuildEllipsis());

                list.InnerHtml.AppendHtml(BuildPageItem(page, page.ToString(), disabled: false, active: page == currentPage));
                lastRenderedPage = page;
            }
        }

        list.InnerHtml.AppendHtml(BuildPageItem(currentPage + 1, NextText, !HasNext));
        nav.InnerHtml.AppendHtml(list);
        return nav;
    }

    private IEnumerable<int> GetVisiblePages(int currentPage)
    {
        if (Pages <= MaxVisiblePages)
        {
            for (var page = 1; page <= Pages; page++)
                yield return page;

            yield break;
        }

        var pages = new SortedSet<int> { 1, Pages };
        for (var page = currentPage - 2; page <= currentPage + 2; page++)
        {
            if (page > 1 && page < Pages)
                pages.Add(page);
        }

        foreach (var page in pages)
            yield return page;
    }

    private TagBuilder BuildPageItem(int page, string text, bool disabled, bool active = false)
    {
        var item = new TagBuilder("li");
        item.AddCssClass("page-item");
        if (disabled)
            item.AddCssClass("disabled");
        if (active)
            item.AddCssClass("active");

        if (disabled || active)
        {
            var span = new TagBuilder("span");
            span.AddCssClass("page-link");
            span.InnerHtml.Append(text);
            item.InnerHtml.AppendHtml(span);
            return item;
        }

        var anchor = new TagBuilder("a");
        anchor.AddCssClass("page-link");
        anchor.Attributes["href"] = BuildUrl(page);
        anchor.InnerHtml.Append(text);
        item.InnerHtml.AppendHtml(anchor);
        return item;
    }

    private static TagBuilder BuildEllipsis()
    {
        var item = new TagBuilder("li");
        item.AddCssClass("page-item disabled");

        var span = new TagBuilder("span");
        span.AddCssClass("page-link");
        span.InnerHtml.Append("...");

        item.InnerHtml.AppendHtml(span);
        return item;
    }

    private string BuildUrl(int page)
    {
        var routeValues = new RouteValueDictionary();

        if (PreserveQuery)
        {
            foreach (var (key, value) in ViewContext.HttpContext.Request.Query)
            {
                if (PageParameterAliases.Contains(key))
                    continue;

                if (!string.IsNullOrWhiteSpace(value.ToString()))
                    routeValues[key] = value.ToString();
            }
        }

        foreach (var (key, value) in RouteValues)
        {
            if (!string.IsNullOrWhiteSpace(value))
                routeValues[key] = value;
        }

        routeValues[PageParameter] = page;

        var area = Area ?? ViewContext.RouteData.Values["area"]?.ToString();
        if (!string.IsNullOrWhiteSpace(area))
            routeValues["area"] = area;

        var action = Action ?? ViewContext.RouteData.Values["action"]?.ToString();
        var controller = Controller ?? ViewContext.RouteData.Values["controller"]?.ToString();
        var urlHelper = urlHelperFactory.GetUrlHelper(ViewContext);

        return urlHelper.Action(new UrlActionContext
        {
            Action = action,
            Controller = controller,
            Values = routeValues
        }) ?? "#";
    }
}
