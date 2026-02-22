using Application.Abstractions;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Application.CategorySuggestions.Commands
{
    public class SubmitCategorySuggestionCommand : IRequest, ITransactionalRequest
    {
        public string Title { get; set; } = null!;
        public string? Reason { get; set; }
    }

    public class SubmitCategorySuggestionHandler(
        IRepository<CategorySuggestion> repo,
        IRepository<Owner> ownerRepo,
        IUserContext userContext)
        : IRequestHandler<SubmitCategorySuggestionCommand>
    {
        public async Task Handle(SubmitCategorySuggestionCommand request, CancellationToken ct)
        {
            var userId = userContext.UserId;

            // Kullanıcının şirketini bul
            Guid? companyId = null;
            string? companyName = null;
            if (!string.IsNullOrEmpty(userId))
            {
                var owner = await ownerRepo.Query()
                    .Include(o => o.Company)
                    .FirstOrDefaultAsync(o => o.ApplicationUserId == userId, ct);
                companyId = owner?.Company?.Id;
                companyName = owner?.Company?.Title;
            }

            var suggestion = new CategorySuggestion
            {
                Title = request.Title.Trim(),
                Reason = string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason.Trim(),
                CompanyId = companyId,
                SubmittedByName = companyName,
                Status = CategorySuggestionStatus.Pending
            };
            suggestion.Created(userId);

            await repo.AddAsync(suggestion, ct);
        }
    }
}
