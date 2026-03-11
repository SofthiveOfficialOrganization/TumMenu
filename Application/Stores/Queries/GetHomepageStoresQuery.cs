using Application.Abstractions;
using Application.Stores.DTOs;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Stores.Queries;

public sealed record GetHomepageStoresQuery : IRequest<HomepageStoresDTO>;

public sealed class HomepageStoresDTO
{
	public List<StoreSearchResultDTO> NewStores { get; set; } = [];
	public List<StoreSearchResultDTO> PopularStores { get; set; } = [];
	public List<StoreSearchResultDTO> DailyStores { get; set; } = [];
}

public class GetHomepageStoresQueryHandler(
	IRepository<Store> repoStore
) : IRequestHandler<GetHomepageStoresQuery, HomepageStoresDTO>
{
	public async Task<HomepageStoresDTO> Handle(GetHomepageStoresQuery req, CancellationToken ct)
	{
		var baseQuery = repoStore.Query()
			.Include(s => s.Address)
			.Include(s => s.Medias)
			.Include(s => s.Company)
			.Where(s => !s.IsDeleted);

		// Yeni restoranlar – en son eklenenler
		var newStores = await baseQuery
			.OrderByDescending(s => s.CreatedAt)
			.Take(6)
			.ToListAsync(ct);

		// Popüler restoranlar – en çok menüsü olan (basit popülerlik ölçütü)
		var popularStores = await baseQuery
			.OrderByDescending(s => s.Menus.Count(m => m.Status == MenuStatus.Active))
			.ThenBy(s => s.Title)
			.Take(6)
			.ToListAsync(ct);

		// Günün restoranları – günün seed'i ile rastgele seçim
		var today = DateTime.UtcNow.DayOfYear + DateTime.UtcNow.Year * 1000;
		var allIds = await baseQuery.Select(s => s.Id).ToListAsync(ct);
		var rng = new Random(today);
		var dailyIds = allIds.OrderBy(_ => rng.Next()).Take(6).ToList();
		var dailyStores = dailyIds.Count > 0
			? await baseQuery.Where(s => dailyIds.Contains(s.Id)).Include(s => s.Address).Include(s => s.Medias).Include(s => s.Company).ToListAsync(ct)
			: new List<Store>();

		return new HomepageStoresDTO
		{
			NewStores = newStores.Select(MapToDTO).ToList(),
			PopularStores = popularStores.Select(MapToDTO).ToList(),
			DailyStores = dailyStores.Select(MapToDTO).ToList(),
		};
	}

	private static StoreSearchResultDTO MapToDTO(Store store)
	{
		var storeImage = store.Medias?
			.Where(m => m.Kind == MediaKind.Image)
			.OrderBy(m => m.SortOrder)
			.FirstOrDefault();

		string? city = null;
		string? district = null;
		if (store.Address?.FullAddress != null)
		{
			var parts = store.Address.FullAddress.Split(',', StringSplitOptions.TrimEntries);
			if (parts.Length >= 2)
			{
				city = parts[^1];
				district = parts.Length >= 3 ? parts[^2] : parts[0];
			}
			else if (parts.Length == 1)
			{
				city = parts[0];
			}
		}

		return new StoreSearchResultDTO
		{
			Id = store.Id,
			Title = store.Title,
			Slug = store.Slug,
			CompanySlug = store.Company.Slug,
			ImageUrl = storeImage?.MediaUrl,
			City = city,
			District = district,
		};
	}
}
