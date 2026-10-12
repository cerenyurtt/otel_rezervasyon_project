using OtelRezervasyonProject.Models;

namespace OtelRezervasyonProject.ViewModels
{
    public class OdaAramaVM
    {
        public DateTime GirisTarihi { get; set; } = DateTime.Now;
        public DateTime CikisTarihi { get; set; } = DateTime.Now.AddDays(1);
        public decimal? MinFiyat { get; set; } // Yeni: Minimum Fiyat
        public decimal? MaxFiyat { get; set; }

        // HATA BURADAYDI: Bu ismin tam olarak böyle olduğundan emin ol
        public int? SecilenOdaTipiId { get; set; }

        public List<Oda>? BulunanOdalar { get; set; }
    }
}