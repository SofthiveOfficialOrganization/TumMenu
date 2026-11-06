# 7 Günlük E-posta Gönderim Planı (07.11.2026 - 13.11.2026)

Bu doküman, TümMenü QR menü tanıtımı için 7 Kasım 2026 Cumartesi gününden 13 Kasım 2026 Cuma gününe kadar uygulanacak haftalık e-posta tanıtım ve gönderim operasyonunu tanımlar.

---

## 1. Operasyonel Özet ve Gönderim Metodu

* **Dosya Oluşumu:** Bu aday listeleri ve CSV dosyaları doğrudan bu çalışma kapsamında araştırılıp derlenerek oluşturulmuştur.
* **Geçmiş Çakışma Denetimi:** Klasördeki önceki tüm 310 geçmiş e-posta adresi taranmış ve filtrelenmiştir. **Çakışma sayısı: 0**.
* **Yeni Dosyalar Arası Tekrar Denetimi:** 7 günlük 105 aday arasında da hiçbir tekrar yoktur. **Yinelenme sayısı: 0**.
* **Günlük Hacim:** Günde **15 işletme** (Toplam 7 gün x 15 = **105 işletme**).
* **Kullanılacak Şablon:** `DOCS/E-posta/isletmeler-icin-tanitim-e-postasi.md`
  * **Konu:** *Menü ve fiyat güncellemelerini QR menüyle kolaylaştırın*
* **Gönderim Motoru / Metodu:** `Tools/EmailCampaignSender`
  * SMTP üzerinden TLS/SSL ile güvenli gönderim (`destek@tummenu.com`)
  * Her gönderim arasında otomatik 2 saniye bekleme süresi
  * Gönderilen iletinin IMAP *Sent* (Gönderilmiş Öğeler) klasörüne otomatik arşivlenmesi
  * RFC uyumlu `List-Unsubscribe` ve `X-TumMenu-Campaign-Archive-Id` başlıkları
  * Gönderim durumunun (Queued / Sending / Sent / Failed) anlık olarak sonuç CSV dosyasına kaydedilmesi
* **Çalıştırma Akışı:** İlgili gün geldiğinde bana tarih veya gün adını söylemeniz yeterlidir; ilgili dosya gönderim motoruna verilerek çalıştırılır ve sonuç raporu anında ekrana sunulur.

---

## 2. Tarih Bazlı Günlük Dağılım ve Takvim

| Tarih | Gün | Aday CSV Dosyası | Aday Sayısı | Odak Şehirler / Konsept | Durum |
| :--- | :--- | :--- | :---: | :--- | :--- |
| **07.11.2026** | Cumartesi | `kampanya-20261107-cumartesi.csv` | 15 | Ankara, İzmir, İstanbul, Eskişehir, Trabzon, Samsun (Kafe & Kahve) | Beklemede |
| **08.11.2026** | Pazar | `kampanya-20261108-pazar.csv` | 15 | İstanbul, İzmir, Adana, Gaziantep, Afyon (Kahve, Balık & Kebap) | Beklemede |
| **09.11.2026** | Pazartesi | `kampanya-20261109-pazartesi.csv` | 15 | Antalya/Kaleiçi, Alanya, Balıkesir/Cunda, Eskişehir, Ankara (Bistro & Steak) | Beklemede |
| **10.11.2026** | Salı | `kampanya-20261110-sali.csv` | 15 | Balıkesir, Muğla/Akyaka, Mersin, Kayseri, Denizli, Sakarya, Kocaeli, Tekirdağ, Aydın | Beklemede |
| **11.11.2026** | Çarşamba | `kampanya-20261111-carsamba.csv` | 15 | Aydın, İzmir, İstanbul (Kahvaltı, Artisan Kruvasan & Butik Kahve, Urla) | Beklemede |
| **12.11.2026** | Perşembe | `kampanya-20261112-persembe.csv` | 15 | İzmir/Urla, Bodrum, İstanbul (Fine Dining, Şef & Boğaz Restoranları), Ankara | Beklemede |
| **13.11.2026** | Cuma | `kampanya-20261113-cuma.csv` | 15 | Bursa, Trabzon/Akçaabat, Gaziantep, Adana, Hatay, Konya, Antalya, Ankara, İzmir | Beklemede |

---

## 3. Tarih Bazlı Çalıştırma Komutları Referansı

```bash
# 07.11.2026 Cumartesi
dotnet run --project Tools/EmailCampaignSender -- "DOCS/E-posta/kampanya-20261107-cumartesi.csv" "DOCS/E-posta/sonuc-kampanya-20261107-cumartesi.csv" "DOCS/E-posta/isletmeler-icin-tanitim-e-postasi.md" "Özel yeme-içme işletmesi"

# 08.11.2026 Pazar
dotnet run --project Tools/EmailCampaignSender -- "DOCS/E-posta/kampanya-20261108-pazar.csv" "DOCS/E-posta/sonuc-kampanya-20261108-pazar.csv" "DOCS/E-posta/isletmeler-icin-tanitim-e-postasi.md" "Özel yeme-içme işletmesi"

# 09.11.2026 Pazartesi
dotnet run --project Tools/EmailCampaignSender -- "DOCS/E-posta/kampanya-20261109-pazartesi.csv" "DOCS/E-posta/sonuc-kampanya-20261109-pazartesi.csv" "DOCS/E-posta/isletmeler-icin-tanitim-e-postasi.md" "Özel yeme-içme işletmesi"

# 10.11.2026 Salı
dotnet run --project Tools/EmailCampaignSender -- "DOCS/E-posta/kampanya-20261110-sali.csv" "DOCS/E-posta/sonuc-kampanya-20261110-sali.csv" "DOCS/E-posta/isletmeler-icin-tanitim-e-postasi.md" "Özel yeme-içme işletmesi"

# 11.11.2026 Çarşamba
dotnet run --project Tools/EmailCampaignSender -- "DOCS/E-posta/kampanya-20261111-carsamba.csv" "DOCS/E-posta/sonuc-kampanya-20261111-carsamba.csv" "DOCS/E-posta/isletmeler-icin-tanitim-e-postasi.md" "Özel yeme-içme işletmesi"

# 12.11.2026 Perşembe
dotnet run --project Tools/EmailCampaignSender -- "DOCS/E-posta/kampanya-20261112-persembe.csv" "DOCS/E-posta/sonuc-kampanya-20261112-persembe.csv" "DOCS/E-posta/isletmeler-icin-tanitim-e-postasi.md" "Özel yeme-içme işletmesi"

# 13.11.2026 Cuma
dotnet run --project Tools/EmailCampaignSender -- "DOCS/E-posta/kampanya-20261113-cuma.csv" "DOCS/E-posta/sonuc-kampanya-20261113-cuma.csv" "DOCS/E-posta/isletmeler-icin-tanitim-e-postasi.md" "Özel yeme-içme işletmesi"
```
