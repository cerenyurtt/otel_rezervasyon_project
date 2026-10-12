# Otel Rezervasyon Sistemi

Müşterilerin tarih aralığına göre oda arayıp rezervasyon yapabildiği, yöneticilerin ise oda ve oda tiplerini yönetebildiği bir web uygulaması. Üniversite dersi kapsamında geliştirilmiştir ve **geliştirme aşamasındadır**.

## Teknolojiler

- C# / ASP.NET Core MVC (.NET 8)
- Razor View'lar
- Entity Framework Core 8 (Code First, Migrations)
- Microsoft SQL Server

## Özellikler

- **Kayıt ve giriş:** Müşteri kaydı, müşteri girişi ve ayrı yönetici giriş sayfası (`/Account/AdminLogin`)
- **Oda arama:** Giriş/çıkış tarihi, fiyat aralığı ve oda tipine göre filtreleme. Seçilen tarihlerde rezerve edilmiş odalar, tarih aralığı çakışma kontrolüyle sonuçlardan çıkarılır.
- **Rezervasyon:** Seçilen oda için rezervasyon oluşturma ve gece sayısına göre toplam tutar hesabı
- **Oda yönetimi:** Oda ekleme, düzenleme, silme ve listeleme (aynı oda numarası tekrar eklenemez)
- **Oda tipi yönetimi:** Oda tipi ekleme, listeleme ve silme (odası olan tip silinemez)
- **İşlem kayıtları (log):** Önemli işlemler veritabanına ve konsola kaydedilir, loglar bir sayfada listelenir
- **REST API:** Odaları JSON olarak döndüren uç nokta (`GET /api/OdaApi`)

## Veri Modeli

| Tablo | Açıklama |
|---|---|
| `Oda` | Oda numarası, gecelik fiyat, müsaitlik, oda tipi |
| `OdaTipi` | Tip adı ve açıklaması (bir tipte birçok oda olabilir) |
| `Kullanici` | Ad, soyad, e-posta, şifre ve rol |
| `Rol` | Admin veya müşteri |
| `Rezervasyon` | Oda, kullanıcı, başlangıç/bitiş tarihi, toplam tutar |
| `Log` | Kullanıcı, işlem, detay, tarih |

## Kurulum ve Çalıştırma

**Gereksinimler:** Visual Studio 2022, .NET 8 SDK, SQL Server (veya SQL Server Express)

1. Depoyu klonlayın:
   ```
   git clone https://github.com/<kullanici-adiniz>/OtelRezervasyonProject.git
   ```
2. `OtelRezervasyonProject.sln` dosyasını Visual Studio ile açın.
3. `appsettings.json` içindeki `DefaultConnection` değerinde `Server=` kısmını kendi SQL Server adınızla değiştirin (örn. `localhost\SQLEXPRESS`).
4. Package Manager Console'da veritabanını oluşturun:
   ```
   Update-Database
   ```
5. Veritabanında `Roller` tablosuna iki kayıt ekleyin: `Id = 1` (`Admin`) ve `Id = 2` (`Musteri`). Tablolar otomatik doldurulmaz.
6. Projeyi çalıştırın (F5). Müşteri hesabı için kayıt sayfasını kullanın. Yönetici hesabı için `Kullanicilar` tablosuna `RolId = 1` ile bir kayıt ekleyin.

## Bilinen Eksikler ve Geliştirme Planı

- Yetkilendirme henüz tamamlanmadı; sayfa erişimleri rol bazında kısıtlanacak
- Şifreler şu an düz metin saklanıyor; hash'lenmesi planlanıyor
- Rezervasyon anında tarih çakışması tekrar kontrol edilecek
- Oda doluluğu yalnızca rezervasyon tarihlerinden hesaplanacak
- E-posta adresi için veritabanında benzersizlik kısıtı eklenecek
