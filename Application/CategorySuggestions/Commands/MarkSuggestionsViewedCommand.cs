using Application.Abstractions;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.CategorySuggestions.Commands
{
    /// <summary>
    /// Admin öneri listesini ziyaret ettiğinde tüm Pending önerileri Viewed yapar.
    /// </summary>
    public class MarkSuggestionsViewedCommand : IRequest, ITransactionalRequest { }

    public class MarkSuggestionsViewedCommandHandler(IRepository<CategorySuggestion> repo)
        : IRequestHandler<MarkSuggestionsViewedCommand>
    {
        public async Task Handle(MarkSuggestionsViewedCommand request, CancellationToken ct)
        {
            var pending = await repo.Query(tracked: true)
                .Where(s => s.Status == CategorySuggestionStatus.Pending)
                .ToListAsync(ct);

            foreach (var s in pending)
            {
                s.Status = CategorySuggestionStatus.Viewed;
                s.Modified();
                repo.Update(s);
            }
        }
    }
}
