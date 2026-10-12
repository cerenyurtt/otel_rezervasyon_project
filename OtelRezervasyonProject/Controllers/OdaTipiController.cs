using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OtelRezervasyonProject.Data;
using OtelRezervasyonProject.Models;

namespace OtelRezervasyonProject.Controllers
{
    public class OdaTipiController : Controller
    {
        private readonly OtelDbContext _context;

        public OdaTipiController(OtelDbContext context)
        {
            _context = context;
        }

        // Listeleme: Veritabanındaki oda tiplerini getirir
        public async Task<IActionResult> Index()
        {
            var tipler = await _context.OdaTipleri.ToListAsync();
            return View(tipler);
        }

        // Ekleme Sayfası: Yeni veri giriş formunu açar
        public IActionResult Create()
        {
            return View();
        }

        // Kaydetme İşlemi: Formdan gelen veriyi veritabanına yazar
        [HttpPost]
        [ValidateAntiForgeryToken] // Güvenlik için eklendi
        public async Task<IActionResult> Create(OdaTipi odaTipi)
        {
            // HOCANIN ŞARTI: Controlbase yapısında loglama
            Console.WriteLine($"[LOG - {DateTime.Now}]: Yeni Oda Tipi ekleme isteği geldi: {odaTipi.Ad}");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(odaTipi);
                    await _context.SaveChangesAsync();

                    // Başarılı kayıt logu
                    Console.WriteLine($"[LOG - {DateTime.Now}]: '{odaTipi.Ad}' başarıyla veritabanına kaydedildi.");

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // Veritabanı hatası oluşursa burası çalışır
                    Console.WriteLine($"[HATA - {DateTime.Now}]: Veritabanı kaydedilirken hata oluştu: {ex.Message}");
                    ModelState.AddModelError("", "Veritabanı hatası: " + ex.Message);
                }
            }
            else
            {
                // Formda eksik veya hatalı bilgi varsa logla
                Console.WriteLine($"[LOG - {DateTime.Now}]: Form verileri doğrulanamadı (ModelState Invalid).");
            }

            return View(odaTipi);
        }

        // --- SİLME İŞLEMİ ---
        public async Task<IActionResult> Delete(int id)
        {
            var odaTipi = await _context.OdaTipleri.FindAsync(id);
            if (odaTipi == null) return NotFound();

            // KRİTİK KONTROL: Bu tipe ait kayıtlı oda var mı?
            var odaVarMi = await _context.Odalar.AnyAsync(o => o.OdaTipiId == id);
            if (odaVarMi)
            {
                TempData["HataMesaj"] = $"'{odaTipi.Ad}' tipine ait kayıtlı odalar olduğu için bu tipi silemezsiniz!";
                return RedirectToAction(nameof(Index));
            }

            // --- VERİTABANI LOGLAMA ---
            _context.Loglar.Add(new Log
            {
                KullaniciAd = HttpContext.Session.GetString("UserName") ?? "Admin",
                Islem = "Oda Tipi Silme",
                Detay = $"'{odaTipi.Ad}' isimli oda tipi sistemden kaldırıldı.",
                Tarih = DateTime.Now
            });

            _context.OdaTipleri.Remove(odaTipi);
            await _context.SaveChangesAsync();

            TempData["RezervasyonMesaj"] = "Oda tipi başarıyla silindi.";
            return RedirectToAction(nameof(Index));
        }
    }
}