using Application.Abstractions;
using Application.CategorySuggestions.DTOs;
using Application.Common.Base.Page;
using Application.Common.Base.Page.RequestBase;
using Domain.Entities;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.CategorySuggestions.Queries
{
    public class GetCategorySuggestionsQuery : PageRequest, IRequest<IPaginate<CategorySuggestionDTO>>
    {
        public int? StatusFilter { get; set; }
    }

    public class GetCategorySuggestionsHandler(
        IRepository<CategorySuggestion> repo,
        IMapper mapper)
        : IRequestHandler<GetCategorySuggestionsQuery, IPaginate<CategorySuggestionDTO>>
    {
        public async Task<IPaginate<CategorySuggestionDTO>> Handle(GetCategorySuggestionsQuery request, CancellationToken ct)
        {
            var paged = await repo.GetPageListAsync(
                request,
                s => !request.StatusFilter.HasValue || (int)s.Status == request.StatusFilter.Value,
                include: q => q.Include(s => s.Company),
                orderBy: q => q.OrderByDescending(s => s.CreatedAt),
                ct: ct
            );

            return mapper.Map<IPaginate<CategorySuggestionDTO>>(paged);
        }
    }
}
