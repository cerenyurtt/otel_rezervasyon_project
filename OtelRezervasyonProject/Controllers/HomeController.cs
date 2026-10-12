using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OtelRezervasyonProject.Data;
using OtelRezervasyonProject.Models;
using OtelRezervasyonProject.ViewModels;
using System.Diagnostics;

namespace OtelRezervasyonProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly OtelDbContext _context;

        public HomeController(OtelDbContext context)
        {
            _context = context;
        }

        // Ana Sayfa - Ýlk açýlýþ
        public IActionResult Index()
        {
            // Dropdown listesini dolduruyoruz
            ViewBag.OdaTipleri = new SelectList(_context.OdaTipleri, "Id", "Ad");

            // Varsayýlan olarak bugünü ve yarýný atýyoruz
            var model = new OdaAramaVM
            {
                GirisTarihi = DateTime.Now,
                CikisTarihi = DateTime.Now.AddDays(1)
            };
            return View(model);
        }

        // HOCANIN ÞARTI: ViewModel ile Detaylý ve Tarih Bazlý Arama
        [HttpGet]
        public async Task<IActionResult> OdaAra(OdaAramaVM model)
        {
            // Dropdown'un sayfa yenilendiðinde dolu kalmasýný saðlýyoruz
            ViewBag.OdaTipleri = new SelectList(_context.OdaTipleri, "Id", "Ad");

            // 1. ADIM: Seçilen tarihler arasýnda zaten rezerve edilmiþ olan oda ID'lerini bul
            // Mantýk: Girilen Giriþ Tarihi < Rezervasyon Bitiþ VE Girilen Çýkýþ Tarihi > Rezervasyon Baþlangýç
            var doluOdaIds = await _context.Rezervasyonlar
                .Where(r => model.GirisTarihi < r.BitisTarihi && model.CikisTarihi > r.BaslangicTarihi)
                .Select(r => r.OdaId)
                .ToListAsync();

            // 2. ADIM: Temel Sorgu - Dolu olmayan ve genel olarak müsait iþaretlenmiþ odalar
            var sorgu = _context.Odalar
                .Include(o => o.OdaTipi)
                .Where(o => o.MusaitMi == true && !doluOdaIds.Contains(o.Id))
                .AsQueryable();

            // --- YENÝ EKLEME: FÝYAT FÝLTRELEME ---
            // Minimum fiyat filtresi
            if (model.MinFiyat.HasValue && model.MinFiyat > 0)
            {
                sorgu = sorgu.Where(o => o.GecelikFiyat >= model.MinFiyat.Value);
            }

            // Maksimum fiyat filtresi
            if (model.MaxFiyat.HasValue && model.MaxFiyat > 0)
            {
                sorgu = sorgu.Where(o => o.GecelikFiyat <= model.MaxFiyat.Value);
            }
            // -------------------------------------

            // 3. ADIM: Eðer kullanýcý bir oda tipi seçtiyse filtrele
            if (model.SecilenOdaTipiId.HasValue && model.SecilenOdaTipiId.Value > 0)
            {
                sorgu = sorgu.Where(o => o.OdaTipiId == model.SecilenOdaTipiId.Value);
            }

            model.BulunanOdalar = await sorgu.ToListAsync();

            // HOCANIN ÞARTI: Loglama
            Console.WriteLine($"[LOG - {DateTime.Now}]: Arama yapýldý ({model.GirisTarihi.ToShortDateString()} - {model.CikisTarihi.ToShortDateString()}).");
            Console.WriteLine($"[LOG]: Fiyat Aralýðý: {model.MinFiyat} - {model.MaxFiyat} TL");
            Console.WriteLine($"[LOG]: Bulunan uygun oda sayýsý: {model.BulunanOdalar.Count}");

            if (model.SecilenOdaTipiId.HasValue)
                Console.WriteLine($"[LOG]: Seçilen Oda Tipi ID: {model.SecilenOdaTipiId}");

            return View("Index", model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


      
    }
}