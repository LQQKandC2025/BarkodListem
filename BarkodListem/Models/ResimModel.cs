using SQLite;
namespace BarkodListem.Models
{
    [Table("RESIMLER")]
    public class ResimModel
    {
        [PrimaryKey, AutoIncrement]
        public int RESIM_ID { get; set; }
        [MaxLength(50), NotNull]
        public string SEVKIYAT_NO { get; set; } = string.Empty;
        public int? STOK_ID { get; set; }
        public int? SIPSTR_ID { get; set; }
        [MaxLength(255), NotNull]
        public string DOSYA_ADI { get; set; } = string.Empty;
        [MaxLength(500)]
        public string? YOL { get; set; }
        public DateTime? TARIH { get; set; }
        [MaxLength(50)]
        public string? TIPI { get; set; }
        public int GONDERIM { get; set; } = 0;
    }
}