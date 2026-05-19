namespace Application.MenuDesigns;

public static class MenuDesignFieldHints
{
    public static IReadOnlyDictionary<string, string> All { get; } = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["Name"] = "Tasarımın yönetim panelinde görünen adı.",
        ["Slug"] = "Sistem içinde kullanılan benzersiz kısa kod (URL benzeri).",
        ["Description"] = "Tasarım hakkında kısa açıklama; yalnızca yönetim panelinde görünür.",
        ["PrimaryColor"] = "Menüdeki butonlar, vurgular ve marka öğelerinde kullanılan ana renk (--pm-brand).",
        ["PrimaryDarkColor"] = "Hover durumları, koyu vurgular ve header gradyanının ikinci rengi (--pm-brand-dark).",
        ["AccentColor"] = "İkincil vurgular ve dikkat çekici detaylar için kullanılır (--pm-accent).",
        ["BackgroundColor"] = "Menü sayfasının genel arka plan rengi (--pm-bg).",
        ["SurfaceColor"] = "Kartlar ve yüzey alanlarının arka plan rengi (--pm-surface).",
        ["TextColor"] = "Ürün adları, açıklamalar ve genel gövde metinlerinin rengi (--pm-text).",
        ["MutedColor"] = "Fiyat altı metinler, notlar ve ikincil bilgilerin rengi (--pm-muted).",
        ["BorderRadius"] = "Kartlar, butonlar ve yuvarlatılmış köşeli tüm öğelerin köşe yarıçapı (--pm-radius).",
        ["ButtonStyle"] = "Menü içi butonların görünümü: dolu, çerçeveli veya şeffaf.",
        ["BackgroundGradient"] = "Üst hero/header bölümündeki gradyan arka plan (--pm-header-gradient).",
        ["BackgroundImageUrl"] = "Menü sayfası arka planında kullanılan filigran veya desen görseli.",
        ["PreviewImageUrl"] = "Yönetim panelindeki tasarım seçim kartlarında gösterilen önizleme görseli.",
        ["SortOrder"] = "Tasarım listesindeki sıralama; küçük değerler önce gelir.",
        ["IsDefault"] = "Yeni menüler için otomatik seçilecek varsayılan tasarım.",
        ["HeaderBackgroundColor"] = "Menü üstündeki hero/header bölümünün düz arka plan rengi.",
        ["HeaderTextColor"] = "Header’daki dükkan adı, şirket adı ve üst navigasyon metinlerinin rengi.",
        ["CardBorderColor"] = "Kategori ve ürün kartlarının kenarlık rengi (--pm-border).",
        ["CardBorderWidth"] = "Kartların kenarlık kalınlığı; örn. 1px veya 2px.",
        ["CardBackgroundColor"] = "Kartların arka plan rengi; boş bırakılırsa yüzey rengi kullanılır.",
        ["CardShadow"] = "Kartların gölge efekti (--pm-shadow).",
        ["CardImageBorderRadius"] = "Ürün ve kategori görsellerinin köşe yarıçapı.",
        ["FooterBackgroundColor"] = "Sayfanın alt kısmındaki site footer’ının arka plan rengi.",
        ["FooterTextColor"] = "Footer’daki başlık, link ve alt bilgi metinlerinin rengi.",
        ["FooterButtonColor"] = "Footer’daki sosyal medya ve aksiyon butonlarının ana rengi.",
        ["FooterButtonStyle"] = "Footer butonlarının stili; boş bırakılırsa genel tuş stili uygulanır.",
        ["FontFamily"] = "Menü gövde metinlerinin yazı tipi (--pm-font).",
        ["BodyFontSize"] = "Genel gövde metin boyutu; örn. 1rem veya 15px.",
        ["HeadingFontFamily"] = "Kategori başlıkları ve bölüm başlıklarının yazı tipi (--pm-heading-font).",
        ["HeadingFontSize"] = "Hero’daki büyük dükkan adı boyutu (--pm-heading-size).",
        ["SubheadingFontSize"] = "Kategori ve bölüm başlıklarının boyutu (--pm-subheading-size).",
        ["HeadingTextColor"] = "Kategori başlıkları ve bölüm başlıklarının metin rengi; boşsa genel metin rengi.",
        ["HeadingBorderWidth"] = "Başlıkların alt/çevre kenarlık kalınlığı; örn. 0, 2px veya 0 0 3px 0.",
        ["HeadingBorderColor"] = "Başlık kenarlığının rengi; genelde marka veya vurgu rengiyle uyumlu.",
        ["HeadingFontWeight"] = "Başlıkların kalınlığı; örn. 700, 800 veya 900.",
        ["LinkColor"] = "Menü içi bağlantıların ve tıklanabilir metinlerin rengi.",
        ["PriceColor"] = "Ürün fiyatlarının rengi; boş bırakılırsa soluk metin rengi kullanılır.",
        ["ButtonTextColor"] = "Menü butonlarının metin rengi; boş bırakılırsa stile göre otomatik.",
        ["ButtonBorderColor"] = "Menü butonlarının kenarlık rengi; boş bırakılırsa ana renk kullanılır.",
        ["ButtonBorderRadius"] = "Yalnızca butonlar için köşe yarıçapı; boş bırakılırsa genel köşe yarıçapı.",
        ["SurfaceOpacity"] = "Modal ve yarı saydam panellerin yüzey opaklığı (0–100). Varsayılan: 94.",
        ["CardOpacity"] = "Kategori ve ürün kartlarının arka plan opaklığı (0–100). 100 = tam opak.",
        ["HeaderSecondaryTextOpacity"] = "Header’daki alt başlık ve açıklama metinlerinin opaklığı (0–100). Varsayılan: 86.",
        ["HeaderAccentTextOpacity"] = "Header’daki üst etiket ve şirket adı metinlerinin opaklığı (0–100). Varsayılan: 94.",
        ["PageBackgroundOverlayOpacity"] = "Sayfa arka planındaki renk katmanının opaklığı (0–100). Varsayılan: 68.",
        ["MutedTextOpacity"] = "Soluk/ikincil metinlerin görünürlüğü (0–100). Düşük değer = daha silik.",
        ["PanelOpacity"] = "İşletme bilgisi gibi açılır panellerin cam efekti opaklığı (0–100). Varsayılan: 80.",
        ["HeaderChipOpacity"] = "Header’daki rozet ve istatistik arka planlarının opaklığı (0–100). Varsayılan: 12."
    };

    public static string? Get(string fieldName) =>
        All.TryGetValue(fieldName, out var hint) ? hint : null;
}
