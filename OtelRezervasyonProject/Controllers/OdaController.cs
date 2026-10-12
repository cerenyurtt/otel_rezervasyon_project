using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OtelRezervasyonProject.Data;
using OtelRezervasyonProject.Models;

namespace OtelRezervasyonProject.Controllers
{
    public class OdaController : Controller
    {
        private readonly OtelDbContext _context;

        public OdaController(OtelDbContext context)
        {
            _context = context;
        }

        // 1. ŞART: En az 4 tablodan ilişkili kayıt içeren listeleme (Oda + OdaTipi)
        public async Task<IActionResult> Index()
        {
            var odalar = await _context.Odalar.Include(o => o.OdaTipi).ToListAsync();
            return View(odalar);
        }

        // --- YENİ EKLEME: LOG İZLEME SAYFASI (Sadece Admin Görebilir) ---
        public async Task<IActionResult> LogListesi()
        {
            // Güvenlik kontrolü: Sadece RolId'si 1 (Admin) olanlar görebilsin
            if (HttpContext.Session.GetInt32("UserRolId") != 1)
                return RedirectToAction("Login", "Account");

            var loglar = await _context.Loglar.OrderByDescending(x => x.Tarih).ToListAsync();
            return View(loglar);
        }

        // Yeni Oda Ekleme Sayfası (Görüntüleme)
        public IActionResult Create()
        {
            var odaTipleri = _context.OdaTipleri.ToList();
            ViewBag.OdaTipiId = new SelectList(odaTipleri, "Id", "Ad");
            return View();
        }

        // Yeni Oda Ekleme (Kaydetme)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Oda oda)
        {
            ModelState.Remove("OdaTipi");

            var odaVarMi = await _context.Odalar.AnyAsync(o => o.OdaNo == oda.OdaNo);
            if (odaVarMi)
            {
                ModelState.AddModelError("OdaNo", "Bu oda numarası zaten sisteme kayıtlı!");
            }

            if (ModelState.IsValid)
            {
                _context.Add(oda);

                // --- VERİTABANI LOGLAMA ---
                var log = new Log
                {
                    KullaniciAd = HttpContext.Session.GetString("UserName") ?? "Admin",
                    Islem = "Oda Ekleme",
                    Detay = $"{oda.OdaNo} numaralı yeni oda sisteme dahil edildi.",
                    Tarih = DateTime.Now
                };
                _context.Loglar.Add(log);

                await _context.SaveChangesAsync();
                Console.WriteLine($"[LOG - {DateTime.Now}]: {oda.OdaNo} numaralı yeni oda eklendi.");
                return RedirectToAction(nameof(Index));
            }

            ViewBag.OdaTipiId = new SelectList(_context.OdaTipleri, "Id", "Ad", oda.OdaTipiId);
            return View(oda);
        }

        // HOCANIN ŞARTI: REST API Sunumu (JSON Veri Dönüşü)
        [HttpGet]
        public async Task<JsonResult> ApiListesi()
        {
            var odalar = await _context.Odalar.Include(o => o.OdaTipi).ToListAsync();
            return Json(odalar);
        }

        // --- DÜZENLEME İŞLEMİ (Kaydetme) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Oda oda)
        {
            if (id != oda.Id) return NotFound();
            ModelState.Remove("OdaTipi");

            var odaVarMi = await _context.Odalar.AnyAsync(o => o.OdaNo == oda.OdaNo && o.Id != id);
            if (odaVarMi)
            {
                ModelState.AddModelError("OdaNo", "Bu oda numarası başka bir oda tarafından kullanılıyor!");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(oda);

                    // --- VERİTABANI LOGLAMA ---
                    _context.Loglar.Add(new Log
                    {
                        KullaniciAd = HttpContext.Session.GetString("UserName") ?? "Admin",
                        Islem = "Oda Güncelleme",
                        Detay = $"Oda #{oda.OdaNo} bilgileri başarıyla güncellendi.",
                        Tarih = DateTime.Now
                    });

                    await _context.SaveChangesAsync();
                    TempData["RezervasyonMesaj"] = $"Oda #{oda.OdaNo} başarıyla güncellendi.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Odalar.Any(e => e.Id == oda.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.OdaTipiId = new SelectList(_context.OdaTipleri, "Id", "Ad", oda.OdaTipiId);
            return View(oda);
        }

        // --- SİLME İŞLEMİ ---
        public async Task<IActionResult> Delete(int id)
        {
            var oda = await _context.Odalar.FindAsync(id);
            if (oda != null)
            {
                // --- VERİTABANI LOGLAMA ---
                _context.Loglar.Add(new Log
                {
                    KullaniciAd = HttpContext.Session.GetString("UserName") ?? "Admin",
                    Islem = "Oda Silme",
                    Detay = $"{oda.OdaNo} numaralı oda sistemden kaldırıldı.",
                    Tarih = DateTime.Now
                });

                _context.Odalar.Remove(oda);
                await _context.SaveChangesAsync();

                Console.WriteLine($"[LOG - {DateTime.Now}]: Oda #{oda.OdaNo} silindi.");
                TempData["RezervasyonMesaj"] = $"Oda #{oda.OdaNo} başarıyla silindi.";
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var oda = await _context.Odalar.FindAsync(id);
            if (oda == null) return NotFound();
            ViewBag.OdaTipiId = new SelectList(_context.OdaTipleri, "Id", "Ad", oda.OdaTipiId);
            return View(oda);
        }
    }
}