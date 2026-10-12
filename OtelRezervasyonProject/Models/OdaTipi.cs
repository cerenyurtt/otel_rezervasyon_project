namespace OtelRezervasyonProject.Models
{
    public class OdaTipi
    {
        public int Id { get; set; }
        public string Ad { get; set; } // Örn: Kral Dairesi
        public string Aciklama { get; set; }
        public ICollection<Oda>? Odalar { get; set; } // Bir tipte birçok oda olabilir
    }
}
