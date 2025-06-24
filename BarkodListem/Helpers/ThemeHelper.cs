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
                UpdateEntryStyle(theme);
            }
        }
        private static void UpdateEntryStyle(AppTheme theme)
        {
            if (Application.Current.Resources.TryGetValue(
                theme == AppTheme.Dark ? "DarkEntryStyle" : "LightEntryStyle",
                out var entryStyle))
            {
                Application.Current.Resources["CurrentEntryStyle"] = entryStyle;
            }
        }
    }
}
