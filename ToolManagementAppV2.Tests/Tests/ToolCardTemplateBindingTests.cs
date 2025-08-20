using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using ToolManagementAppV2.Utilities.Converters;
using Xunit;

namespace ToolManagementAppV2.Tests.Tests
{
    public class ToolCardTemplateBindingTests
    {
        [Fact]
        public void ToolCardTemplate_UsesNullToDefaultImageConverter()
        {
            Exception? threadEx = null;
            var thread = new Thread(() =>
            {
                try
                {
                    var dict = new ResourceDictionary
                    {
                        Source = new Uri("pack://application:,,,/Resources/Templates.xaml", UriKind.Absolute)
                    };
                    var template = Assert.IsType<DataTemplate>(dict["ToolCardTemplate"]);
                    var grid = Assert.IsType<Grid>(template.LoadContent());
                    var border = Assert.IsType<Border>(grid.Children[0]);
                    var image = Assert.IsType<Image>(border.Child);
                    var binding = BindingOperations.GetBinding(image, Image.SourceProperty);
                    Assert.NotNull(binding);
                    Assert.IsType<NullToDefaultImageConverter>(binding!.Converter);
                    Assert.Equal("item", binding.ConverterParameter);
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
