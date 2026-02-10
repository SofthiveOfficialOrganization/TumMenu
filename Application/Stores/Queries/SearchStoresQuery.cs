using Application.Abstractions;
using Application.Common.Base.Page.RequestBase;
using Application.Stores.DTOs;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Stores.Queries;

public sealed class SearchStoresQuery : PageRequest, IRequest<StoreSearchResultListDTO>
{
	public string? SearchTerm { get; set; }
	public List<Guid>? CategoryLibraryItemIds { get; set; }
	public bool? IsVegan { get; set; }
	public double? UserLatitude { get; set; }
	public double? UserLongitude { get; set; }
	public double? MaxDistanceKm { get; set; }
	public string? City { get; set; }
	public string? District { get; set; }
}

public class SearchStoresQueryHandler(
	IRepository<Store> repoStore
) : IRequestHandler<SearchStoresQuery, StoreSearchResultListDTO>
{
	private const double EarthRadiusKm = 6371.0;

	public async Task<StoreSearchResultListDTO> Handle(SearchStoresQuery req, CancellationToken ct)
	{
		var query = repoStore.Query()
			.Include(s => s.Address)
			.Include(s => s.Medias)
			.Include(s => s.Menus.Where(m => m.Status == MenuStatus.Active))
				.ThenInclude(m => m.Categories.Where(c => c.IsActive))
					.ThenInclude(c => c.CategoryLibraryItem)
			.Include(s => s.Menus.Where(m => m.Status == MenuStatus.Active))
				.ThenInclude(m => m.Categories.Where(c => c.IsActive))
					.ThenInclude(c => c.Products.Where(p => p.IsActive))
						.ThenInclude(p => p.Medias)
			.Where(s => !s.IsDeleted)
			.AsQueryable();

		// Search term filter
		var searchTerm = req.SearchTerm?.Trim().ToLower();
		bool hasSearchTerm = !string.IsNullOrEmpty(searchTerm);

		if (hasSearchTerm)
		{
			query = query.Where(s =>
				s.Title.ToLower().Contains(searchTerm!) ||
				s.Menus.Any(m => m.Status == MenuStatus.Active &&
					m.Categories.Any(c => c.IsActive &&
						(c.CategoryLibraryItem.Title.ToLower().Contains(searchTerm!) ||
						 c.Products.Any(p => p.IsActive && p.Title.ToLower().Contains(searchTerm!)))
					)
				)
			);
		}

		// Category filter
		if (req.CategoryLibraryItemIds is { Count: > 0 })
		{
			query = query.Where(s =>
				s.Menus.Any(m => m.Status == MenuStatus.Active &&
					m.Categories.Any(c => c.IsActive &&
						req.CategoryLibraryItemIds.Contains(c.CategoryLibraryItemId)
					)
				)
			);
		}

		// Vegan filter
		if (req.IsVegan == true)
		{
			query = query.Where(s =>
				s.Menus.Any(m => m.Status == MenuStatus.Active &&
					m.Categories.Any(c => c.IsActive &&
						c.Products.Any(p => p.IsActive && p.IsVegan == true)
					)
				)
			);
		}

		// City/District filter via FullAddress
		if (!string.IsNullOrEmpty(req.City))
		{
			var cityLower = req.City.Trim().ToLower();
			query = query.Where(s =>
				s.Address != null &&
				s.Address.FullAddress != null &&
				s.Address.FullAddress.ToLower().Contains(cityLower)
			);
		}
		if (!string.IsNullOrEmpty(req.District))
		{
			var districtLower = req.District.Trim().ToLower();
			query = query.Where(s =>
				s.Address != null &&
				s.Address.FullAddress != null &&
				s.Address.FullAddress.ToLower().Contains(districtLower)
			);
		}

		// Materialize before distance calculation (Haversine can't translate to SQL easily)
		var allStores = await query.ToListAsync(ct);

		// Distance calculation + filter
		bool hasUserLocation = req.UserLatitude.HasValue && req.UserLongitude.HasValue;
		var results = new List<(Store store, double? distance, Product? matchedProduct)>();

		foreach (var store in allStores)
		{
			double? distance = null;

			if (hasUserLocation && store.Address?.Latitude != null && store.Address?.Longitude != null)
			{
				distance = CalculateHaversineDistance(
					req.UserLatitude!.Value, req.UserLongitude!.Value,
					store.Address.Latitude!.Value, store.Address.Longitude!.Value
				);

				if (req.MaxDistanceKm.HasValue && distance > req.MaxDistanceKm.Value)
					continue;
			}
			else if (req.MaxDistanceKm.HasValue)
			{
				// If distance filter is set but no coordinates available, skip
				continue;
			}

			// Find matched product if search was by product name
			Product? matchedProduct = null;
			if (hasSearchTerm && !store.Title.ToLower().Contains(searchTerm!))
			{
				matchedProduct = store.Menus
					.Where(m => m.Status == MenuStatus.Active)
					.SelectMany(m => m.Categories)
					.Where(c => c.IsActive)
					.SelectMany(c => c.Products)
					.Where(p => p.IsActive && p.Title.ToLower().Contains(searchTerm!))
					.FirstOrDefault();
			}

			results.Add((store, distance, matchedProduct));
		}

		// Sort by distance if available, otherwise by title
		if (hasUserLocation)
			results = results.OrderBy(r => r.distance ?? double.MaxValue).ToList();
		else
			results = results.OrderBy(r => r.store.Title).ToList();

		var totalCount = results.Count;
		var page = req.Page;
		var pageSize = req.PageSize > 0 ? req.PageSize : 12;

		var pagedResults = results
			.Skip(page * pageSize)
			.Take(pageSize)
			.ToList();

		var items = pagedResults.Select(r => MapToDTO(r.store, r.distance, r.matchedProduct)).ToList();

		return new StoreSearchResultListDTO
		{
			Items = items,
			TotalCount = totalCount,
			Page = page,
			PageSize = pageSize,
			HasNext = (page + 1) * pageSize < totalCount
		};
	}

	private static StoreSearchResultDTO MapToDTO(Store store, double? distance, Product? matchedProduct)
	{
		var storeImage = store.Medias?
			.Where(m => m.Kind == MediaKind.Image)
			.OrderBy(m => m.SortOrder)
			.FirstOrDefault();

		// Try to extract city/district from FullAddress
		string? city = null;
		string? district = null;
		if (store.Address?.FullAddress != null)
		{
			var parts = store.Address.FullAddress.Split(',', StringSplitOptions.TrimEntries);
			if (parts.Length >= 2)
			{
				city = parts[^1]; // last part typically city
				district = parts.Length >= 3 ? parts[^2] : parts[0];
			}
			else if (parts.Length == 1)
			{
				city = parts[0];
			}
		}

		var dto = new StoreSearchResultDTO
		{
			Id = store.Id,
			Title = store.Title,
			Slug = store.Slug,
			ImageUrl = storeImage?.MediaUrl,
			City = city,
			District = district,
			DistanceKm = distance.HasValue ? Math.Round(distance.Value, 1) : null,
		};

		if (matchedProduct != null)
		{
			var productImage = matchedProduct.Medias?
				.Where(m => m.Kind == MediaKind.Image)
				.OrderBy(m => m.SortOrder)
				.FirstOrDefault();

			dto.MatchedProduct = new MatchedProductDTO
			{
				Id = matchedProduct.Id,
				Title = matchedProduct.Title,
				BasePrice = matchedProduct.BasePrice,
				ImageUrl = productImage?.MediaUrl,
			};
		}

		return dto;
	}

	private static double CalculateHaversineDistance(double lat1, double lon1, double lat2, double lon2)
	{
		var dLat = DegreesToRadians(lat2 - lat1);
		var dLon = DegreesToRadians(lon2 - lon1);

		var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
				Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
				Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

		var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
		return EarthRadiusKm * c;
	}

	private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180.0;
}
