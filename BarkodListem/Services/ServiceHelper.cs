namespace BarkodListem.Services
{
    public static class ServiceHelper
    {
        public static T GetService<T>() where T : notnull =>
            MauiApplication.Current.Services.GetRequiredService<T>();
    }
}
