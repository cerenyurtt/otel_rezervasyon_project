namespace OtelRezervasyonProject.Models
{
    public class Kullanici
    {
        public int Id { get; set; }
        public string Ad { get; set; }
        public string Soyad { get; set; }
        public string Email { get; set; }
        public string Sifre { get; set; }

        public int RolId { get; set; }
        public Rol Rol { get; set; }
    }
}
