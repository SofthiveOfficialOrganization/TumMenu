namespace WebAPI.Constants;

public static class ApiUrls
{
    #region Turkey API Urls

    public const string TURKEY_API_BASE_URL = "https://turkiyeapi.dev/api/v1"; // Baz URL

    // Uzantılar
    public const string TURKEY_API_GET_ALL_PROVINCES = "/provinces";         // Tüm illeri getir
    public const string TURKEY_API_GET_ALL_DISTRICTS = "/districts";         // Tüm illeri getir

    public const string TURKEY_API_GET_DISTRICTS_BY_PROVINCE_ID = "/provinces"; // İlçe bilgilerini getir (cityId eklenir)
    public const string GET_PROVINCES_WITH_PAGINATION = "/provinces"; // Offset ve limit ile sayfalı illeri getir

    #endregion

    #region Google Cloud API Urls

    public const string GOOGLE_CLOUD_GEOCODE_API = "https://maps.googleapis.com/maps/api/geocode/json?address={0}&key={1}";
    public const string GOOGLE_CLOUD_AUTOCOMPLETE_API = "https://maps.googleapis.com/maps/api/place/autocomplete/json?input={0}&key={1}";

    #endregion
    
    public const string REST_COUNTRIES_API_BASE_URL = "https://restcountries.com/v3.1";
    public const string REST_COUNTRIES_API_GET_ALL_COUNTRIES = "/all";
}