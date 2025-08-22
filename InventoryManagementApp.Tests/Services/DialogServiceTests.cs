using System.Collections.Generic;
using InventoryManagementApp.Services;
using InventoryManagementApp.Views.Windows;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace InventoryManagementApp.Tests.Services
{
    public class DialogServiceTests
    {
        private class StubImportMappingWindow : ImportMappingWindow
        {
            public StubImportMappingWindow(IEnumerable<string> headers, IEnumerable<string> properties)
                : base(headers, properties)
            {
                Loaded += (_, __) => { DialogResult = true; Close(); };
            }
        }

        private class TestDialogService : DialogService
        {
            private readonly ImportMappingWindow _window;
            public TestDialogService(ImportMappingWindow window)
                : base(new ServiceCollection().BuildServiceProvider())
            {
                _window = window;
            }

            protected override ImportMappingWindow CreateImportMappingWindow(
                IEnumerable<string> headers,
                IEnumerable<string> propertyNames,
                IEnumerable<string>? requiredPropertyNames)
                => _window;
        }

        [Fact]
        public void ShowImportMapping_ExcludesNullMappings()
        {
            var headers = new[] { "H1", "H2" };
            var properties = new[] { "Prop1", "Prop2" };
            var window = new StubImportMappingWindow(headers, properties);
            window.VM.Mappings[0].SelectedColumn = "H1";
            // second mapping remains null

            var service = new TestDialogService(window);

            var result = service.ShowImportMapping(headers, properties);

            Assert.Single(result);
            Assert.Equal("H1", result["Prop1"]);
        }
    }
}
