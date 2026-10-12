using OtelRezervasyonProject.Models;
using System.ComponentModel.DataAnnotations;


namespace OtelRezervasyonProject.Models
{
    public class Oda
    {
        public int Id { get; set; }

        [Display(Name = "Oda Numarası")]
        public string OdaNo { get; set; }

        [Display(Name = "Gecelik Fiyat")]
        public decimal GecelikFiyat { get; set; }

        [Display(Name = "Müsait Mi?")]
        public bool MusaitMi { get; set; } = true;

        // Oda Tipi ile İlişki
        [Display(Name = "Oda Tipi")]
        public int OdaTipiId { get; set; }

        // GÜNCELLEME: Soru işareti (?) ekleyerek bu alanı 'null' olabilir (opsiyonel) yaptık. 
        // Böylece kayıt anında 'OdaTipi nesnesi nerede?' hatası almayacaksın.
        public virtual OdaTipi? OdaTipi { get; set; }
    }
}