using BarkodListem.ViewModels;
using Microsoft.Maui.Controls;
using BarkodListem.Views;


namespace BarkodListem.Views
{
    public partial class ListePage : ContentPage
    {
       
        public ListePage(BarkodListViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (BindingContext is BarkodListViewModel viewModel)
            {
                await viewModel.LoadData();
            }
        }
    }
}
