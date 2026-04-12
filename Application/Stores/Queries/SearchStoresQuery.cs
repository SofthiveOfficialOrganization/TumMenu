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
}

public class SearchStoresQueryHandler(
	IRepository<Store> repoStore,
	IRepository<Product> repoProduct
) : IRequestHandler<SearchStoresQuery, StoreSearchResultListDTO>
{
	private const double EarthRadiusKm = 6371.0;

	public async Task<StoreSearchResultListDTO> Handle(SearchStoresQuery req, CancellationToken ct)
	{
		// 1. Base Query with minimal data for filtering
		var query = repoStore.Query(tracked: false)
			.Where(s => !s.IsDeleted);

		var searchTerm = req.SearchTerm?.Trim().ToLower();
		bool hasSearchTerm = !string.IsNullOrEmpty(searchTerm);

		// 2. Application of SQL-side filters
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

		// 3. Project to lightweight objects for distance calculation and identification
		var lightStores = await query
			.Select(s => new
			{
				s.Id,
				s.Title,
				s.Slug,
				CompanySlug = s.Company.Slug,
				CompanyName = s.Company.Title,
				Latitude = s.Address != null ? s.Address.Latitude : null,
				Longitude = s.Address != null ? s.Address.Longitude : null,
				FullAddress = s.Address != null ? s.Address.FullAddress : null,
				MatchedProductId = hasSearchTerm ? s.Menus
					.Where(m => m.Status == MenuStatus.Active)
					.SelectMany(m => m.Categories)
					.Where(c => c.IsActive)
					.SelectMany(c => c.Products)
					.Where(p => p.IsActive && p.Title.ToLower().Contains(searchTerm!))
					.Select(p => (Guid?)p.Id)
					.FirstOrDefault() : null
			})
			.ToListAsync(ct);

		// 4. In-memory Distance calculation + initial ranking
		bool hasUserLocation = req.UserLatitude.HasValue && req.UserLongitude.HasValue;
		var candidateResults = new List<StoreSearchMatch>();

		foreach (var s in lightStores)
		{
			double? distance = null;
			if (hasUserLocation && s.Latitude.HasValue && s.Longitude.HasValue)
			{
				bool coordsValid = s.Latitude >= -90 && s.Latitude <= 90 && s.Longitude >= -180 && s.Longitude <= 180;
				if (!coordsValid) continue;

				distance = CalculateHaversineDistance(req.UserLatitude!.Value, req.UserLongitude!.Value, s.Latitude.Value, s.Longitude.Value);
				if (req.MaxDistanceKm.HasValue && distance > req.MaxDistanceKm.Value) continue;
			}

			candidateResults.Add(new StoreSearchMatch
			{
				Id = s.Id,
				Title = s.Title,
				Slug = s.Slug,
				CompanySlug = s.CompanySlug,
				CompanyName = s.CompanyName,
				Latitude = s.Latitude,
				Longitude = s.Longitude,
				FullAddress = s.FullAddress,
				DistanceKm = distance,
				MatchedProductId = s.MatchedProductId
			});
		}

		// 5. Sorting
		var sortedResults = hasUserLocation 
			? candidateResults.OrderBy(r => r.DistanceKm ?? double.MaxValue).ToList() 
			: candidateResults.OrderBy(r => r.Title).ToList();

		var totalCount = sortedResults.Count;
		var page = req.Page;
		var pageSize = req.PageSize > 0 ? req.PageSize : 12;
		var from = req.From;

		// 6. Pagination
		var pagedMatches = sortedResults
			.Skip((page - from) * pageSize)
			.Take(pageSize)
			.ToList();

		if (!pagedMatches.Any())
		{
			return new StoreSearchResultListDTO { Page = page, PageSize = pageSize, TotalCount = totalCount };
		}

		// 7. Deferred load of heavy data (Images) ONLY for the result page
		var storeIds = pagedMatches.Select(m => m.Id).ToList();
		var productIds = pagedMatches.Where(m => m.MatchedProductId.HasValue).Select(m => m.MatchedProductId!.Value).ToList();

		var storeMedias = await repoStore.Query(tracked: false)
			.Where(s => storeIds.Contains(s.Id))
			.Select(s => new { s.Id, Medias = s.Medias.Where(m => m.IsDeleted == false).OrderBy(m => m.SortOrder).ToList() })
			.ToListAsync(ct);

		var matchedProducts = productIds.Any()
			? await repoProduct.Query(tracked: false)
				.Where(p => productIds.Contains(p.Id))
				.Select(p => new { p.Id, p.Title, p.BasePrice, Medias = p.Medias.Where(m => m.IsDeleted == false).OrderBy(m => m.SortOrder).ToList() })
				.ToListAsync(ct)
			: [];

		// 8. Final Mapping
		var dtos = pagedMatches.Select(match =>
		{
			var fullStore = storeMedias.First(s => s.Id == match.Id);
			var storeImage = fullStore.Medias.FirstOrDefault(m => m.Kind == MediaKind.Image)?.MediaUrl;
			
			// Try to extract city/district from FullAddress
			string? city = null;
			string? district = null;
			if (match.FullAddress != null)
			{
				var parts = match.FullAddress.Split(',', StringSplitOptions.TrimEntries);
				if (parts.Length >= 2)
				{
					city = parts[^1];
					district = parts.Length >= 3 ? parts[^2] : parts[0];
				}
				else if (parts.Length == 1) city = parts[0];
			}

			var dto = new StoreSearchResultDTO
			{
				Id = match.Id,
				Title = match.Title,
				Slug = match.Slug,
				CompanySlug = match.CompanySlug,
				CompanyName = match.CompanyName,
				ImageUrl = storeImage,
				City = city,
				District = district,
				DistanceKm = match.DistanceKm.HasValue ? Math.Round(match.DistanceKm.Value, 2) : null,
				Latitude = match.Latitude,
				Longitude = match.Longitude
			};

			if (match.MatchedProductId.HasValue)
			{
				var fullProd = matchedProducts.FirstOrDefault(p => p.Id == match.MatchedProductId.Value);
				if (fullProd != null)
				{
					dto.MatchedProduct = new MatchedProductDTO
					{
						Id = fullProd.Id,
						Title = fullProd.Title,
						BasePrice = fullProd.BasePrice,
						ImageUrl = fullProd.Medias.FirstOrDefault(m => m.Kind == MediaKind.Image)?.MediaUrl
					};
				}
			}

			return dto;
		}).ToList();

		return new StoreSearchResultListDTO
		{
			Items = dtos,
			TotalCount = totalCount,
			Page = page,
			PageSize = pageSize,
			HasNext = (page - from + 1) * pageSize < totalCount
		};
	}

	private class StoreSearchMatch
	{
		public Guid Id { get; set; }
		public string Title { get; set; } = null!;
		public string Slug { get; set; } = null!;
		public string CompanySlug { get; set; } = null!;
		public string? CompanyName { get; set; }
		public double? Latitude { get; set; }
		public double? Longitude { get; set; }
		public string? FullAddress { get; set; }
		public double? DistanceKm { get; set; }
		public Guid? MatchedProductId { get; set; }
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
