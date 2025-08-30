using System;
using System.Windows;
using InventoryManagementApp.Interfaces;

namespace InventoryManagementApp.Services
{
    public class ThemeService : IThemeService
    {
        public void ApplyTheme(string? theme)
        {
            var path = string.Equals(theme, "Light", StringComparison.OrdinalIgnoreCase)
                ? "Resources/Colors.Light.xaml"
                : "Resources/Colors.xaml";
            var dict = new ResourceDictionary { Source = new Uri(path, UriKind.Relative) };
            var app = Application.Current;
            if (app != null && app.Resources.MergedDictionaries.Count > 0)
            {
                app.Resources.MergedDictionaries[0] = dict;
            }
        }
    }
}
