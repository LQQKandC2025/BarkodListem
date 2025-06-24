using SQLite;
namespace BarkodListem.Models
{
    public class BarkodModel
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Barkod { get; set; } = string.Empty;
        public string ListeAdi { get; set; } = string.Empty;
    }
}
