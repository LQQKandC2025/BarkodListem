using SQLite;


namespace BarkodListem.Models
{
    public class AyarlarModel
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string WebServisURL { get; set; } = String.Empty;
        public int Port { get; set; }
        public string KullaniciAdi { get; set; } = String.Empty;
        public string Sifre { get; set; } = String.Empty;
    }

}
