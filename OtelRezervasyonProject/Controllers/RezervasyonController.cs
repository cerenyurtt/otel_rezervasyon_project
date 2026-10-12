using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OtelRezervasyonProject.Data;
using OtelRezervasyonProject.Models;

namespace OtelRezervasyonProject.Controllers
{
    public class RezervasyonController : Controller
    {
        private readonly OtelDbContext _context;
        public RezervasyonController(OtelDbContext context) { _context = context; }

        // 1. REZERVASYON LİSTESİ (Rol Bazlı Filtreleme)
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var rolId = HttpContext.Session.GetInt32("UserRolId");

            if (userId == null) return RedirectToAction("Login", "Account");

            var sorgu = _context.Rezervasyonlar
                .Include(r => r.Oda)
                    .ThenInclude(o => o.OdaTipi)
                .Include(r => r.Kullanici)
                .AsQueryable();

            if (rolId != 1)
            {
                sorgu = sorgu.Where(r => r.KullaniciId == userId);
                Console.WriteLine($"[LOG - {DateTime.Now}]: Kullanıcı {userId} kendi kayıtlarını görüntüledi.");
            }
            else
            {
                Console.WriteLine($"[LOG - {DateTime.Now}]: Admin tüm rezervasyon listesine erişti.");
            }

            var listem = await sorgu.ToListAsync();
            return View(listem);
        }

        // 2. REZERVE ET İŞLEMİ (Loglama Sistemi Eklenmiş Hali)
        public async Task<IActionResult> RezerveEt(int id, DateTime giris, DateTime cikis)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userName = HttpContext.Session.GetString("UserName"); // Log için kullanıcı adı

            if (userId == null) return RedirectToAction("Login", "Account");

            var oda = await _context.Odalar.FindAsync(id);
            if (oda == null) return NotFound();

            // Tarih kontrolü
            if (giris == default) giris = DateTime.Now;
            if (cikis == default) cikis = DateTime.Now.AddDays(1);

            // GÜN SAYISI VE TUTAR HESAPLAMA
            int toplamGun = (cikis.Date - giris.Date).Days;
            if (toplamGun <= 0) toplamGun = 1;

            decimal hesaplananTutar = oda.GecelikFiyat * toplamGun;

            // Odayı rezerve edildiği için "Dolu" yapıyoruz
            oda.MusaitMi = false;

            // Rezervasyon kaydı oluştur
            var yeniRez = new Rezervasyon
            {
                OdaId = id,
                KullaniciId = userId.Value,
                BaslangicTarihi = giris,
                BitisTarihi = cikis,
                ToplamTutar = hesaplananTutar
            };

            // --- YENİ EKLEME: VERİTABANI LOGLAMA ---
            var yeniLog = new Log
            {
                KullaniciAd = userName ?? "Bilinmiyor",
                Islem = "Yeni Rezervasyon",
                Detay = $"Oda #{oda.OdaNo} için {giris.ToShortDateString()}-{cikis.ToShortDateString()} arası {hesaplananTutar:N0} TL tutarında rezervasyon yapıldı.",
                Tarih = DateTime.Now
            };
            _context.Loglar.Add(yeniLog);
           

            _context.Rezervasyonlar.Add(yeniRez);
            _context.Update(oda);

            await _context.SaveChangesAsync();

            // BİLGİLENDİRME MESAJI
            TempData["RezervasyonMesaj"] = $"Oda #{oda.OdaNo} için {toplamGun} gecelik rezervasyonunuz ({hesaplananTutar:N0} TL) başarıyla tamamlanmıştır.";

            // HOCANIN ŞARTI: Konsol Loglama
            Console.WriteLine($"[LOG - {DateTime.Now}]: Kullanıcı {userId}, Oda {oda.OdaNo} için rezervasyon yaptı.");

            return RedirectToAction(nameof(Index));
        }
    }
}