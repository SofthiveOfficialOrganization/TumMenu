namespace Application.Common.Base.DTOs;

public class PaginatedListDTOBase
{
    public int From { get; set; }
    public int Index { get; set; }
    public int Size { get; set; }
    public int Count { get; set; }
    public int Pages { get; set; }
    public bool HasPrevious { get; set; }
    public bool HasNext { get; set; }
    public IDictionary<string, string> FilterNames { get; set; } = new Dictionary<string, string>();
}
