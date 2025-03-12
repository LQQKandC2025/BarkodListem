﻿using BarkodListem.Data;
using BarkodListem.Services;
using BarkodListem.ViewModels;



namespace BarkodListem
{
    public partial class App : Application
    {
        public static bool IsLoggedIn { get; private set; } = false;
        public static IServiceProvider Services { get; private set; }
        private static DatabaseService _databaseService;
        private static WebService _webService;
        public App(IServiceProvider services,WebService webService,DatabaseService databaseService)
        {
            InitializeComponent();
            Services = services;
            _webService=webService;
            _databaseService=databaseService;
            MainPage = new LoginPage();
            //MainPage = new NavigationPage(new MainPage(Services.GetService<BarkodListViewModel>(),_webService,_databaseService)); // ✅ NavigationPage kullan
        }
        public static void LoginSuccessful()
        {
            IsLoggedIn = true;

            Application.Current.MainPage = new NavigationPage(new MainPage(Services.GetService<BarkodListViewModel>(), _webService, _databaseService));
        }
        public static void Logout()
        {
            IsLoggedIn = false;
            Application.Current.MainPage = new LoginPage();
        }
        //protected override Window CreateWindow(IActivationState? activationState)
        //{
        //    return new Window(new AppShell());
        //}
    }
}