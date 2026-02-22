using Application.Abstractions;
using Domain.Entities;
using MediatR;

namespace Application.CategorySuggestions.Commands
{
    // --- Approve ---
    public class ApproveCategorySuggestionCommand : IRequest, ITransactionalRequest
    {
        public Guid Id { get; set; }
    }

    public class ApproveCategorySuggestionHandler(IRepository<CategorySuggestion> repo)
        : IRequestHandler<ApproveCategorySuggestionCommand>
    {
        public async Task Handle(ApproveCategorySuggestionCommand request, CancellationToken ct)
        {
            var suggestion = await repo.GetByIdAsync(request.Id, ct)
                ?? throw new Exception("Öneri bulunamadı.");

            suggestion.Status = CategorySuggestionStatus.Approved;
            suggestion.Modified();
            repo.Update(suggestion);
        }
    }

    // --- Reject ---
    public class RejectCategorySuggestionCommand : IRequest, ITransactionalRequest
    {
        public Guid Id { get; set; }
        public string? AdminNote { get; set; }
    }

    public class RejectCategorySuggestionHandler(IRepository<CategorySuggestion> repo)
        : IRequestHandler<RejectCategorySuggestionCommand>
    {
        public async Task Handle(RejectCategorySuggestionCommand request, CancellationToken ct)
        {
            var suggestion = await repo.GetByIdAsync(request.Id, ct)
                ?? throw new Exception("Öneri bulunamadı.");

            suggestion.Status = CategorySuggestionStatus.Rejected;
            suggestion.AdminNote = request.AdminNote;
            suggestion.Modified();
            repo.Update(suggestion);
        }
    }
}
