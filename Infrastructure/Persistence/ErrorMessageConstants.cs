namespace Infrastructure.Persistence;

public class ErrorMessageConstants
{
    #region GenericErrors

    public const string GENERIC_ERROR = "Bir hata oluştu.";
    public const string NULL_API_RESPONSE = "API'den geçersiz yanıt alındı.";
    public const string API_ERROR = "API hatası oluştu.";
    public const string NOT_FOUND = "Kayıt bulunamadı.";
    public const string DUPLICATE = "Kayıt zaten mevcut.";
    public const string INVALID_ACTION = "Geçersiz işlem.";
    public const string FOREIGNKEY_ERROR = "İlişkili kayıt bulunduğu için işleminiz gerçekleştirilememiştir.";

    #endregion GenericErrors
}
