namespace OtelRezervasyonProject.Data
{
    using Microsoft.EntityFrameworkCore;
    using OtelRezervasyonProject.Models;

    public class OtelDbContext : DbContext
    {
        public OtelDbContext(DbContextOptions<OtelDbContext> options) : base(options) { }

        public DbSet<Oda> Odalar { get; set; }
        public DbSet<OdaTipi> OdaTipleri { get; set; }
        public DbSet<Kullanici> Kullanicilar { get; set; }
        public DbSet<Rol> Roller { get; set; }
        public DbSet<Rezervasyon> Rezervasyonlar { get; set; }
        public DbSet<Log> Loglar { get; set; }
    }
}
