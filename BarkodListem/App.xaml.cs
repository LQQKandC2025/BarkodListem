using BarkodListem.Services;
using BarkodListem.ViewModels;


namespace BarkodListem
{
    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; }

        private readonly WebService _webService;
        public App(IServiceProvider services,WebService webService)
        {
            InitializeComponent();
            Services = services;
            _webService=webService;
            MainPage = new NavigationPage(new MainPage(Services.GetService<BarkodListViewModel>(),_webService )); // ✅ NavigationPage kullan
        }

        //protected override Window CreateWindow(IActivationState? activationState)
        //{
        //    return new Window(new AppShell());
        //}
    }
}