﻿using CommunityToolkit.Mvvm.ComponentModel;

namespace BarkodListem.Models
{
    public partial class PaketHrkModel : ObservableObject
    {
        public int PAKET_HRK_ID { get; set; }
        public string PAKET_ADI { get; set; }
        public string KAREKOD { get; set; }
        public string TERMINAL { get; set; }
        public string ZR_KAREKOD { get; set; }
        public int KAREKOD_ID { get; set; }

        
        [ObservableProperty]
        private bool isHighlighted;
    }
}
