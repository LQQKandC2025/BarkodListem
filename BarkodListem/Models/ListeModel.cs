using System.Collections.ObjectModel;
namespace BarkodListem.Models
{
    public class ListeModel
    {
        public string ListeAdi { get; set; } = string.Empty;
        public ObservableCollection<BarkodModel> Barkodlar { get; set; } = new();
    }
}