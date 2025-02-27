﻿using BarkodListem.ViewModels;

namespace BarkodListem
{
    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; }

        public App(IServiceProvider services)
        {
            InitializeComponent();
            Services = services;
            MainPage = new NavigationPage(new MainPage(Services.GetService<BarkodListViewModel>())); // ✅ NavigationPage kullan
        }

        //protected override Window CreateWindow(IActivationState? activationState)
        //{
        //    return new Window(new AppShell());
        //}
    }
}