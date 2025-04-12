namespace BarkodListem.Services
{
    [System.Obsolete]
    public static class ServiceHelper
    {
        public static T GetService<T>() => MauiApplication.Current.Services.GetService<T>();
    }
}
