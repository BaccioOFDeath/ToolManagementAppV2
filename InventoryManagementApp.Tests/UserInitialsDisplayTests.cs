using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class UserInitialsDisplayTests
    {
        [Fact]
        public void UsersPage_ShowsInitialsWhenNoPhotoPath()
        {
            Exception? threadEx = null;
            var thread = new Thread(() =>
            {
                try
                {
                    var app = new Application();
                    app.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("pack://application:,,,/InventoryManagementApp;component/Resources/Colors.Dark.xaml", UriKind.Absolute) });
                    app.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("pack://application:,,,/InventoryManagementApp;component/Resources/Styles.xaml", UriKind.Absolute) });
                    app.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("pack://application:,,,/InventoryManagementApp;component/Resources/Converters.xaml", UriKind.Absolute) });

                    var path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "InventoryManagementApp", "Views", "Pages", "UsersPage.xaml"));
                    var xaml = File.ReadAllText(path);
                    xaml = Regex.Replace(xaml, "x:Class=\\\"[^\\\"]*\\\"\\s*", string.Empty);
                    var page = (Page)XamlReader.Parse(xaml);
                    var dataGrid = FindVisualChild<DataGrid>(page) ?? throw new InvalidOperationException("DataGrid not found");
                    var col = (DataGridTemplateColumn)dataGrid.Columns[0];
                    var element = (FrameworkElement)col.CellTemplate.LoadContent();
                    element.DataContext = new { UserName = "John Doe", UserPhotoPath = (string?)null };
                    element.Measure(new Size(36, 36));
                    element.Arrange(new Rect(0, 0, 36, 36));
                    element.UpdateLayout();
                    var textBlock = FindVisualChild<TextBlock>(element) ?? throw new InvalidOperationException("TextBlock not found");
                    var image = FindVisualChild<Image>(element) ?? throw new InvalidOperationException("Image not found");
                    Assert.Equal("JD", textBlock.Text);
                    Assert.Equal(Visibility.Visible, textBlock.Visibility);
                    Assert.Equal(Visibility.Collapsed, image.Visibility);
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

        [Fact]
        public void UsersPage_ShowsDefaultPhotoWhenNameBlank()
        {
            Exception? threadEx = null;
            var thread = new Thread(() =>
            {
                try
                {
                    var app = new Application();
                    app.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("pack://application:,,,/InventoryManagementApp;component/Resources/Colors.Dark.xaml", UriKind.Absolute) });
                    app.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("pack://application:,,,/InventoryManagementApp;component/Resources/Styles.xaml", UriKind.Absolute) });
                    app.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("pack://application:,,,/InventoryManagementApp;component/Resources/Converters.xaml", UriKind.Absolute) });

                    var path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "InventoryManagementApp", "Views", "Pages", "UsersPage.xaml"));
                    var xaml = File.ReadAllText(path);
                    xaml = Regex.Replace(xaml, "x:Class=\\\"[^\\\"]*\\\"\\s*", string.Empty);
                    var page = (Page)XamlReader.Parse(xaml);
                    var dataGrid = FindVisualChild<DataGrid>(page) ?? throw new InvalidOperationException("DataGrid not found");
                    var col = (DataGridTemplateColumn)dataGrid.Columns[0];
                    var element = (FrameworkElement)col.CellTemplate.LoadContent();
                    element.DataContext = new { UserName = string.Empty, UserPhotoPath = (string?)null };
                    element.Measure(new Size(36, 36));
                    element.Arrange(new Rect(0, 0, 36, 36));
                    element.UpdateLayout();
                    var textBlock = FindVisualChild<TextBlock>(element) ?? throw new InvalidOperationException("TextBlock not found");
                    var image = FindVisualChild<Image>(element) ?? throw new InvalidOperationException("Image not found");
                    Assert.Equal(Visibility.Collapsed, textBlock.Visibility);
                    Assert.Equal(Visibility.Visible, image.Visibility);
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

        [Fact]
        public void MainWindow_ShowsInitialsWhenNoPhotoPath()
        {
            Exception? threadEx = null;
            var thread = new Thread(() =>
            {
                try
                {
                    var app = new Application();
                    app.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("pack://application:,,,/InventoryManagementApp;component/Resources/Colors.Dark.xaml", UriKind.Absolute) });
                    app.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("pack://application:,,,/InventoryManagementApp;component/Resources/Styles.xaml", UriKind.Absolute) });
                    app.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("pack://application:,,,/InventoryManagementApp;component/Resources/Converters.xaml", UriKind.Absolute) });

                    var path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "InventoryManagementApp", "MainWindow.xaml"));
                    var xaml = File.ReadAllText(path);
                    xaml = Regex.Replace(xaml, "x:Class=\\\"[^\\\"]*\\\"\\s*", string.Empty);
                    var window = (Window)XamlReader.Parse(xaml);
                    window.DataContext = new { CurrentUserName = "John Doe", CurrentUserPhotoPath = (string?)null };
                    window.Measure(new Size(100, 100));
                    window.Arrange(new Rect(0, 0, 100, 100));
                    window.UpdateLayout();
                    var border = FindVisualChildren<Border>(window).First(b => b.Width == 60 && b.Height == 60);
                    var grid = VisualTreeHelper.GetChild(border, 0) as Grid ?? throw new InvalidOperationException("Grid not found");
                    var textBlock = grid.Children.OfType<TextBlock>().Single();
                    var image = grid.Children.OfType<Image>().Single();
                    Assert.Equal("JD", textBlock.Text);
                    Assert.Equal(Visibility.Visible, textBlock.Visibility);
                    Assert.Equal(Visibility.Collapsed, image.Visibility);
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

        [Fact]
        public void MainWindow_ShowsInitialsWhenPhotoPathMissing()
        {
            Exception? threadEx = null;
            var thread = new Thread(() =>
            {
                try
                {
                    var app = new Application();
                    app.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("pack://application:,,,/InventoryManagementApp;component/Resources/Colors.Dark.xaml", UriKind.Absolute) });
                    app.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("pack://application:,,,/InventoryManagementApp;component/Resources/Styles.xaml", UriKind.Absolute) });
                    app.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("pack://application:,,,/InventoryManagementApp;component/Resources/Converters.xaml", UriKind.Absolute) });

                    var path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "InventoryManagementApp", "MainWindow.xaml"));
                    var xaml = File.ReadAllText(path);
                    xaml = Regex.Replace(xaml, "x:Class=\\\"[^\\\"]*\\\"\\s*", string.Empty);
                    var window = (Window)XamlReader.Parse(xaml);
                    var missing = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".png");
                    window.DataContext = new { CurrentUserName = "John Doe", CurrentUserPhotoPath = missing };
                    window.Measure(new Size(100, 100));
                    window.Arrange(new Rect(0, 0, 100, 100));
                    window.UpdateLayout();
                    var border = FindVisualChildren<Border>(window).First(b => b.Width == 60 && b.Height == 60);
                    var grid = VisualTreeHelper.GetChild(border, 0) as Grid ?? throw new InvalidOperationException("Grid not found");
                    var textBlock = grid.Children.OfType<TextBlock>().Single();
                    var image = grid.Children.OfType<Image>().Single();
                    Assert.Equal("JD", textBlock.Text);
                    Assert.Equal(Visibility.Visible, textBlock.Visibility);
                    Assert.Equal(Visibility.Collapsed, image.Visibility);
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

        [Fact]
        public void UsersEditWindow_ShowsInitialsWhenNoPhotoPath()
        {
            Exception? threadEx = null;
            var thread = new Thread(() =>
            {
                try
                {
                    var app = new Application();
                    app.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("pack://application:,,,/InventoryManagementApp;component/Resources/Colors.Dark.xaml", UriKind.Absolute) });
                    app.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("pack://application:,,,/InventoryManagementApp;component/Resources/Styles.xaml", UriKind.Absolute) });
                    app.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("pack://application:,,,/InventoryManagementApp;component/Resources/Converters.xaml", UriKind.Absolute) });

                    var path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "InventoryManagementApp", "Views", "Windows", "UsersEditWindow.xaml"));
                    var xaml = File.ReadAllText(path);
                    xaml = Regex.Replace(xaml, "x:Class=\\\"[^\\\"]*\\\"\\s*", string.Empty);
                    var window = (Window)XamlReader.Parse(xaml);
                    window.DataContext = new { EditingUser = new { UserName = "John Doe", UserPhotoPath = (string?)null } };
                    window.Measure(new Size(120, 120));
                    window.Arrange(new Rect(0, 0, 120, 120));
                    window.UpdateLayout();
                    var border = FindVisualChildren<Border>(window).First(b => b.Width == 96 && b.Height == 96);
                    var grid = VisualTreeHelper.GetChild(border, 0) as Grid ?? throw new InvalidOperationException("Grid not found");
                    var textBlock = grid.Children.OfType<TextBlock>().Single();
                    var image = grid.Children.OfType<Image>().Single();
                    Assert.Equal("JD", textBlock.Text);
                    Assert.Equal(Visibility.Visible, textBlock.Visibility);
                    Assert.Equal(Visibility.Collapsed, image.Visibility);
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

        [Fact]
        public void LoginWindow_ShowsInitialsWhenNoPhotoPath()
        {
            Exception? threadEx = null;
            var thread = new Thread(() =>
            {
                try
                {
                    var app = new Application();
                    app.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("pack://application:,,,/InventoryManagementApp;component/Resources/Colors.Dark.xaml", UriKind.Absolute) });
                    app.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("pack://application:,,,/InventoryManagementApp;component/Resources/Styles.xaml", UriKind.Absolute) });
                    app.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("pack://application:,,,/InventoryManagementApp;component/Resources/Converters.xaml", UriKind.Absolute) });

                    var path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "InventoryManagementApp", "Views", "Windows", "LoginWindow.xaml"));
                    var xaml = File.ReadAllText(path);
                    xaml = Regex.Replace(xaml, "x:Class=\\\"[^\\\"]*\\\"\\s*", string.Empty);
                    var window = (Window)XamlReader.Parse(xaml);
                    window.DataContext = new { WindowTitle = "", Users = new[] { new { UserName = "John Doe", UserPhotoPath = (string?)null } } };
                    window.Measure(new Size(120, 120));
                    window.Arrange(new Rect(0, 0, 120, 120));
                    window.UpdateLayout();
                    var itemsControl = FindVisualChildren<ItemsControl>(window).First(i => i.Name == "UsersListBox");
                    var element = (FrameworkElement)itemsControl.ItemTemplate.LoadContent();
                    element.DataContext = new { UserName = "John Doe", UserPhotoPath = (string?)null };
                    element.Measure(new Size(100, 100));
                    element.Arrange(new Rect(0, 0, 100, 100));
                    element.UpdateLayout();
                    var textBlock = FindVisualChild<TextBlock>(element) ?? throw new InvalidOperationException("TextBlock not found");
                    var image = FindVisualChild<Image>(element) ?? throw new InvalidOperationException("Image not found");
                    Assert.Equal("JD", textBlock.Text);
                    Assert.Equal(Visibility.Visible, textBlock.Visibility);
                    Assert.Equal(Visibility.Collapsed, image.Visibility);
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

        [Fact]
        public void LoginWindow_ShowsPhotoWhenPhotoPathExists()
        {
            Exception? threadEx = null;
            var thread = new Thread(() =>
            {
                try
                {
                    var app = new Application();
                    app.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("pack://application:,,,/InventoryManagementApp;component/Resources/Colors.Dark.xaml", UriKind.Absolute) });
                    app.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("pack://application:,,,/InventoryManagementApp;component/Resources/Styles.xaml", UriKind.Absolute) });
                    app.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("pack://application:,,,/InventoryManagementApp;component/Resources/Converters.xaml", UriKind.Absolute) });

                    var path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "InventoryManagementApp", "Views", "Windows", "LoginWindow.xaml"));
                    var xaml = File.ReadAllText(path);
                    xaml = Regex.Replace(xaml, "x:Class=\\\"[^\\\"]*\\\"\\s*", string.Empty);
                    var window = (Window)XamlReader.Parse(xaml);
                    var photoPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Assets", "Avatars", "40.png"));
                    window.DataContext = new { WindowTitle = "", Users = new[] { new { UserName = "John Doe", UserPhotoPath = photoPath } } };
                    window.Measure(new Size(120, 120));
                    window.Arrange(new Rect(0, 0, 120, 120));
                    window.UpdateLayout();
                    var itemsControl = FindVisualChildren<ItemsControl>(window).First(i => i.Name == "UsersListBox");
                    var element = (FrameworkElement)itemsControl.ItemTemplate.LoadContent();
                    element.DataContext = new { UserName = "John Doe", UserPhotoPath = photoPath };
                    element.Measure(new Size(100, 100));
                    element.Arrange(new Rect(0, 0, 100, 100));
                    element.UpdateLayout();
                    var textBlock = FindVisualChild<TextBlock>(element) ?? throw new InvalidOperationException("TextBlock not found");
                    var image = FindVisualChild<Image>(element) ?? throw new InvalidOperationException("Image not found");
                    Assert.Equal(Visibility.Collapsed, textBlock.Visibility);
                    Assert.Equal(Visibility.Visible, image.Visibility);
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

        private static System.Collections.Generic.IEnumerable<T> FindVisualChildren<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T t)
                    yield return t;
                foreach (var grand in FindVisualChildren<T>(child))
                    yield return grand;
            }
        }
    }
}
