using System;
using System.Windows;
using System.Windows.Threading;
using InventoryManagementApp.Interfaces;
using Application = System.Windows.Application;

namespace InventoryManagementApp.Services
{
    /// <summary>
    /// Service for managing application theme (light/dark mode) by loading and applying appropriate resource dictionaries.
    /// </summary>
    public class ThemeService : IThemeService
    {
        private static readonly Uri LightThemeUri = new("pack://application:,,,/InventoryManagementApp;component/Resources/Colors.Light.xaml", UriKind.Absolute);
        private static readonly Uri DarkThemeUri = new("pack://application:,,,/InventoryManagementApp;component/Resources/Colors.Dark.xaml", UriKind.Absolute);

        /// <summary>
        /// Applies the specified theme to the application.
        /// </summary>
        /// <param name="theme">The theme name ("Dark" for dark mode, any other value for light mode).</param>
        public void ApplyTheme(string? theme)
        {
            var app = Application.Current;
            if (app is null) return;

            void ApplyOnUiThread()
            {
                var dictionaries = app.Resources.MergedDictionaries;
                var themeUri = string.Equals(theme, "Dark", StringComparison.OrdinalIgnoreCase) ? DarkThemeUri : LightThemeUri;

                for (int i = dictionaries.Count - 1; i >= 0; i--)
                {
                    var source = dictionaries[i].Source?.OriginalString;
                    if (string.Equals(source, LightThemeUri.OriginalString, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(source, DarkThemeUri.OriginalString, StringComparison.OrdinalIgnoreCase))
                    {
                        dictionaries.RemoveAt(i);
                    }
                }

                dictionaries.Insert(0, new ResourceDictionary { Source = themeUri });

                foreach (Window window in app.Windows)
                {
                    window.InvalidateVisual();
                    window.UpdateLayout();
                }
            }

            if (app.Dispatcher.CheckAccess())
            {
                ApplyOnUiThread();
                return;
            }

            app.Dispatcher.Invoke(ApplyOnUiThread, DispatcherPriority.Send);
        }
    }
}
