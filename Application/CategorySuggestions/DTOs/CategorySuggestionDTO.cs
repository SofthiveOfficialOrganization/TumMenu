namespace Application.CategorySuggestions.DTOs
{
    public class CategorySuggestionDTO
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Reason { get; set; }
        public string? CompanyName { get; set; }
        public string? SubmittedByName { get; set; }
        public int Status { get; set; }
        public string? AdminNote { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
