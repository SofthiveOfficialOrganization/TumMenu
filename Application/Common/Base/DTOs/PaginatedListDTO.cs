namespace Application.Common.Base.DTOs;

public class PaginatedListDTO<T> : PaginatedListDTOBase
{
    public IList<T> Items { get; set; } = new List<T>();
}
