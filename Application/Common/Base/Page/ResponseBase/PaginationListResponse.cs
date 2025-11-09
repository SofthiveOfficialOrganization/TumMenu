namespace Application.Common.Base.Page.ResponseBase
{
	public class PaginationListResponse<T> : PaginationListResponseBase
	{
		public IList<T> Items { get; set; } = new List<T>();
	}

}
