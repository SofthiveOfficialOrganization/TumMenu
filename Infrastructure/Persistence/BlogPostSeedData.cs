using Domain.Entities;

namespace Infrastructure.Persistence;

public static class BlogPostSeedData
{
    public static List<BlogPost> GetPosts() => new()
    {
        new BlogPost
        {
            Id = Guid.Parse("a1000000-0000-0000-0000-000000000001"),
            Title = "QR Menü Nedir? Nasıl Çalışır?",
            Slug = "qr-menu-nedir-nasil-calisir",
            Summary = "QR menü, restoranların müşterilerine kağıt menü yerine akıllı telefon ile erişilebilen dijital menü sunmasını sağlayan teknolojidir.",
            Content = """
<h2>QR Menü Nedir?</h2>
<p>QR menü (Quick Response Menü), restoran ve kafelerin masa başlarına koydukları küçük bir QR kod aracılığıyla müşterilerin akıllı telefonlarından kolayca erişebildiği dijital menü sistemidir. Müşteri, telefon kamerasıyla kodu okuttuğunda tarayıcısında menü anında açılır; herhangi bir uygulama indirmesine gerek yoktur.</p>
<h2>Nasıl Çalışır?</h2>
<p>Süreç oldukça basittir:</p>
<ol>
<li>Restoran sahibi dijital menüsünü oluşturur ve yönetim panelinden günceller.</li>
<li>Sistem, menüye özgü benzersiz bir QR kod üretir.</li>
<li>Bu kod masalara, pencerlere veya kartlara bastırılır.</li>
<li>Müşteri kodu okuttuğunda güncel menüyü görür.</li>
</ol>
<h2>QR Menünün Avantajları</h2>
<ul>
<li><strong>Anlık güncelleme:</strong> Fiyat veya ürün değişikliğini saniyeler içinde yayınlayabilirsiniz.</li>
<li><strong>Hijyen:</strong> Elleri değen kağıt menü yerine herkes kendi telefonu üzerinden erişir.</li>
<li><strong>Maliyet:</strong> Kağıt baskı masrafı ortadan kalkar.</li>
<li><strong>Analitik:</strong> Hangi ürünlerin daha çok görüntülendiğini takip edebilirsiniz.</li>
</ul>
<h2>Kimler Kullanabilir?</h2>
<p>Küçük bir kafe ya da büyük bir restoran zinciri fark etmeksizin her ölçekteki işletme QR menü sisteminden yararlanabilir. Özellikle hızlı güncelleme ihtiyacı duyan, sezonluk menü değişikliği yapan veya hijyen standartlarına önem veren işletmeler için idealdir.</p>
""",
            IsPublished = true,
            PublishedAt = new DateTime(2026, 3, 1),
            Tags = "qr-menu,dijital-menu,restoran"
        },
        new BlogPost
        {
            Id = Guid.Parse("a1000000-0000-0000-0000-000000000002"),
            Title = "Restoranlar Neden Dijital Menüye Geçmeli?",
            Slug = "restoranlar-neden-dijital-menuye-gecmeli",
            Summary = "Dijital menü, maliyet tasarrufu, anlık güncelleme ve daha iyi müşteri deneyimi sunarak restoran işletmeciliğini kolaylaştırıyor.",
            Content = """
<h2>Değişen Müşteri Beklentileri</h2>
<p>Günümüz müşterileri hız, temizlik ve kolaylık bekliyor. Akıllı telefon kullanımının yaygınlaşmasıyla birlikte dijital menüler artık bir tercih değil, rekabetçi bir zorunluluk haline geliyor.</p>
<h2>Maliyet Tasarrufu</h2>
<p>Kağıt menü bastırmak düşünüldüğünden daha pahalıdır. Tasarım, baskı, laminasyon ve sık güncelleme maliyetleri yıllık binlerce liraya ulaşabilir. Dijital menüye geçen bir restoran bu masrafı tamamen ortadan kaldırır.</p>
<h2>Anlık Güncelleme Özgürlüğü</h2>
<p>Bir ürün bitti mi? Fiyat değişti mi? Yeni sezonluk bir yemek eklemek mi istiyorsunuz? Dijital menüde bu değişiklikleri saniyeler içinde yayınlayabilir, tüm masalara aynı anda yansıtabilirsiniz.</p>
<h2>Daha İyi Analitik</h2>
<p>Hangi ürünler en çok görüntüleniyor? Müşteriler menüde ne kadar vakit geçiriyor? Bu verilere erişmek işletmenizi daha akıllıca yönetmenizi sağlar.</p>
<h2>Çevre Dostu</h2>
<p>Kağıt tüketimini azaltmak hem çevreye duyarlı bir tercih hem de modern müşterilere karşı olumlu bir marka imajı yaratır.</p>
""",
            IsPublished = true,
            PublishedAt = new DateTime(2026, 3, 5),
            Tags = "dijital-menu,restoran,maliyet"
        },
        new BlogPost
        {
            Id = Guid.Parse("a1000000-0000-0000-0000-000000000003"),
            Title = "Restoran Menüsü Nasıl Tasarlanır?",
            Slug = "restoran-menusu-nasil-tasarlanir",
            Summary = "Etkili bir restoran menüsü tasarlamak, hem satışları artırır hem de müşteri deneyimini iyileştirir. İşte dikkat etmeniz gereken temel noktalar.",
            Content = """
<h2>Menü Tasarımının Önemi</h2>
<p>Menü, restoranınızın sessiz satış elemanıdır. İyi tasarlanmış bir menü, müşterilerin doğru ürünlere yönlenmesini sağlar, ortalama sipariş tutarını artırır ve marka kimliğini yansıtır.</p>
<h2>Kategorileri Net Ayırın</h2>
<p>Başlangıçlar, ana yemekler, tatlılar, içecekler gibi kategorilerin net biçimde ayrılması müşterinin menüde kaybolmasını engeller. Dijital menülerde bu kategori yapısı hem gezinmeyi kolaylaştırır hem de ürünleri daha hızlı buldurmayı sağlar.</p>
<h2>Fotoğraf Kullanımı</h2>
<p>Araştırmalar, fotoğraflı ürünlerin fotoğrafsız olanlara göre %30 daha fazla sipariş aldığını gösteriyor. Kaliteli, iştah açıcı fotoğraflar yatırımın geri dönüşünü hızlandırır.</p>
<h2>Açıklamalar Kısa ve Cazip Olsun</h2>
<p>Her ürün için 1-2 cümlelik bir açıklama yeterlidir. Malzemeleri, pişirme yöntemini veya ürünün özelliğini vurgulayan kısa bir metin müşteriyi ikna eder.</p>
<h2>Fiyatlandırma Psikolojisi</h2>
<p>Fiyatları "TL" sembolü olmadan yazmak, müşterinin harcama kaygısını azaltır. Yuvarlak rakamlar yerine 89,90 gibi fiyatlar daha düşük algılanır.</p>
<h2>Düzenli Güncelleme</h2>
<p>Sezonluk değişiklikler, popüler yeni ürünler ve biten stokları menüden zamanında kaldırmak marka güvenilirliğini korur.</p>
""",
            IsPublished = true,
            PublishedAt = new DateTime(2026, 3, 10),
            Tags = "menu-tasarimi,restoran,ipuclari"
        },
        new BlogPost
        {
            Id = Guid.Parse("a1000000-0000-0000-0000-000000000004"),
            Title = "Müşteri Sadakati Nasıl Sağlanır?",
            Slug = "musteri-sadakati-nasil-saglanir",
            Summary = "Restoran işletmeciliğinde yeni müşteri kazanmak, mevcut müşteriyi elde tutmaktan beş kat daha pahalıdır. Sadakat oluşturmanın yollarını keşfedin.",
            Content = """
<h2>Neden Sadakat Bu Kadar Önemli?</h2>
<p>Bir restorana düzenli gelen müşteri, yeni müşteriye kıyasla ortalama %67 daha fazla harcama yapar. Üstelik memnun sadık müşteriler organik bir pazarlama gücüdür; tavsiye ederler, sosyal medyada paylaşırlar.</p>
<h2>Tutarlı Kalite</h2>
<p>Müşterinin aynı lezzeti her gelişinde bulmasını beklemesi doğaldır. Standart tarifler, porsiyon tutarlılığı ve aynı kalite ürün kullanımı sadakatin temelini oluşturur.</p>
<h2>Kişiselleştirilmiş Deneyim</h2>
<p>Müşterinin adını hatırlamak, sık sipariş ettiği ürünü önermek, doğum gününde küçük bir jest yapmak; bunlar büyük fark yaratır.</p>
<h2>Geri Bildirimi Ciddiye Alın</h2>
<p>Olumsuz bir yorum birer fırsat olarak görülmelidir. Hızlı ve yapıcı yanıt veren restoranlar, müşteriyi kaybetmek yerine onları en sadık savunuculara dönüştürür.</p>
<h2>Sadakat Programları</h2>
<p>Basit bir damgalı kart bile işe yarar. "10 kahve al, 1 bedava kazan" modeli müşteriyi geri gelmeye teşvik eder. Dijital sadakat programları ise veri toplamanıza da olanak sağlar.</p>
""",
            IsPublished = true,
            PublishedAt = new DateTime(2026, 3, 15),
            Tags = "musteri-sadakati,restoran,ipuclari"
        },
        new BlogPost
        {
            Id = Guid.Parse("a1000000-0000-0000-0000-000000000005"),
            Title = "Türk Mutfağının Vazgeçilmez Lezzetleri",
            Slug = "turk-mutfaginin-vazgecilmez-lezzetleri",
            Summary = "Kebaptan mezeden tatlıya kadar Türk mutfağının ikonik lezzetleri ve bunların menülerde nasıl sunulabileceğine dair ipuçları.",
            Content = """
<h2>Dünyanın En Zengin Mutfaklarından Biri</h2>
<p>Türk mutfağı, Orta Asya, Orta Doğu ve Akdeniz mutfaklarının sentezinden doğmuş, yüzyıllar içinde olgunlaşmış köklü bir gastronomi geleneğini temsil eder. Çeşitliliği ve zenginliği ile dünya mutfakları arasında üst sıralarda yer alır.</p>
<h2>Vazgeçilmez Ana Yemekler</h2>
<ul>
<li><strong>Adana Kebap:</strong> Kıyma, kuyruk yağı ve baharattan yapılan bu kebap, mangalda pişirilen Türk mutfağının sembolüdür.</li>
<li><strong>İskender Kebap:</strong> Döner et, tereyağı, domates sosu ve yoğurtla servis edilen Bursa'nın gururu.</li>
<li><strong>Mantı:</strong> İnce hamurla sarılmış küçük et doldurmalı mantılar, üzerine dökülen sarımsaklı yoğurt ve tereyağıyla sunulur.</li>
<li><strong>İmam Bayıldı:</strong> Zeytinyağında soğan, sarımsak ve domatesle hazırlanan patlıcan yemeği.</li>
</ul>
<h2>Mezeler</h2>
<p>Türk mutfağında meze kültürü başlı başına bir ritüeldir. Humus, cacık, haydari, sigara böreği, midye dolma gibi onlarca çeşit, sofrayı paylaşma geleneğini yaşatır.</p>
<h2>Tatlılar</h2>
<p>Baklava, künefe, sütlaç, kazandibi ve lokum Türk tatlı geleneğinin öncü isimleridir. Her birinin bölgesel varyasyonları ayrı bir zenginlik oluşturur.</p>
<h2>Menünüzde Türk Mutfağını Nasıl Sunarsınız?</h2>
<p>Ürün açıklamalarında yöresel köken ve hikayeye yer vermek, müşteride merak ve değer algısı oluşturur. "Urfa biberinin özgün acısıyla hazırlanan" gibi ifadeler lezzeti somutlaştırır.</p>
""",
            IsPublished = true,
            PublishedAt = new DateTime(2026, 3, 20),
            Tags = "turk-mutfagi,yemek,lezzetler"
        }
    };
}
