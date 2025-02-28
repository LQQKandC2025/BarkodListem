using BarkodListem.Data;
using BarkodListem.Services;
using BarkodListem.ViewModels;



namespace BarkodListem
{
    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; }
        private readonly DatabaseService _databaseService;
        private readonly WebService _webService;
        public App(IServiceProvider services,WebService webService,DatabaseService databaseService)
        {
            InitializeComponent();
            Services = services;
            _webService=webService;
            _databaseService=databaseService;
            MainPage = new NavigationPage(new MainPage(Services.GetService<BarkodListViewModel>(),_webService,_databaseService)); // ✅ NavigationPage kullan
        }

        //protected override Window CreateWindow(IActivationState? activationState)
        //{
        //    return new Window(new AppShell());
        //}
    }
}