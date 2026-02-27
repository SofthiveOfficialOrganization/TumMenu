namespace Application.Microservices.DTOs
{
	public class ApiResponse<T>
	{
		public bool Success { get; set; }
		public string Message { get; set; }
		public ErrorModel ErrorModel { get; set; }
		public T Data { get; set; }
	}
}
