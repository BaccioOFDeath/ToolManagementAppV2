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
        public void ItemCardTemplate_ShowsAvailabilityAndStatusBindings()
        {
            var path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "InventoryManagementApp", "Resources", "Templates.xaml"));
            var doc = XDocument.Load(path);
            XNamespace ns = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
            XNamespace x = "http://schemas.microsoft.com/winfx/2006/xaml";
            var template = doc.Root?.Elements(ns + "DataTemplate").First(e => e.Attribute(x + "Key")?.Value == "ItemCardTemplate");
            var image = template!.Descendants(ns + "Image").First();
            Assert.Contains("NullToDefaultImageConverter", image.Attribute("Source")?.Value);

            var texts = template.Descendants(ns + "TextBlock")
                .Select(element => element.Attribute("Text")?.Value)
                .Where(value => value != null)
                .ToList();

            Assert.Contains("{Binding OnHand, StringFormat=On Hand: {0}}", texts);

            var triggers = template.Descendants(ns + "DataTrigger")
                .Select(trigger => trigger.Attribute("Binding")?.Value)
                .Where(value => value != null)
                .ToList();

            Assert.Contains("{Binding HasNoOnHand}", triggers);
            Assert.Contains("{Binding HasRentedStock}", triggers);
            Assert.Contains("{Binding IsCheckedOut}", triggers);
        }
    }
}
