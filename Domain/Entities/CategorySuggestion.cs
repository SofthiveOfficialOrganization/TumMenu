using Domain.Base;

namespace Domain.Entities
{
    public enum CategorySuggestionStatus
    {
        Pending = 0,
        Viewed = 1,
        Approved = 2,
        Rejected = 3
    }

    public class CategorySuggestion : BaseEntity
    {
        public string Title { get; set; } = null!;

        public string? Reason { get; set; }

        public Guid? CompanyId { get; set; }
        public Company? Company { get; set; }

        public string? SubmittedByName { get; set; }

        public CategorySuggestionStatus Status { get; set; } = CategorySuggestionStatus.Pending;

        public string? AdminNote { get; set; }
    }
}
