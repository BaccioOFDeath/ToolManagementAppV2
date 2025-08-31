using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using InventoryManagementApp.Services;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class ThemeServiceTests
    {
        [Fact]
        public async Task ApplyTheme_LoadsDarkDictionary()
        {
            await RunOnStaThread(async () =>
            {
                var app = new Application();
                var service = new ThemeService();
                service.ApplyTheme("Dark");
                Assert.Contains("Colors.Dark.xaml", app.Resources.MergedDictionaries[0].Source.OriginalString);
                app.Shutdown();
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task ApplyTheme_LoadsLightDictionary()
        {
            await RunOnStaThread(async () =>
            {
                var app = new Application();
                var service = new ThemeService();
                service.ApplyTheme("Light");
                Assert.Contains("Colors.Light.xaml", app.Resources.MergedDictionaries[0].Source.OriginalString);
                app.Shutdown();
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task ApplyTheme_ReplacesExistingDictionary()
        {
            await RunOnStaThread(async () =>
            {
                var app = new Application();
                app.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("Resources/Colors.Dark.xaml", UriKind.Relative) });
                var service = new ThemeService();
                service.ApplyTheme("Light");
                Assert.DoesNotContain(app.Resources.MergedDictionaries, d => d.Source?.OriginalString.Contains("Colors.Dark.xaml") == true);
                Assert.Contains("Colors.Light.xaml", app.Resources.MergedDictionaries[0].Source.OriginalString);
                app.Shutdown();
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
