using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BarkodListem.Data;
using BarkodListem.Services;
using Microsoft.Maui.Storage;

namespace BarkodListem.Helpers
{

    public static class SettingsHelper
    {
        private const string WebServiceUrlKey = "WebServiceUrl";
        private const string WebServicePortKey = "WebServicePort";
        private const string UserNameKey = "UserName";
        private const string PasswordKey = "Password";

        public static string WebServiceUrl
        {
            get => Preferences.Get(WebServiceUrlKey, "http://default-url.com");
            set => Preferences.Set(WebServiceUrlKey, value);
        }

        public static string WebServicePort
        {
            get => Preferences.Get(WebServicePortKey, "8080");
            set => Preferences.Set(WebServicePortKey, value);
        }

        public static string UserName
        {
            get => Preferences.Get(UserNameKey, "admin");
            set => Preferences.Set(UserNameKey, value);
        }

        public static string Password
        {
            get => Preferences.Get(PasswordKey, "1234");
            set => Preferences.Set(PasswordKey, value);
        }
    }

}
