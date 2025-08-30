using System;
using System.Windows;
using InventoryManagementApp.Interfaces;

namespace InventoryManagementApp.Services
{
    public class ThemeService : IThemeService
    {
        readonly ResourceDictionary _light = new() { Source = new Uri("Resources/Colors.Light.xaml", UriKind.Relative) };
        readonly ResourceDictionary _dark = new() { Source = new Uri("Resources/Colors.Dark.xaml", UriKind.Relative) };

        public void ApplyTheme(string? theme)
        {
            var app = Application.Current;
            if (app == null) return;

            var dictionaries = app.Resources.MergedDictionaries;
            var dict = string.Equals(theme, "Light", StringComparison.OrdinalIgnoreCase) ? _light : _dark;
            var other = dict == _light ? _dark : _light;

            if (!dictionaries.Contains(dict))
            {
                dictionaries.Insert(0, dict);
            }
            if (dictionaries.Contains(other))
            {
                dictionaries.Remove(other);
            }
        }
    }
}
