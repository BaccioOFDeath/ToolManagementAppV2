using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class ItemEditWindowXamlTests
    {
        [Fact]
        public void ContainsRentalItemCheckBoxBinding()
        {
            var path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "InventoryManagementApp", "Views", "Windows", "ItemEditWindow.xaml"));
            var doc = XDocument.Load(path);
            XNamespace ns = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
            var hasBinding = doc
                .Descendants(ns + "CheckBox")
                .Any(cb => cb.Attribute("IsChecked")?.Value.Contains("ItemModel.IsRentalItem") == true);
            Assert.True(hasBinding, "CheckBox bound to ItemModel.IsRentalItem not found");
        }
    }
}
