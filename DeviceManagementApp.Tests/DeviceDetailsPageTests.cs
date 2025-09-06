using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DeviceManagementApp.Models;
using DeviceManagementApp.ViewModels;
using DeviceManagementApp.Views.Pages;
using Xunit;

namespace DeviceManagementApp.Tests
{
    public class DeviceDetailsPageTests
    {
        [Fact]
        public void DeviceDetailsPage_ShowsHardwareAndSoftware()
        {
            Exception? threadEx = null;
            string? cpuText = null;
            int softwareCount = 0;

            var thread = new Thread(() =>
            {
                try
                {
                    var app = new Application();
                    app.Resources.MergedDictionaries.Add(new ResourceDictionary
                    {
                        Source = new Uri("pack://application:,,,/DeviceManagementApp;component/Resources/Styles.xaml", UriKind.Absolute)
                    });

                    var device = new Device { Ip = "1.2.3.4", Cpu = "Intel", MemoryGb = 16, StorageGb = 512, OperatingSystem = "Windows" };
                    var software = new[] { new DeviceSoftware { Name = "App", Version = "1.0" } };
                    var vm = new DeviceDetailsViewModel(device, software);
                    var page = new DeviceDetailsPage { DataContext = vm };
                    page.ApplyTemplate();
                    page.Measure(new System.Windows.Size(double.PositiveInfinity, double.PositiveInfinity));
                    page.Arrange(new System.Windows.Rect(0, 0, page.DesiredSize.Width, page.DesiredSize.Height));
                    page.UpdateLayout();
                    cpuText = FindVisualChildren<TextBlock>(page).FirstOrDefault(t => t.Text == "Intel")?.Text;
                    var list = FindVisualChild<ListBox>(page);
                    softwareCount = list?.Items.Count ?? 0;

                    Application.Current?.Shutdown();
                }
                catch (Exception ex)
                {
                    threadEx = ex;
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            if (threadEx != null) throw threadEx;
            Assert.Equal("Intel", cpuText);
            Assert.Equal(1, softwareCount);
        }

        static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild)
                    return typedChild;
                var result = FindVisualChild<T>(child);
                if (result != null)
                    return result;
            }
            return null;
        }

        static System.Collections.Generic.IEnumerable<T> FindVisualChildren<T>(DependencyObject parent) where T : DependencyObject
        {
            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild)
                    yield return typedChild;
                foreach (var c in FindVisualChildren<T>(child))
                    yield return c;
            }
        }
    }
}
