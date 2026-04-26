using Application.Microservices.Location.DTOs;
using Application.Microservices.DTOs;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Net.Http.Headers;

namespace Application.Microservices.Location
{
	public class LocationMicroservice : ILocationMicroservice
	{
		private readonly HttpClient _httpClient;
		private readonly LocationMicroserviceOptions _options;

		public LocationMicroservice(HttpClient httpClient, IOptions<LocationMicroserviceOptions> options)
		{
			_httpClient = httpClient;
			_options = options.Value;

			_httpClient.BaseAddress = new Uri(_options.BaseUrl);
			_httpClient.Timeout = TimeSpan.FromSeconds(5);
			_httpClient.DefaultRequestHeaders.Accept.Add(
				new MediaTypeWithQualityHeaderValue("text/plain"));
			_httpClient.DefaultRequestHeaders.Authorization =
				new AuthenticationHeaderValue("Basic", _options.Authorization.Replace("Basic ", ""));
		}

		public async Task<List<GetCountriesResponseDTO>> GetAllCountriesAsync()
		{
			var response = await _httpClient.GetAsync("api/Countries/GetAllBasicInfo");

			if(response.IsSuccessStatusCode)
			{
				var content = await response.Content.ReadAsStringAsync();
				var result = JsonSerializer.Deserialize<ApiResponse<List<GetCountriesResponseDTO>>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
				return result?.Data ?? [];
			}

			return [];
		}

		public async Task<List<GetProvincesResponseDTO>> GetAllProvincesAsync()
		{
			var response = await _httpClient.GetAsync("api/Provinces/GetOnlyProvinces");

			if(response.IsSuccessStatusCode)
			{
				var content = await response.Content.ReadAsStringAsync();
				var result = JsonSerializer.Deserialize<ApiResponse<List<GetProvincesResponseDTO>>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
				return result?.Data ?? [];
			}

			return [];
		}
		public async Task<List<GetDisctrictsResponseDTO>> GetDistrictsByProvinceIdAsync(Guid provinceId)
		{
			try
			{
				var response = await _httpClient.GetAsync($"api/Districts/GetListByProvinceId/{provinceId}");

				if (response.IsSuccessStatusCode)
				{
					var content = await response.Content.ReadAsStringAsync();
					if (string.IsNullOrWhiteSpace(content)) return [];
					var result = JsonSerializer.Deserialize<ApiResponse<List<GetDisctrictsResponseDTO>>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
					return result?.Data ?? [];
				}
			}
			catch { }

			return [];
		}

		public async Task<GetProvinceResponseBasicDTO?> GetProvinceBasicByIdAsync(Guid provinceId)
		{
			try
			{
				var response = await _httpClient.GetAsync($"api/Provinces/GetByProvinceId/{provinceId}");
				if (response.IsSuccessStatusCode)
				{
					var content = await response.Content.ReadAsStringAsync();
					if (string.IsNullOrWhiteSpace(content)) return null;
					var result = JsonSerializer.Deserialize<ApiResponse<GetProvinceResponseBasicDTO>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
					return result?.Data;
				}
			}
			catch { }

			return null;
		}
	}
}
