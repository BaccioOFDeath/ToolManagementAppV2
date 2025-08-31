using System;
using System.Windows;
using InventoryManagementApp.Interfaces;
using Application = System.Windows.Application;

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

            // Remove any existing theme dictionaries
            for (int i = dictionaries.Count - 1; i >= 0; i--)
            {
                var source = dictionaries[i].Source?.OriginalString;
                if (source == _light.Source.OriginalString || source == _dark.Source.OriginalString)
                {
                    dictionaries.RemoveAt(i);
                }
            }

            dictionaries.Insert(0, dict);
        }
    }
}
