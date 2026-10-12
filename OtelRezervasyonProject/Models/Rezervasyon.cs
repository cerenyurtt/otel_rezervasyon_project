using System.ComponentModel.DataAnnotations;

namespace OtelRezervasyonProject.Models
{
    public class Rezervasyon
    {
        public int Id { get; set; }
        public int OdaId { get; set; }
        public int KullaniciId { get; set; }
        public DateTime BaslangicTarihi { get; set; }
        public DateTime BitisTarihi { get; set; }
        public decimal ToplamTutar { get; set; }

        // İlişkiler
        public virtual Oda? Oda { get; set; }
        public virtual Kullanici? Kullanici { get; set; }
    }
}