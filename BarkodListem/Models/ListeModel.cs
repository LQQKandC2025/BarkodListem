using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections.ObjectModel;

namespace BarkodListem.Models
{
    public class ListeModel
    {
        public string ListeAdi { get; set; } = string.Empty;
        public ObservableCollection<BarkodModel> Barkodlar { get; set; } = new();
    }
}