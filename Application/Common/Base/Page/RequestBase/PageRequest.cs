namespace Application.Common.Base.Page.RequestBase;

public class PageRequest
{
    private int _page = 1;
    public int Page 
    { 
        get => _page; 
        set => _page = value; 
    }

    // Aliases for robust model binding from various frontend sources
    public int PageIndex { get => Page; set => Page = value; }
    public int Index { get => Page; set => Page = value; }
    public int page { get => Page; set => Page = value; }

    public int PageSize { get; set; } = 10;
    public int From { get; set; } = 1;
}
