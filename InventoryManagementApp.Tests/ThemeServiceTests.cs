using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;
using InventoryManagementApp.Models;
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

        [Fact]
        public async Task ApplyCustomTheme_UpdatesBorderlessTransparentAndTypographyResources()
        {
            await RunOnStaThread(async () =>
            {
                var app = WpfTestHelper.CreateApplication();
                var service = new ThemeService();
                var settings = AppThemeSettings.CreateDefault("Dark");
                settings.BordersVisible = false;
                settings.SurfaceOpacity = 0.25;
                settings.SurfaceAltOpacity = 0.2;
                settings.InputOpacity = 0.3;
                settings.ButtonOpacity = 0.35;
                settings.NavigationOpacity = 0.22;
                settings.FontFamily = "Aptos";
                settings.FontScale = 1.2;
                settings.HeadingFontScale = 1.1;

                service.ApplyCustomTheme(settings);

                Assert.Equal(new Thickness(0), (Thickness)app.Resources["ThemeBorderThickness"]);
                Assert.Equal(new Thickness(0), (Thickness)app.Resources["ThemeControlBorderThickness"]);
                Assert.Equal("Aptos", ((FontFamily)app.Resources["ThemeFontFamily"]).Source);
                Assert.Equal(15.6, (double)app.Resources["ThemeBodyFontSize"]);
                Assert.Equal(23.8, (double)app.Resources["ThemeTitleFontSize"]);
                Assert.Equal(0x40, ((SolidColorBrush)app.Resources["SurfaceBrush"]).Color.A);
                Assert.Equal(0x59, ((SolidColorBrush)app.Resources["BtnBg"]).Color.A);
                Assert.Same(Brushes.Transparent, app.Resources["BtnBorder"]);
                WpfTestHelper.ShutdownApplication();
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task ApplyCustomTheme_SeparatesSurfaceAndControlShadowDepth()
        {
            await RunOnStaThread(async () =>
            {
                var app = WpfTestHelper.CreateApplication();
                var service = new ThemeService();
                var settings = AppThemeSettings.CreateDefault();
                settings.EnableSurfaceShadows = true;
                settings.EnableControlShadows = true;
                settings.ShadowBlurRadius = 10;
                settings.ShadowDepth = 4;
                settings.ShadowOpacity = 0.2;
                settings.SurfaceShadowScale = 2;
                settings.ControlShadowScale = 0.5;

                service.ApplyCustomTheme(settings);

                var surfaceShadow = (DropShadowEffect)app.Resources["ThemeSurfaceShadow"];
                var controlShadow = (DropShadowEffect)app.Resources["ThemeControlShadow"];

                Assert.Equal(20, surfaceShadow.BlurRadius);
                Assert.Equal(8, surfaceShadow.ShadowDepth);
                Assert.Equal(2.5, controlShadow.BlurRadius);
                Assert.Equal(1, controlShadow.ShadowDepth);
                Assert.True(surfaceShadow.Opacity > controlShadow.Opacity);
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
