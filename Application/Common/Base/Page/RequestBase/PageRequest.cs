namespace Application.Common.Base.Page.RequestBase
{
	public class PageRequest
	{
		public int Page { get; set; } = 0;
		public int PageSize { get; set; } = 10;
		public int From { get; set; } = 0;
	}
}
