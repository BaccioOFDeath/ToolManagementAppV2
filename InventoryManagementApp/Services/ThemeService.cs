using System;
using System.Windows;
using InventoryManagementApp.Interfaces;
using Application = System.Windows.Application;

namespace InventoryManagementApp.Services
{
    /// <summary>
    /// Service for managing application theme (light/dark mode) by loading and applying appropriate resource dictionaries.
    /// </summary>
    public class ThemeService : IThemeService
    {
        private readonly ResourceDictionary _light = new() { Source = new Uri("Resources/Colors.Light.xaml", UriKind.Relative) };
        private readonly ResourceDictionary _dark = new() { Source = new Uri("Resources/Colors.Dark.xaml", UriKind.Relative) };

        /// <summary>
        /// Applies the specified theme to the application.
        /// </summary>
        /// <param name="theme">The theme name ("Dark" for dark mode, any other value for light mode).</param>
        public void ApplyTheme(string? theme)
        {
            var app = Application.Current;
            if (app is null) return;

            var dictionaries = app.Resources.MergedDictionaries;
            var dict = string.Equals(theme, "Dark", StringComparison.OrdinalIgnoreCase) ? _dark : _light;

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
