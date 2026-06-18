using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using InventoryManagementApp.Services;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class ThemeServiceTests
    {
        private static readonly Uri LightThemeRelativeUri = new("/InventoryManagementApp;component/Resources/Colors.Light.xaml", UriKind.Relative);
        private static readonly Uri DarkThemeUri = new("pack://application:,,,/InventoryManagementApp;component/Resources/Colors.Dark.xaml", UriKind.Absolute);

        [Fact]
        public async Task ApplyTheme_LoadsDarkDictionary()
        {
            await RunOnStaThread(async () =>
            {
                var app = WpfTestHelper.CreateApplication();
                var service = new ThemeService();
                service.ApplyTheme("Dark");
                Assert.Contains("Colors.Dark.xaml", app.Resources.MergedDictionaries[0].Source.OriginalString);
                WpfTestHelper.ShutdownApplication();
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task ApplyTheme_LoadsLightDictionary()
        {
            await RunOnStaThread(async () =>
            {
                var app = WpfTestHelper.CreateApplication();
                var service = new ThemeService();
                service.ApplyTheme("Light");
                Assert.Contains("Colors.Light.xaml", app.Resources.MergedDictionaries[0].Source.OriginalString);
                WpfTestHelper.ShutdownApplication();
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task ApplyTheme_DefaultsToLightWhenThemeIsNull()
        {
            await RunOnStaThread(async () =>
            {
                var app = WpfTestHelper.CreateApplication();
                var service = new ThemeService();
                service.ApplyTheme(null);
                Assert.Contains("Colors.Light.xaml", app.Resources.MergedDictionaries[0].Source.OriginalString);
                WpfTestHelper.ShutdownApplication();
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task ApplyTheme_ReplacesExistingDictionary()
        {
            await RunOnStaThread(async () =>
            {
                var app = WpfTestHelper.CreateApplication();
                app.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = DarkThemeUri });
                var service = new ThemeService();
                service.ApplyTheme("Light");
                Assert.DoesNotContain(app.Resources.MergedDictionaries, d => d.Source?.OriginalString.Contains("Colors.Dark.xaml") == true);
                Assert.Contains("Colors.Light.xaml", app.Resources.MergedDictionaries[0].Source.OriginalString);
                WpfTestHelper.ShutdownApplication();
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task ApplyTheme_ReplacesAppStartupRelativeDictionary()
        {
            await RunOnStaThread(async () =>
            {
                var app = WpfTestHelper.CreateApplication();
                app.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = LightThemeRelativeUri });
                var service = new ThemeService();
                service.ApplyTheme("Dark");
                Assert.DoesNotContain(app.Resources.MergedDictionaries, d => d.Source?.OriginalString.Contains("Colors.Light.xaml") == true);
                Assert.Contains("Colors.Dark.xaml", app.Resources.MergedDictionaries[0].Source.OriginalString);
                Assert.Equal(Color.FromRgb(0x1E, 0x1E, 0x1E), (Color)app.Resources["Col.Background"]);
                WpfTestHelper.ShutdownApplication();
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task NavButtonBrushes_UpdateWithTheme()
        {
            await RunOnStaThread(async () =>
            {
                var app = WpfTestHelper.CreateApplication();
                var service = new ThemeService();
                service.ApplyTheme("Dark");
                var darkHover = (SolidColorBrush)app.Resources["NavButtonHoverBrush"];
                service.ApplyTheme("Light");
                var lightHover = (SolidColorBrush)app.Resources["NavButtonHoverBrush"];
                Assert.NotEqual(darkHover.Color, lightHover.Color);
                WpfTestHelper.ShutdownApplication();
                await Task.CompletedTask;
            });
        }

        static Task RunOnStaThread(Func<Task> action)
        {
            var tcs = new TaskCompletionSource();
            var thread = new Thread(async () =>
            {
                try
                {
                    await action();
                    tcs.SetResult();
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            return tcs.Task;
        }
    }
}
