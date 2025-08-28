using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class UserAvatarTests
    {
        [Fact]
        public void ShowsInitialsWhenNoPhoto()
        {
            Exception? threadEx = null;
            var thread = new Thread(() =>
            {
                try
                {
                    var app = new Application();
                    app.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("pack://application:,,,/InventoryManagementApp;component/Resources/Colors.xaml", UriKind.Absolute) });
                    app.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("pack://application:,,,/InventoryManagementApp;component/Resources/Styles.xaml", UriKind.Absolute) });
                    app.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("pack://application:,,,/InventoryManagementApp;component/Resources/Converters.xaml", UriKind.Absolute) });

                    var avatar = new Controls.UserAvatar
                    {
                        UserName = "John Doe",
                        UserPhotoPath = null,
                        Foreground = Brushes.Black,
                        FontSize = 20
                    };
                    avatar.Measure(new Size(36, 36));
                    avatar.Arrange(new Rect(0, 0, 36, 36));
                    avatar.UpdateLayout();

                    var textBlock = FindVisualChild<TextBlock>(avatar) ?? throw new InvalidOperationException("TextBlock not found");
                    var image = FindVisualChild<Image>(avatar) ?? throw new InvalidOperationException("Image not found");
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
        public void LeavesAvatarBlankWhenNameAndPhotoMissing()
        {
            Exception? threadEx = null;
            var thread = new Thread(() =>
            {
                try
                {
                    var app = new Application();
                    app.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("pack://application:,,,/InventoryManagementApp;component/Resources/Colors.xaml", UriKind.Absolute) });
                    app.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("pack://application:,,,/InventoryManagementApp;component/Resources/Styles.xaml", UriKind.Absolute) });
                    app.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("pack://application:,,,/InventoryManagementApp;component/Resources/Converters.xaml", UriKind.Absolute) });

                    var avatar = new Controls.UserAvatar
                    {
                        UserName = string.Empty,
                        UserPhotoPath = null
                    };
                    avatar.Measure(new Size(36, 36));
                    avatar.Arrange(new Rect(0, 0, 36, 36));
                    avatar.UpdateLayout();

                    var textBlock = FindVisualChild<TextBlock>(avatar) ?? throw new InvalidOperationException("TextBlock not found");
                    var image = FindVisualChild<Image>(avatar) ?? throw new InvalidOperationException("Image not found");
                    Assert.Equal(Visibility.Collapsed, textBlock.Visibility);
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
        public void ShowsPhotoWhenPhotoPathExists()
        {
            Exception? threadEx = null;
            var thread = new Thread(() =>
            {
                try
                {
                    var app = new Application();
                    app.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("pack://application:,,,/InventoryManagementApp;component/Resources/Colors.xaml", UriKind.Absolute) });
                    app.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("pack://application:,,,/InventoryManagementApp;component/Resources/Styles.xaml", UriKind.Absolute) });
                    app.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("pack://application:,,,/InventoryManagementApp;component/Resources/Converters.xaml", UriKind.Absolute) });

                    var photoPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Assets", "Avatars", "40.png"));
                    var avatar = new Controls.UserAvatar
                    {
                        UserName = "John Doe",
                        UserPhotoPath = photoPath
                    };
                    avatar.Measure(new Size(36, 36));
                    avatar.Arrange(new Rect(0, 0, 36, 36));
                    avatar.UpdateLayout();

                    var textBlock = FindVisualChild<TextBlock>(avatar) ?? throw new InvalidOperationException("TextBlock not found");
                    var image = FindVisualChild<Image>(avatar) ?? throw new InvalidOperationException("Image not found");
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
    }
}
