using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class ItemSearchPageXamlTests
    {
        [Fact]
        public void ListBox_UsesVerticalVirtualizingStackPanel()
        {
            Exception? threadEx = null;
            var thread = new Thread(() =>
            {
                try
                {
                    var app = new Application();
                    app.Resources.MergedDictionaries.Add(new ResourceDictionary
                    {
                        Source = new Uri("pack://application:,,,/InventoryManagementApp;component/Resources/Styles.xaml", UriKind.Absolute)
                    });

                    var path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "InventoryManagementApp", "Views", "Pages", "ItemSearchPage.xaml"));
                    var xaml = File.ReadAllText(path);
                    xaml = Regex.Replace(xaml, "x:Class=\\\"[^\\\"]*\\\"\\s*", string.Empty);

                    var page = (Page)XamlReader.Parse(xaml);
                    var listBox = FindVisualChild<ListBox>(page) ?? throw new InvalidOperationException("ListBox not found");

                    listBox.Measure(new Size(1000, 1000));
                    listBox.Arrange(new Rect(0, 0, 1000, 1000));
                    listBox.UpdateLayout();

                    var panel = FindVisualChild<VirtualizingStackPanel>(listBox) ?? throw new InvalidOperationException("VirtualizingStackPanel not found");

                    Assert.Equal(Orientation.Vertical, panel.Orientation);
                    Assert.True(VirtualizingStackPanel.GetIsVirtualizing(listBox));
                    Assert.Equal(VirtualizationMode.Recycling, VirtualizingStackPanel.GetVirtualizationMode(listBox));
                }
                catch (Exception ex)
                {
                    threadEx = ex;
                }
                finally
                {
                    Application.Current?.Shutdown();
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            if (threadEx != null) throw threadEx;
        }

        private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T t)
                    return t;
                var result = FindVisualChild<T>(child);
                if (result != null)
                    return result;
            }
            return null;
        }
    }
}

