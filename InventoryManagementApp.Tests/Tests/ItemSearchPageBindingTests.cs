using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using InventoryManagementApp.Utilities.Converters;
using InventoryManagementApp.Views.Pages;
using InventoryManagementApp.Views.Windows;
using Xunit;

namespace InventoryManagementApp.Tests.Tests
{
    public class ItemSearchPageBindingTests
    {
        [Fact]
        public void ItemsList_UsesNullToDefaultImageConverter()
        {
            Exception? threadEx = null;
            var thread = new Thread(() =>
            {
                try
                {
                    bool createdApp = false;
                    var app = Application.Current;
                    if (app == null)
                    {
                        app = new Application();
                        createdApp = true;
                    }

                    void EnsureDictionary(string file)
                    {
                        var uri = new Uri($"pack://application:,,,/Resources/{file}", UriKind.Absolute);
                        if (!app.Resources.MergedDictionaries.Any(d => d.Source == uri))
                        {
                            app.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = uri });
                        }
                    }

                    EnsureDictionary("Colors.xaml");
                    EnsureDictionary("Styles.xaml");
                    EnsureDictionary("Converters.xaml");
                    EnsureDictionary("Templates.xaml");

                    ItemSearchPage page = null!;
                    var ex = Record.Exception(() => page = new ItemSearchPage());
                    Assert.Null(ex);

                    var itemsBinding = BindingOperations.GetBinding(page.ItemsList, ItemsControl.ItemsSourceProperty);
                    Assert.Equal("Tools", itemsBinding?.Path.Path);

                    var template = page.ItemsList.ItemTemplate;
                    var outer = Assert.IsType<Border>(template.LoadContent());
                    var grid = Assert.IsType<Grid>(outer.Child);
                    var border = Assert.IsType<Border>(grid.Children[0]);
                    var image = Assert.IsType<Image>(border.Child);
                    var imageBinding = BindingOperations.GetBinding(image, Image.SourceProperty);
                    Assert.NotNull(imageBinding);
                    Assert.IsType<NullToDefaultImageConverter>(imageBinding!.Converter);
                    Assert.Equal("item", imageBinding.ConverterParameter);

                    if (createdApp)
                        app.Shutdown();
                }
                catch (Exception ex)
                {
                    threadEx = ex;
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (threadEx != null)
            {
                throw threadEx;
            }
        }
    }
}
