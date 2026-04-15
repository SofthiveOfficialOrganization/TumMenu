namespace Application.Microservices.DTOs
{
	public class ApiResponse<T>
	{
		public bool Success { get; set; }
		public string Message { get; set; } = null!;
		public ErrorModel ErrorModel { get; set; } = null!;
		public T Data { get; set; } = default!;
	}
}
