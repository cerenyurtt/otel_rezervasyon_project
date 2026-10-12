using System.ComponentModel.DataAnnotations;

namespace OtelRezervasyonProject.Models
{
    public class Log
    {
        [Key]
        public int Id { get; set; }

        public string KullaniciAd { get; set; } = "Sistem";

        public string Islem { get; set; } // Örn: Oda Ekleme, Rezervasyon

        public string Detay { get; set; } // İşlem açıklaması

        public DateTime Tarih { get; set; } = DateTime.Now;
    }
}