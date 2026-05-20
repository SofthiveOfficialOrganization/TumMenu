# TumMenu Admin Front-End Toolbox

Bu doküman, Admin panelindeki detay, güncelleme, liste ve yönetim sayfalarını aynı görsel dilde üretmek için kullanılır.

Yeni Admin sayfası oluştururken veya mevcut detay sayfasını güncelleme sayfasına çevirirken önce bu dosyayı oku. Amaç her sayfada aynı başlık düzeni, aynı geri tuşu yeri, aynı buton metinleri, aynı kart/form hiyerarşisi ve aynı responsive davranışı kullanmaktır.

## Kapsam

- Alan: `WebUI/Areas/Admin/Views/**`
- Ana tema dosyası: `WebUI/wwwroot/css/admin-tummenu.css`
- Layout: `WebUI/Areas/Admin/Views/Shared/_AdminLayout.cshtml`
- UI temeli: Bootstrap 5, Font Awesome 6, `horizon-*` tema değişkenleri

## Tasarım İlkeleri

- Admin panel operasyonel bir araçtır; sayfalar sade, taranabilir ve tutarlı olmalıdır.
- Detay sayfaları mümkün oldukça güncelleme sayfasına dönüştürülmelidir. Kullanıcı ayrı bir "detay oku, sonra düzenle" akışına zorlanmamalıdır.
- Geri tuşu her sayfada aynı yerde olmalıdır: sayfanın ilk kartında, başlığın solunda.
- Ana aksiyonlar sağ üstte veya form footer'ında olmalıdır; aynı aksiyon farklı sayfalarda farklı isimlerle görünmemelidir.
- Admin içinde varsayılan yüzey `horizon-card` olmalıdır. Yeni sayfalarda ham Bootstrap `card` ancak üçüncü parti bileşen veya küçük lokal istisna için kullanılmalıdır.
- Inline style kullanma. Tekrar eden ölçü, boşluk, renk ve buton düzenleri sınıfa taşınmalıdır.
- Mobilde başlık, geri tuşu ve aksiyonlar alt alta akabilmelidir; metinler butonların dışına taşmamalıdır.

## Sayfa İskeleti

Her Admin sayfası şu sırayla başlamalıdır:

1. Razor değişkenleri: `ViewData["Title"]`, `returnUrl`, gerekiyorsa `currentPageUrl`
2. `tm-page-header` kartı
3. Ana içerik: form, tablo, yönetim kartları veya özel araç
4. Modal/script/style bölümleri

Standart değişken bloğu:

```cshtml
@{
    ViewData["Title"] = "Dükkan Düzenle";
    var returnUrl = ViewData["ReturnUrl"] as string ?? Url.Action("Index");
    var currentPageUrl = $"{Context.Request.Path}{Context.Request.QueryString}";
}
```

Standart sayfa başlığı:

```cshtml
<div class="horizon-card tm-page-header">
    <div class="tm-page-header__main">
        <a href="@returnUrl"
           data-tummenu-back
           data-fallback-url="@returnUrl"
           class="btn btn-outline-secondary tm-back-button"
           title="Geri Dön">
            <i class="fas fa-arrow-left"></i>
            <span>Geri Dön</span>
        </a>

        <div class="tm-page-heading">
            <div class="tm-page-eyebrow">Dükkan Yönetimi</div>
            <h1 class="tm-page-title">@Model.Title</h1>
            <p class="tm-page-subtitle">Dükkan bilgilerini, görünürlük ayarlarını ve medya alanlarını yönetin.</p>
        </div>
    </div>

    <div class="tm-page-actions">
        <a asp-action="Details" asp-route-id="@Model.Id" class="btn btn-outline-secondary">
            <i class="fas fa-eye me-2"></i>Önizle
        </a>
        <button type="submit" form="storeUpdateForm" class="btn btn-primary">
            <i class="fas fa-save me-2"></i>Değişiklikleri Kaydet
        </button>
    </div>
</div>
```

## Geri Tuşu Standardı

Geri tuşu:

- Her detay/güncelleme sayfasında ilk `horizon-card` içinde olmalı.
- Başlığın solunda olmalı.
- Metin her zaman `Geri Dön` olmalı.
- İkon her zaman `fas fa-arrow-left` olmalı.
- `data-tummenu-back` ve `data-fallback-url` kullanılmalı.
- Yuvarlak yalnız ikon butonu kullanılmamalı; masaüstünde metin görünmelidir.

Kullanılacak örnek:

```cshtml
<a href="@returnUrl"
   data-tummenu-back
   data-fallback-url="@returnUrl"
   class="btn btn-outline-secondary tm-back-button">
    <i class="fas fa-arrow-left"></i>
    <span>Geri Dön</span>
</a>
```

Kullanılmayacak varyasyonlar:

- `Geri`
- `İptal` geri navigasyonu için
- Kartın sağ üstünde tek başına geri butonu
- Detay kartının altında geri butonu
- Inline `width: 36px; height: 36px`

## Buton Sözlüğü

| Amaç | Sınıf | Metin | İkon |
| --- | --- | --- | --- |
| Geri | `btn btn-outline-secondary tm-back-button` | `Geri Dön` | `fas fa-arrow-left` |
| Oluştur | `btn btn-primary` | `Kaydet` | `fas fa-save` |
| Güncelle | `btn btn-primary` | `Değişiklikleri Kaydet` | `fas fa-save` |
| Listeye ekle | `btn btn-primary btn-sm` | `{Varlık} Ekle` | `fas fa-plus` |
| Düzenle | `btn btn-outline-primary tm-icon-button` | ikon-only | `fas fa-edit` |
| Sil | `btn btn-outline-danger tm-icon-button` | ikon-only | `fas fa-trash-alt` |
| Önizle | `btn btn-outline-secondary` | `Önizle` | `fas fa-eye` |
| Dış bağlantı | `btn btn-outline-secondary` | `Aç` / `Menüyü Gör` | `fas fa-external-link-alt` |
| İptal modal | `btn btn-outline-secondary` | `Vazgeç` | yok |
| Sil onayı | `btn btn-danger` | `Evet, Sil` | gerekirse `fas fa-trash-alt` |

Notlar:

- İkon-only butonlarda mutlaka `title` veya `aria-label` kullan.
- Form submit butonlarında ikon + metin kullan.
- Tablo satırı aksiyonlarında `btn-sm` kullan.
- Sayfa ana aksiyonlarında `btn-sm` kullanma; standart boyut kullan.
- Link görünümü için `btn btn-link` kullanma; Admin aksiyonları buton gibi görünmelidir.

## Form Standardı

Formlar tek bir büyük kart veya mantıksal bölümlere ayrılmış kartlar halinde olmalıdır.

Basit güncelleme formu:

```cshtml
<form asp-action="Update" method="post" id="storeUpdateForm" class="tm-form">
    @Html.AntiForgeryToken()
    <input type="hidden" asp-for="Id" />

    <div class="row g-3">
        <div class="col-md-6">
            <label asp-for="Title" class="form-label tm-form-label">Dükkan Adı</label>
            <input asp-for="Title" class="form-control" />
            <span asp-validation-for="Title" class="text-danger small"></span>
        </div>

        <div class="col-md-6">
            <label asp-for="Slug" class="form-label tm-form-label">Slug</label>
            <input asp-for="Slug" class="form-control" />
            <span asp-validation-for="Slug" class="text-danger small"></span>
        </div>
    </div>

    <div class="tm-form-footer">
        <a href="@returnUrl"
           data-tummenu-back
           data-fallback-url="@returnUrl"
           class="btn btn-outline-secondary">
            Vazgeç
        </a>
        <button type="submit" class="btn btn-primary">
            <i class="fas fa-save me-2"></i>Değişiklikleri Kaydet
        </button>
    </div>
</form>
```

Form bölümü:

```cshtml
<section class="horizon-card tm-form-section">
    <div class="tm-section-header">
        <div>
            <h2 class="tm-section-title">Görünürlük Ayarları</h2>
            <p class="tm-section-subtitle">Dükkanın public sayfada ve QR menüde nasıl görüneceğini belirler.</p>
        </div>
    </div>

    <!-- form controls -->
</section>
```

Form label standardı:

```cshtml
class="form-label tm-form-label"
```

`tm-form-label`, mevcut kullanımın daha okunur karşılığıdır:

```css
.tm-form-label {
    color: var(--horizon-text-secondary);
    font-size: .78rem;
    font-weight: 700;
    text-transform: uppercase;
    margin-bottom: .35rem;
}
```

## Kart ve Bölüm Standardı

Kart başlıklarında şu yapı kullanılmalıdır:

```cshtml
<div class="tm-section-header">
    <div>
        <h2 class="tm-section-title">Genel Bilgiler</h2>
        <p class="tm-section-subtitle">Temel kimlik ve görünürlük bilgileri.</p>
    </div>
    <div class="tm-section-actions">
        <!-- optional buttons -->
    </div>
</div>
```

Başlık boyutları:

- Sayfa başlığı: `tm-page-title`
- Kart/bölüm başlığı: `tm-section-title`
- İç küçük başlık: Bootstrap `h6.fw-bold`

Başlık içinde `h1`, `h2`, `h3` karmaşası yaratma. Sayfada tek `h1` olmalıdır.

## Detay Alanı Standardı

Okunur bilgi satırları için:

```cshtml
<dl class="tm-description-list">
    <div class="tm-info-row">
        <dt>Telefon</dt>
        <dd>@Model.PhoneNumber</dd>
    </div>
    <div class="tm-info-row">
        <dt>Durum</dt>
        <dd><span class="badge tm-badge-success">Aktif</span></dd>
    </div>
</dl>
```

Kullanım:

- Sadece read-only özel sayfalarda kullan: sistem logu, QR çıktısı, analitik özet.
- Normal varlık sayfalarında read-only detay yerine update formu tercih et.

## Badge Standardı

```cshtml
<span class="badge tm-badge-success"><i class="fas fa-check-circle me-1"></i>Aktif</span>
<span class="badge tm-badge-danger"><i class="fas fa-times-circle me-1"></i>Pasif</span>
<span class="badge tm-badge-neutral">Sıra: @Model.SortOrder</span>
<span class="badge tm-badge-warning"><i class="fas fa-thumbtack me-1"></i>Varsayılan</span>
```

Metin standardı:

- `Aktif`
- `Pasif`
- `Varsayılan`
- `Ana Menü`
- `Kapalı`
- `Açık`

Tamamen büyük harf badge yazma. `ANA MENÜ` yerine `Ana Menü`.

## Boş Durum Standardı

```cshtml
<div class="tm-empty-state">
    <i class="fas fa-images"></i>
    <h3>Henüz görsel yok</h3>
    <p>Bu alana logo, banner veya galeri görseli ekleyebilirsiniz.</p>
</div>
```

Boş durumlarda:

- Tek ikon kullan.
- Kısa başlık kullan.
- Yardımcı metni bir cümleyle sınırla.
- İç içe kart kullanma.

## Tablo Standardı

Liste sayfalarında:

```cshtml
<section class="horizon-card">
    <div class="tm-section-header">
        <div>
            <h1 class="tm-section-title">Dükkanlar</h1>
            <p class="tm-section-subtitle">Dükkan kayıtlarını yönetin.</p>
        </div>
        <a asp-action="Create" class="btn btn-primary">
            <i class="fas fa-plus me-2"></i>Dükkan Ekle
        </a>
    </div>

    <div class="table-responsive">
        <table class="table table-hover align-middle tm-table">
            <!-- rows -->
        </table>
    </div>
</section>
```

Satır aksiyon sırası:

1. Önizle veya detay
2. Düzenle
3. Sil

Detay sayfası kaldırılıp update sayfasına geçildiyse, satır ana tıklama hedefi update olmalıdır.

## Modal Standardı

Silme için SweetAlert tercih edilir:

```cshtml
<button type="button"
        class="btn btn-outline-danger tm-icon-button"
        title="Sil"
        onclick="TumMenuAlerts.confirm('Bu kaydı silmek istediğinize emin misiniz?', 'Bu işlem geri alınamaz.', 'Evet, Sil').then(res => { if(res) document.getElementById('deleteForm-@Model.Id').submit(); })">
    <i class="fas fa-trash-alt"></i>
</button>
```

Bootstrap modal sadece kullanıcıdan ek bilgi veya seçenek alınacaksa kullanılmalıdır.

## Medya Alanları

Medya kartları update sayfasının altında ayrı bölüm olarak durmalıdır.

```cshtml
<section class="horizon-card tm-media-section">
    <div class="tm-section-header">
        <div>
            <h2 class="tm-section-title">Dükkan Logosu</h2>
            <p class="tm-section-subtitle">Kare formatta görsel önerilir.</p>
        </div>
    </div>
    <partial name="_MediaGallery" model='(Model.Id, Domain.Entities.MediaRefType.Store, Model.Medias, "logo")' />
</section>
```

Medya başlıkları:

- `Logo`
- `Banner`
- `Galeri`
- `Ürün Görselleri`

## CSS Toolbox

Bu sınıflar `admin-tummenu.css` içinde yoksa eklenmelidir. Yeni sayfalarda inline style yerine bu sınıflar kullanılmalıdır.

```css
.tm-page-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 16px;
}

.tm-page-header__main {
    display: flex;
    align-items: center;
    gap: 14px;
    min-width: 0;
}

.tm-page-heading {
    min-width: 0;
}

.tm-page-eyebrow {
    color: var(--horizon-text-secondary);
    font-size: .78rem;
    font-weight: 700;
    line-height: 1.2;
    margin-bottom: 4px;
    text-transform: uppercase;
}

.tm-page-title {
    color: var(--horizon-text-primary);
    font-size: 1.45rem;
    font-weight: 800;
    line-height: 1.2;
    margin: 0;
    overflow-wrap: anywhere;
}

.tm-page-subtitle {
    color: var(--horizon-text-secondary);
    font-size: .92rem;
    line-height: 1.45;
    margin: 6px 0 0;
}

.tm-page-actions,
.tm-section-actions,
.tm-form-footer {
    display: flex;
    align-items: center;
    justify-content: flex-end;
    flex-wrap: wrap;
    gap: 10px;
}

.tm-back-button {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    gap: 8px;
    min-height: 40px;
    flex-shrink: 0;
}

.tm-icon-button {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    width: 38px;
    height: 38px;
    padding: 0;
    flex: 0 0 38px;
}

.tm-section-header {
    display: flex;
    align-items: flex-start;
    justify-content: space-between;
    gap: 16px;
    margin-bottom: 18px;
}

.tm-section-title {
    color: var(--horizon-text-primary);
    font-size: 1.05rem;
    font-weight: 800;
    line-height: 1.25;
    margin: 0;
}

.tm-section-subtitle {
    color: var(--horizon-text-secondary);
    font-size: .9rem;
    line-height: 1.45;
    margin: 5px 0 0;
}

.tm-form-section {
    margin-bottom: 20px;
}

.tm-form-footer {
    border-top: 1px solid rgba(163, 174, 208, .18);
    margin-top: 24px;
    padding-top: 18px;
}

.tm-form-label {
    color: var(--horizon-text-secondary);
    font-size: .78rem;
    font-weight: 700;
    text-transform: uppercase;
    margin-bottom: .35rem;
}

.tm-description-list {
    display: flex;
    flex-direction: column;
    gap: 0;
    margin: 0;
}

.tm-info-row {
    display: grid;
    grid-template-columns: minmax(140px, 32%) 1fr;
    gap: 16px;
    padding: 12px 0;
    border-bottom: 1px solid rgba(163, 174, 208, .18);
}

.tm-info-row:last-child {
    border-bottom: 0;
}

.tm-info-row dt {
    color: var(--horizon-text-secondary);
    font-size: .76rem;
    font-weight: 800;
    margin: 0;
    text-transform: uppercase;
}

.tm-info-row dd {
    color: var(--horizon-text-primary);
    font-weight: 650;
    margin: 0;
    min-width: 0;
    overflow-wrap: anywhere;
}

.tm-empty-state {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    min-height: 180px;
    padding: 28px;
    text-align: center;
    color: var(--horizon-text-secondary);
    border: 1px dashed rgba(163, 174, 208, .38);
    border-radius: 14px;
    background: rgba(244, 247, 254, .58);
}

.tm-empty-state i {
    font-size: 1.8rem;
    margin-bottom: 10px;
    opacity: .62;
}

.tm-empty-state h3 {
    color: var(--horizon-text-primary);
    font-size: 1rem;
    font-weight: 800;
    margin: 0 0 4px;
}

.tm-empty-state p {
    margin: 0;
    max-width: 420px;
}

.tm-badge-success,
.tm-badge-danger,
.tm-badge-warning,
.tm-badge-neutral {
    border-radius: 999px;
    padding: .42rem .62rem;
    font-weight: 800;
}

.tm-badge-success {
    color: #15803d;
    background: rgba(34, 197, 94, .12);
    border: 1px solid rgba(34, 197, 94, .24);
}

.tm-badge-danger {
    color: #dc2626;
    background: rgba(239, 68, 68, .12);
    border: 1px solid rgba(239, 68, 68, .24);
}

.tm-badge-warning {
    color: #92400e;
    background: rgba(245, 158, 11, .14);
    border: 1px solid rgba(245, 158, 11, .28);
}

.tm-badge-neutral {
    color: var(--horizon-text-secondary);
    background: rgba(163, 174, 208, .12);
    border: 1px solid rgba(163, 174, 208, .24);
}

html.dark-mode .tm-empty-state,
body.dark-mode .tm-empty-state {
    background: rgba(255, 255, 255, .035);
    border-color: rgba(255, 255, 255, .12);
}

html.dark-mode .tm-badge-success,
body.dark-mode .tm-badge-success {
    color: #bbf7d0;
}

html.dark-mode .tm-badge-danger,
body.dark-mode .tm-badge-danger {
    color: #fecaca;
}

html.dark-mode .tm-badge-warning,
body.dark-mode .tm-badge-warning {
    color: #fde68a;
}

@media (max-width: 768px) {
    .tm-page-header,
    .tm-section-header {
        align-items: stretch;
        flex-direction: column;
    }

    .tm-page-header__main {
        align-items: flex-start;
    }

    .tm-page-actions,
    .tm-section-actions,
    .tm-form-footer {
        justify-content: stretch;
    }

    .tm-page-actions > .btn,
    .tm-form-footer > .btn,
    .tm-form-footer > a.btn {
        flex: 1 1 auto;
    }

    .tm-back-button span {
        display: none;
    }

    .tm-info-row {
        grid-template-columns: 1fr;
        gap: 4px;
    }
}
```

## Detay Sayfalarını Güncelleme Sayfasına Çevirme Kılavuzu

Mevcut durumdaki detay sayfaları:

- `Company/Details.cshtml`
- `Store/Details.cshtml`
- `Menu/Details.cshtml`
- `Category/Details.cshtml`
- `Product/Details.cshtml`
- `CategoryLibraryItem/Details.cshtml`
- `QRManagement/Details.cshtml`
- `SystemLogs/Details.cshtml`

Dönüşüm yaklaşımı:

- `Company`, `Store`, `Menu`, `Product`, `CategoryLibraryItem`: varsayılan hedef güncelleme sayfası olmalı.
- `Category`: kategoriye bağlı alt kategori/ürün yönetimi olduğu için "yönetim + güncelleme" sayfası olabilir; yine aynı `tm-page-header` kullanılmalı.
- `QRManagement`: export/print odaklı özel detaydır; ayrı kalabilir ama aynı başlık ve geri standardını kullanmalı.
- `SystemLogs`: read-only özel detaydır; ayrı kalabilir ama aynı başlık ve geri standardını kullanmalı.

Liste sayfalarından yönlendirme:

- Detay sayfası update ile değiştirildiyse satır tıklaması `Update`/`Edit` aksiyonuna gitmeli.
- `returnUrl` her zaman taşınmalı.
- `Details` link metni yerine `Düzenle` veya ikon-only edit kullanılmalı.

## Metin Standardı

Başlıklar:

- `{Varlık} Düzenle`
- `{Varlık} Ekle`
- `{Varlık} Yönetimi`
- `Genel Bilgiler`
- `Görünürlük Ayarları`
- `Adres Bilgisi`
- `Medya`

Butonlar:

- `Geri Dön`
- `Kaydet`
- `Değişiklikleri Kaydet`
- `Vazgeç`
- `Sil`
- `Evet, Sil`
- `Önizle`
- `Menüyü Gör`
- `PDF İndir`

Kullanma:

- `Geri`
- `İptal` geri navigasyonu için
- `Güncelle` tek başına submit metni olarak
- Tamamen büyük harf badge veya başlık

## Kod İnceleme Kontrol Listesi

Admin UI değişikliği tamamlanmadan önce kontrol et:

- Geri tuşu ilk kartta ve başlığın solunda mı?
- `returnUrl` korunuyor mu?
- Ana yüzeylerde `horizon-card` kullanılmış mı?
- Tekrarlayan inline style var mı?
- Sayfada tek `h1` var mı?
- Submit metni create için `Kaydet`, update için `Değişiklikleri Kaydet` mi?
- Silme aksiyonu SweetAlert/TumMenuAlerts ile onay alıyor mu?
- Mobilde başlık ve butonlar alt alta düzgün akıyor mu?
- Dark mode için `--horizon-*` değişkenleri kullanılmış mı?
- Kart içinde kart kullanılmadı mı?

