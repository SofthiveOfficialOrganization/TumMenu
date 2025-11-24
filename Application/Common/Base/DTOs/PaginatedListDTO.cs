namespace Application.Common.Base.DTOs;

public class PaginatedListDTO<T> : PaginationListDTOBase
{
    public IList<T> Items { get; set; } = new List<T>();
}
