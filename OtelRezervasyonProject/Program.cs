using Microsoft.EntityFrameworkCore;
using OtelRezervasyonProject.Data; // OtelDbContext'in bulunduðu klasör

namespace OtelRezervasyonProject
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. Veritabaný Baðlantýsý (localhost\SQLEXPRESS için)
            builder.Services.AddDbContext<OtelDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // 2. Session (Oturum) Servisi Ekleme (Hocanýn þartý)
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30); // 30 dk iþlem yapýlmazsa oturum düþer
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // 3. HttpContextAccessor ekleme (Session'a her yerden kolay eriþim için)
            builder.Services.AddHttpContextAccessor();

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            // 4. Session Kullanýmýný Aktif Etme (Routing'den hemen sonra olmalý)
            app.UseSession();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}