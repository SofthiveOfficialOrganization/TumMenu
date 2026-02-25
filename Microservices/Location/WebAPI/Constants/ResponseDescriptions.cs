namespace WebAPI.Constants;

public static class ResponseDescriptions
{
    #region Provinces Descriptions

    public const string PROVINCES_FETCH_TURKEY_DATA =
        "- https://turkiyeapi.dev/turkiyeapi.dev kullanılarak veri tabanına Türkiye'de bulunan il ve ilçe bilgileri eklenir.<br>" +
        "- Bu işlemi yapmadan önce veri tabanındaki verileri sildiğinizden emin olun.<br>" +
        "- Eğer veri tabanında herhangi bir il veya ilçe var ise sistem hata verecektir. Bu kontrolün amacı tekrar eden veri ihtimalini kaldırmak.<br>";

    public const string PROVINCES_CLEAR_DISTRICT_AND_PROVINCES =
        "- Veri tabanındaki il ve ilçe bilgilerini silmek için kullanılır.";

    public const string PROVINCES_GET_BY_PROVINCE_ID =
        "- Belirtilen il kimliğine (provinceId) göre il bilgilerini getirir.<br>" +
        "- Eğer girilen kimlik veri tabanında yoksa sistem hata döndürür.";

    public const string PROVINCES_GET_ALL =
        "- Veri tabanındaki tüm il bilgilerini getirir.";

    public const string PROVINCES_GET_LIS =
        "- Sayfalama desteğiyle il bilgilerini listeler.<br>" +
        "- Kullanıcı belirli bir sayfa numarası ve sayfa boyutu ile verileri alabilir.";

    public const string PROVINCES_GET_LIST_BY_DYNAMIC =
        "- Dinamik sorgular kullanılarak il bilgilerini filtreleme ve sıralama yaparak listeler.<br>" +
        "- Sayfalama desteği ile belirli bir sayfa ve boyuta göre verileri döndürür.";

    public const string PROVINCES_GET_ONLY_PROVINCES = "Sadece il bilgilerini listeler. İlçeler dahil edilmez.";
    
    #endregion
    
    #region Districts Descriptions

    public const string DISTRICTS_GET_LIST_BY_PROVINCE_ID =
        "- Belirtilen il kimliğine (provinceId) göre ilçeleri listeler.<br>" +
        "- Eğer girilen kimlik veri tabanında yoksa sistem hata döndürür.";

    public const string DISTRICTS_GET_ALL =
        "- Veri tabanındaki tüm ilçe bilgilerini getirir.";

    public const string DISTRICTS_GET_LIST =
        "- Sayfalama desteğiyle ilçe bilgilerini listeler.<br>" +
        "- Kullanıcı belirli bir sayfa numarası ve sayfa boyutu ile verileri alabilir.";

    public const string DISTRICTS_GET_LIST_BY_DYNAMIC =
        "- Dinamik sorgular kullanılarak ilçe bilgilerini filtreleme ve sıralama yaparak listeler.<br>" +
        "- Sayfalama desteği ile belirli bir sayfa ve boyuta göre verileri döndürür.";

    #endregion
    
    #region Google Cloud Locations Descriptions

    public const string GOOGLE_CLOUD_GET_COORDINATES =
        "- Verilen adres bilgisine göre coğrafi koordinatları getirir.<br>" +
        "- Google Cloud'un harita API'si kullanılarak enlem ve boylam bilgileri döndürülür.";

    public const string GOOOGLE_CLOUD_GET_ADDRESS_SUGGESTIONS =
        "- Kullanıcının girdiği adres bilgisini tamamlayacak şekilde öneri adresleri listeler.<br>" +
        "- Google Cloud'un otomatik tamamlama API'si kullanılarak en iyi eşleşmeler getirilir.";

    #endregion
    
    #region AWS Locations Descriptions

    public const string AWS_GET_COORDINATES =
        "- Verilen adres bilgisine göre coğrafi koordinatları getirir.<br>" +
        "- AWS Location Service kullanılarak enlem ve boylam bilgileri döndürülür.";

    public const string AWS_GET_ADDRESS_SUGGESTIONS =
        "- Kullanıcının girdiği adres bilgisini tamamlayacak şekilde öneri adresleri listeler.<br>" +
        "- AWS Location Service'in otomatik tamamlama API'si kullanılarak en iyi eşleşmeler getirilir.";

    public const string AWS_GET_DISTRICTS_BY_CITY =
        "- Belirtilen şehir adına (cityName) göre ilçeleri listeler.<br>" +
        "- AWS Location Service kullanılarak şehir bazlı ilçe verileri getirilir.";

    #endregion
    
    #region Countries Descriptions

    public const string COUNTRIES_FETCH_DATA =
        "- RestCountries API kullanılarak ülke verileri veri tabanına eklenir.<br>" +
        "- Bu işlemi yapmadan önce veri tabanındaki mevcut verileri temizlediğinizden emin olun.<br>" +
        "- Eğer veri tabanında herhangi bir ülke verisi bulunuyorsa sistem hata verecektir.<br>";

    public const string COUNTRIES_CLEAR_DATA =
        "- Veri tabanındaki tüm ülke verilerini siler.<br>" +
        "- İşlem sonrası veri tabanında hiç ülke kaydı kalmaz.";

    public const string COUNTRIES_GET_ALL =
        "- Veri tabanındaki tüm ülke bilgilerini getirir.<br>" +
        "- Ülkeler tüm detaylarıyla birlikte döndürülür.";

    public const string COUNTRIES_GET_ALL_BASIC_INFO =
        "- Tüm ülkelerin temel bilgilerini getirir.<br>" +
        "- Bu metod, yalnızca ülkenin temel kimlik bilgilerini içerir ve detaylı verileri içermez.";

    public const string COUNTRIES_GET_LIST =
        "- Sayfalama desteğiyle ülke bilgilerini listeler.<br>" +
        "- Kullanıcı belirli bir sayfa numarası ve sayfa boyutu ile verileri alabilir.";

    public const string COUNTRIES_GET_LIST_BY_DYNAMIC =
        "- Dinamik sorgular kullanılarak ülke bilgilerini filtreleme ve sıralama yaparak listeler.<br>" +
        "- Sayfalama desteği ile belirli bir sayfa ve boyuta göre verileri döndürür.";
    
    #endregion
}