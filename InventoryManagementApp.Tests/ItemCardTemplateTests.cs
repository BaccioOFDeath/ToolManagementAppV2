using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class ItemCardTemplateTests
    {
        [Fact]
        public void Image_UsesThumbnailConverterAndSize()
        {
            var path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "InventoryManagementApp", "Resources", "Templates.xaml"));
            var doc = XDocument.Load(path);
            XNamespace ns = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
            XNamespace x = "http://schemas.microsoft.com/winfx/2006/xaml";
            var template = doc.Root?.Elements(ns + "DataTemplate").First(e => e.Attribute(x + "Key")?.Value == "ItemCardTemplate");
            var image = template!.Descendants(ns + "Image").First();
            Assert.Equal("280", image.Attribute("Width")?.Value);
            Assert.Equal("280", image.Attribute("Height")?.Value);
            Assert.Contains("NullToDefaultImageConverter", image.Attribute("Source")?.Value);
        }
    }
}
