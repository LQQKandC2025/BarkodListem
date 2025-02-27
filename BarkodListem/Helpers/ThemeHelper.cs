using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Controls;

namespace BarkodListem.Helpers
{
   public static class ThemeHelper
{
    private const string ThemeKey = "AppTheme";

    public static AppTheme SelectedTheme
    {
        get => (AppTheme)Preferences.Get(ThemeKey, (int)AppTheme.Light);
        set
        {
            Preferences.Set(ThemeKey, (int)value);
            ApplyTheme(value);
        }
    }

    public static void ApplyTheme(AppTheme theme)
    {
            if (Application.Current != null)
            {
                Application.Current.UserAppTheme = theme;
            }
        }
}
}
