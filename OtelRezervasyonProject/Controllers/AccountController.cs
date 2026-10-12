using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OtelRezervasyonProject.Data;
using OtelRezervasyonProject.Models;
using OtelRezervasyonProject.ViewModels;

namespace OtelRezervasyonProject.Controllers
{
    public class AccountController : Controller
    {
        private readonly OtelDbContext _context;
        public AccountController(OtelDbContext context) { _context = context; }

        // --- MÜŞTERİ GİRİŞİ ---
        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM model)
        {
            // Sadece Müşteri (RolId = 2) olanları ara
            var user = await _context.Kullanicilar.Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Email == model.Email && u.Sifre == model.Sifre && u.RolId == 2);

            if (user != null)
            {
                OturumAc(user);

                // --- VERİTABANI LOGLAMA ---
                await LogKaydet(user.Ad + " " + user.Soyad, "Müşteri Girişi", $"{user.Email} kullanıcısı sisteme giriş yaptı.");

                Console.WriteLine($"[LOG - {DateTime.Now}]: Müşteri {user.Email} giriş yaptı.");
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Hata = "Hatalı giriş veya bu alandan sadece müşteriler giriş yapabilir!";
            return View();
        }

        // --- ADMİN ÖZEL GİRİŞİ (/Account/AdminLogin) ---
        [HttpGet]
        public IActionResult AdminLogin() => View();

        [HttpPost]
        public async Task<IActionResult> AdminLogin(LoginVM model)
        {
            // Sadece Admin (RolId = 1) olanları ara
            var user = await _context.Kullanicilar.Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Email == model.Email && u.Sifre == model.Sifre && u.RolId == 1);

            if (user != null)
            {
                OturumAc(user);

                // --- VERİTABANI LOGLAMA ---
                await LogKaydet(user.Ad + " " + user.Soyad, "Admin Girişi", $"Yönetici {user.Email} özel panelden giriş yaptı.");

                Console.WriteLine($"[LOG - ADMİN GİRİŞİ - {DateTime.Now}]: Yönetici {user.Email} özel panelden giriş yaptı.");
                return RedirectToAction("Index", "Oda");
            }

            ViewBag.Hata = "Erişim Reddedildi: Admin yetkisi bulunamadı veya bilgiler hatalı!";
            return View();
        }

        // --- KAYIT OLMA ---
        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(KayitVM model)
        {
            if (ModelState.IsValid)
            {
                if (await _context.Kullanicilar.AnyAsync(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "Bu email adresi zaten kullanımda.");
                    return View(model);
                }

                var yeniKullanici = new Kullanici
                {
                    Ad = model.Ad,
                    Soyad = model.Soyad,
                    Email = model.Email,
                    Sifre = model.Sifre,
                    RolId = 2 // Otomatik Müşteri Rolü
                };

                _context.Kullanicilar.Add(yeniKullanici);
                await _context.SaveChangesAsync();

                // --- VERİTABANI LOGLAMA ---
                await LogKaydet(model.Ad + " " + model.Soyad, "Yeni Kayıt", $"{model.Email} adresiyle yeni müşteri hesabı oluşturuldu.");

                Console.WriteLine($"[LOG - {DateTime.Now}]: Yeni müşteri kayıt oldu: {model.Email}");

                // --- İSTEDİĞİNİZ DEĞİŞİKLİK ---
                // Otomatik oturum açmayı (OturumAc) sildik.
                // Kullanıcıya giriş sayfasında görünecek bir mesaj bırakıyoruz.
                TempData["KayitBasarili"] = "Kaydınız başarıyla tamamlandı. Şimdi giriş yapabilirsiniz.";

                // Direkt Giriş sayfasına yönlendiriyoruz
                return RedirectToAction("Login");
            }
            return View(model);
        }

        // --- ÇIKIŞ YAP ---
        public async Task<IActionResult> Logout()
        {
            var userName = HttpContext.Session.GetString("UserName");
            var userRole = HttpContext.Session.GetString("UserRole");

            // --- VERİTABANI LOGLAMA ---
            await LogKaydet(userName ?? "Bilinmiyor", "Oturum Kapatma", "Kullanıcı güvenli çıkış yaptı.");

            HttpContext.Session.Clear();

            if (userRole == "Admin")
            {
                return RedirectToAction("AdminLogin");
            }
            return RedirectToAction("Login");
        }

        // --- YARDIMCI METODLAR ---

        private void OturumAc(Kullanici user)
        {
            HttpContext.Session.SetString("UserName", user.Ad + " " + user.Soyad);
            HttpContext.Session.SetString("UserRole", user.Rol?.RolAdi ?? "Müşteri");
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetInt32("UserRolId", user.RolId);
        }

        private async Task LogKaydet(string kullanici, string islem, string detay)
        {
            var yeniLog = new Log
            {
                KullaniciAd = kullanici,
                Islem = islem,
                Detay = detay,
                Tarih = DateTime.Now
            };
            _context.Loglar.Add(yeniLog);
            await _context.SaveChangesAsync();
        }
    }
}