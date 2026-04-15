namespace Application.Microservices.DTOs
{
	public class ErrorModel
	{
		public string Title { get; set; } = null!;
		public string Detail { get; set; } = null!;
		public int Status { get; set; }
	}
}