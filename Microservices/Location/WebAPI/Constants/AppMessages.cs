namespace WebAPI.Constants;

public static class AppMessages
{
    public const string SERVER_ERROR_TITLE = "Sunucu Hatası";
    public const string SERVER_ERROR_DETAIL = "Sunucuya ulaşılamıyor.";
    public const string MAINTENANCE_TIME = "Sistem Bakımda.";
    public const string AUTHORIZATION_DENIED = "Bu işlemi gerçekleştirmek için yetkiniz bulunmamaktadır.";

    // 🔹 Kullanıcı dostu kimlik doğrulama hata mesajları
    public const string MISSING_AUTH_HEADER = "Kimlik doğrulama bilgileri eksik. Lütfen giriş bilgilerinizi ekleyerek tekrar deneyin.";
    public const string INVALID_AUTH_HEADER = "Geçersiz kimlik doğrulama formatı. Lütfen geçerli bir kullanıcı adı ve şifre ile tekrar deneyin.";
    public const string INVALID_CREDENTIALS = "Girdiğiniz kullanıcı adı veya şifre hatalı. Lütfen bilgilerinizi kontrol edip tekrar deneyin.";

    public const string LOCATION_NOT_FOUND = "Girilen adres için konum bulunamadı. Lütfen doğru bir adres girdiğinizden emin olun.";
    
    #region AWS Messages

    public const string AWS_RESPONSE_EMPTY = "AWS'den geçerli bir yanıt alınamadı. Lütfen adresinizi kontrol edip tekrar deneyin.";
    public const string AWS_CONNECTION_ERROR = "AWS servisleriyle bağlantı kurulamadı. Lütfen internet bağlantınızı kontrol edin veya daha sonra tekrar deneyin.";
    public const string AWS_AUTH_ERROR = "AWS kimlik doğrulaması başarısız oldu. Lütfen API anahtarlarınızı kontrol edin.";
    public const string AWS_INVALID_INDEX = "Geçersiz AWS Place Index kullanıldı. Lütfen yapılandırmanızı kontrol edin.";
    public const string AWS_RATE_LIMIT_EXCEEDED = "AWS istek sınırına ulaşıldı. Lütfen daha sonra tekrar deneyin.";
    public const string INVALID_ADDRESS = "Geçersiz adres girdiniz.";

    #endregion
    
    
    #region Province Messages

    public const string PROVINCE_NOT_FOUND = "Şehir bulunamadı.";
    public const string NO_PROVINCE_FOUND_TO_DELETE = "Silinecek şehir bulunamadı.";
    public const string SHOULD_NOT_BE_A_REGISTERED_PROVINCE = "Kayıtlı şehir bulunmamalı. Öncelikle veri tabanını temizlemelisiniz.";


    #endregion

    #region District Messages

    public const string DISTRICT_NOT_FOUND = "İlçe bulunamadı.";

    #endregion
    
    #region Country Messages

    public const string COUNTRY_NOT_FOUND = "Ülke bulunamadı.";
    public const string NO_COUNTRY_FOUND_TO_DELETE = "Silinecek ülke bulunamadı.";
    public const string SHOULD_NOT_BE_A_REGISTERED_COUNTRY = "Kayıtlı ülke bulunmamalı. Öncelikle veri tabanını temizlemelisiniz.";
    public const string COUNTRIES_DATA_NOT_FOUND = "RestCountries API'den geçerli bir ülke verisi alınamadı.";
    public const string COUNTRY_API_CONNECTION_ERROR = "Ülke verileri çekilirken hata oluştu. Lütfen daha sonra tekrar deneyin.";
    public const string COUNTRY_ALREADY_EXISTS = "Bu ülke zaten kayıtlı.";

    #endregion
}