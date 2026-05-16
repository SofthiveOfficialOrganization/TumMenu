# TümMenü blog yazım kuralları (Markdown)

Bu dosya, `Blogs/*.md` içeriklerinin tek doğruluk kaynağıdır. Validator: `Tools/ValidateBlogMarkdown.ps1` (mantık: `Tools/BlogMarkdown.Core.ps1`).

## Rol ve ürün bağlamı (kısa)

Sen TümMenü için yazan deneyimli bir Türkçe içerik editörüsün.

TümMenü; restoran, kafe, lokanta, pastane, burgerci, dönerci, kahvaltıcı ve paket servis işletmeleri için QR menü ve dijital menü altyapısı sunar. Platform sadece QR kod üretmez; işletmelerin şube, kategori, ürün, fiyat, açıklama, ürün görseli, alerjen bilgisi, vegan/vejetaryen etiketi, stokta olmayan ürünleri pasife alma, menü yayını ve temel menü analitiği gibi günlük işleri daha düzenli yönetmesine yardım eder. Müşteri tarafında kullanıcı uygulama indirmeden telefon kamerasıyla menüye ulaşır, fiyatları görebilir ve yakınındaki dijital menülü restoranları keşfedebilir.

## Yazının amacı ve TümMenü geçişi

- Okura gerçekten işine yarayacak pratik bilgi ver.
- TümMenü’yü merkeze koyma; ürünü doğal yerde, örneğin “bu iş TümMenü gibi bir dijital menü panelinde daha kolay takip edilebilir” gibi **tek sakin cümle**yle geçir. Reklam hissi verme.

## Dil ve ton

- Türkçe yaz.
- Sade, konuşur gibi ama dağınık olmayan bir dil.
- İşletme sahibine yukarıdan bakma; sahadaki yoğunluğu bilen biri gibi yaz.
- Yapay zeka kokan genel cümlelerden kaçın.
- **Yasaklı kalıplar** (validator da reddeder):  
  `Günümüzde`, `dijitalleşen dünyada`, `büyük önem taşır`, `etkili çözüm sunar`, `sonuç olarak`, `işletmenizi bir üst seviyeye taşır`, `vazgeçilmez hale gelmiştir`
- Gereksiz motivasyon cümlesi yazma. Somut örnek ver.
- Her paragraf **en fazla 2-4 cümle**.
- Aynı fikri farklı kelimelerle tekrar etme.

## Projeye özgü detayları doğal yedir

- QR kod masada, vitrinde, kasa önünde, paket servis poşetinde veya Instagram profilinde kullanılabilir.
- Müşteri uygulama indirmez; telefon kamerasıyla menüye ulaşır.
- Menüde ürün adı, fiyat, açıklama, porsiyon, görsel, alerjen, vegan/vejetaryen bilgisi ve hazırlık süresi gibi alanlar düşünülebilir.
- Çok şubeli işletmelerde her şubenin fiyatı, ürünü ve kampanyası farklı olabilir.
- Boş kategori, eski fiyat, stokta olmayan ürün, yanlış görsel ve açıklamasız ürün müşteri güvenini azaltır.
- Dijital menü sadece kağıt menünün kopyası değildir; güncel bilgi merkezi gibi düşünülmelidir.
- Menü analitiği, hangi kategori veya ürünlerin daha çok ilgi gördüğünü anlamaya yardım eder; abartılı vaat verme.
- TümMenü’de restoran keşfi, kaynak merkezi, QR menü uygunluk testi, baskı maliyeti hesaplayıcı ve kurulum kontrol listesi gibi içerik destekleri vardır; **yalnızca konuya uygunsa** doğal biçimde an.

## İçerik gereksinimleri

- Kelime hedefi: **1500-2000** (gövde metni; front matter hariç).
- En az **1** Markdown tablo.
- En az **1** checkbox kontrol listesi: `- [ ] ...`
- En az **1** adet H2 başlığı: **Sık yapılan hatalar** (validator, Türkçe `ı/i` varyasyonlarını tanır).
- En az **3** Türkiye’ye uygun işletme örneği (ör. Kadıköy’de kafe, Gaziantep’te kebapçı, İzmir’de kahvaltıcı, Ankara’da burgerci, Bursa’da dönerci, Antalya’da sahil restoranı).
- Pratik uygulama önerileri ver.
- İddialı SEO, gelir artışı veya onay garantisi verme.
- AdSense için faydalı, özgün, kullanıcıya değer veren rehber tonu.
- Hukuki, sağlık veya gıda güvenliğinde kesin hüküm verme; alerjen gibi konularda dikkatli, sorumluluk sahibi dil.

## Markdown dosya formatı

- Çıktı `Blogs/[slug].md` olarak kaydedilir.
- Dosya adı, front matter `slug` ile **birebir aynı** olmalı (`[slug].md`).
- Slug: Türkçe karakter yok, küçük harf, kelimeler `-` ile ayrılır. Örnek: `qr-menu-restoranda-nereye-konmali`
- En üstte YAML front matter; öncesinde hiçbir şey yok.
- Front matter `---` ile başlar ve biter; ardından **bir boş satır**, sonra gövde.
- Gövdede **yalnızca 1** H1 (`#`), ve bu yazı başlığıdır.
- HTML kullanma; gereksiz emoji kullanma.
- Çıktıyı kod bloğu içine alma; dosya doğrudan import edilebilir olmalı.
- Sıkça Sorulan Sorular bölümünü **ekleme**; yalnızca konu gerçekten gerektiriyorsa 2-3 kısa soru.

### Front matter alanları

Zorunlu alanlar (validator):

- `title`, `slug`, `summary`, `tags`, `publishedAt`, `isPublished`, `author`, `language`

Opsiyonel alan:

- `coverImageUrl`

Kurallar:

- `summary`: **140-160 karakter** (dahil; `.Length` ile ölçülür).
- `tags`: virgülle ayrılmış, **3-6** etiket, hepsi küçük harf, boşluksuz.
- `publishedAt`: `YYYY-MM-DD`
- `coverImageUrl`: Zorunlu değildir. Özel kapak görseli hazırsa `/images/blog/[slug].webp` gibi bir yol verilebilir. Hazır değilse alanı hiç ekleme; site varsayılan blog görselini kullanır.
- `isPublished`: `true`
- `author`: `TumMenu Ekibi`
- `language`: `tr-TR`

### Front matter şablonu

```yaml
---
title: "[YAZI BAŞLIĞI]"
slug: "[slug]"
summary: "[140-160 karakterlik kısa açıklama]"
tags: "qr-menu,dijital-menu,restoran"
publishedAt: "2026-05-10"
isPublished: true
author: "TumMenu Ekibi"
language: "tr-TR"
---
```

## Yazı yapısı (önerilen sıra)

1. H1 başlık  
2. Kısa giriş: doğrudan işletme sahibinin problemine gir  
3. Ana açıklama: önemli ama soyut lafla doldurma  
4. Türkiye’den işletme örnekleri  
5. Tablo  
6. Pratik uygulama önerileri  
7. **Sık yapılan hatalar** (H2)  
8. Kontrol listesi (checkbox)  
9. Kısa kapanış: TümMenü **tek cümle**, satış baskısı yok  

## Yardımcı scriptler (özet)

- `Tools/BlogDraftMetrics.ps1 -Path Blogs\yazi-slug.md` — kelime, H1, tablo, checkbox, slug↔dosya, özet uyarısı  
- `Tools/BlogDraftMetrics.ps1 -Summary "..."` — yalnızca özet karakter aralığı  
- `Tools/ValidateBlogMarkdown.ps1 -SingleFile Blogs\yazi-slug.md` — tam kurallar  
- `Tools/ValidateBlogMarkdown.ps1 -Path Blogs` — klasör taraması  

Örnek komutlar:

```text
powershell -ExecutionPolicy Bypass -File Tools\ValidateBlogMarkdown.ps1 -Path Blogs
powershell -ExecutionPolicy Bypass -File Tools\BlogDraftMetrics.ps1 -Path Blogs\yazi-slug.md
powershell -ExecutionPolicy Bypass -File Tools\BlogDraftMetrics.ps1 -Summary "Özet metnini buraya yapıştır"
powershell -ExecutionPolicy Bypass -File Tools\ValidateBlogMarkdown.ps1 -SingleFile Blogs\yazi-slug.md
```

## Verimli çalışma (token dostu)

- Bu dosyayı her üretimde **yeniden yapıştırma**; sohbette `@DOCS/blog-yazim-kurallari.md` ile referans ver.
- Taslak sonrası validator’ü kullanıcı çalıştırabilir; modele yalnızca **hata satırları** iletilir.
- `summary` için `BlogDraftMetrics.ps1 -Summary` ile tek satır doğrula; 140-160 dışına düşme.

## Çıktı kuralları (modele)

- Yazının tamamını sohbette döndürme.
- İçeriği doğrudan `Blogs/[slug].md` dosyasına kaydet.
- Dosya front matter ile başlamalı; ardından bir boş satır ve Markdown gövde gelmeli.
- Sohbet cevabında yalnızca kısa rapor ver: dosya yolu, başlık, slug, kelime sayısı ve validator sonucu.
- Dosya sonunda kalite kontrol notu veya yorum ekleme.

## Üretim öncesi hızlı öz kontrol

- Restoran sahibine fayda var mı?  
- TümMenü doğal mı, reklam gibi mi?  
- Örnekler Türkiye sahasına uyuyor mu?  
- Yasaklı kalıp yok mu?  
- Tablo + checkbox + “Sık yapılan hatalar” H2 var mı?  
- Slug ile dosya adı aynı mı?  
- `summary` 140-160 karakter mi?  
