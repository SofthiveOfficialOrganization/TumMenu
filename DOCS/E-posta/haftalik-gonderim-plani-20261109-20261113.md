# Hafta İçi E-posta Gönderim Planı (09.11.2026 - 13.11.2026)

Bu doküman, TümMenü QR menü tanıtımı için 9 Kasım 2026 Pazartesi gününden 13 Kasım 2026 Cuma gününe kadar (5 iş günü) uygulanacak e-posta tanıtım operasyonunu tanımlar.

---

## 1. Operasyonel Strateji ve Gönderim Metodu

* **Zamanlama Stratejisi:** Yeme-içme sektörü (restoran, kafe, bistro) hafta sonları yoğun servis trafiğinde olduğundan, e-postalar yalnızca işletme yönetiminin e-postaları incelediği ve operasyonel kararlar aldığı **hafta içi iş günlerinde (Pazartesi – Cuma)** iletilecektir.
* **Geçmiş Çakışma Denetimi:** Sistemdeki önceki tüm 310 geçmiş e-posta adresi filtrelenmiştir. **Çakışma sayısı: 0**.
* **Yeni Dosyalar Arası Tekrar Denetimi:** 5 günlük 105 aday arasında da hiçbir tekrar yoktur. **Yinelenme sayısı: 0**.
* **Günlük Hacim:** Günde **21 işletme** (5 gün x 21 = **105 işletme**).
* **Kullanılacak Şablon:** `DOCS/E-posta/isletmeler-icin-tanitim-e-postasi.md`
  * **Konu:** *Menü ve fiyat güncellemelerini QR menüyle kolaylaştırın*
* **Gönderim Motoru / Metodu:** `Tools/EmailCampaignSender`
  * SMTP üzerinden TLS/SSL ile güvenli gönderim (`destek@tummenu.com`)
  * Her gönderim arasında otomatik 2 saniye bekleme süresi
  * Gönderilen iletinin IMAP *Sent* (Gönderilmiş Öğeler) klasörüne otomatik arşivlenmesi
  * RFC uyumlu `List-Unsubscribe` ve `X-TumMenu-Campaign-Archive-Id` başlıkları
  * Gönderim durumunun (Queued / Sending / Sent / Failed) anlık olarak sonuç CSV dosyasına kaydedilmesi
* **Otomasyon (Antigravity Scheduled Automation):** Hafta içi her gün sabah belirlenen saatte (örn. 10:00) otomatik tetiklenerek o günün CSV dosyasını gönderir ve sonuçları kaydeder.

---

## 2. Tarih Bazlı Günlük Dağılım ve Takvim

| Tarih | Gün | Aday CSV Dosyası | Aday Sayısı | Odak Bölgeler / Konsept | Durum |
| :--- | :--- | :--- | :---: | :--- | :--- |
| **09.11.2026** | Pazartesi | `kampanya-20261109-pazartesi.csv` | 21 | Ankara, İzmir, İstanbul, Eskişehir, Trabzon, Samsun (Kafe, Kahve & Balık) | Tamamlandı (21/21 Sent) |
| **10.11.2026** | Salı | `kampanya-20261110-sali.csv` | 21 | Adana, Gaziantep, İstanbul, İzmir, Afyon, Trabzon, Antalya (Kebap, Burger, Bistro & Steak) | Tamamlandı (21/21 Sent) |
| **11.11.2026** | Çarşamba | `kampanya-20261111-carsamba.csv` | 21 | Antalya, Eskişehir, Balıkesir/Cunda, Muğla/Akyaka, Mersin, Kayseri, Denizli (Restoran & Kahvaltı) | Tamamlandı (21/21 Sent) |
| **12.11.2026** | Perşembe | `kampanya-20261112-persembe.csv` | 21 | Sakarya/Sapanca, Kocaeli, Tekirdağ, Aydın/Kuşadası, İstanbul (Kahvaltı, Artisan Kruvasan & Butik Roastery) | Beklemede |
| **13.11.2026** | Cuma | `kampanya-20261113-cuma.csv` | 21 | İzmir/Urla, Bodrum, İstanbul (Fine Dining, Şef Restoranları), Bursa, Gaziantep, Hatay, Konya, Antalya | Beklemede |

---

## 3. Tarih Bazlı Çalıştırma Komutları Referansı

```bash
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
