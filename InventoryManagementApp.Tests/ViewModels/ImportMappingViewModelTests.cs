using System.Linq;
using InventoryManagementApp.ViewModels;
using Xunit;

namespace InventoryManagementApp.Tests.ViewModels
{
    public class ImportMappingViewModelTests
    {
        [Fact]
        public void Constructor_PopulatesMappingsWithProvidedLists()
        {
            var headers = new[] { "ItemNumber", "NameDescription", "SerialNumber" };
            var properties = new[] { "ItemNumber", "NameDescription" };

            var vm = new ImportMappingViewModel(headers, properties, () => { }, () => { });

            Assert.Equal(headers, vm.ColumnHeaders);
            Assert.Equal(properties, vm.Mappings.Select(m => m.PropertyName));
            foreach (var mapping in vm.Mappings)
            {
                Assert.Equal(headers, mapping.AvailableColumns);
                Assert.Null(mapping.SelectedColumn);
            }
        }

        [Fact]
        public void OkCommand_InvokesOnOk_WhenRequiredFieldsMapped()
        {
            var headers = new[] { "ItemNumber", "NameDescription" };
            var properties = new[] { "ItemNumber", "NameDescription" };
            var required = new[] { "ItemNumber" };

            var called = false;
            var vm = new ImportMappingViewModel(headers, properties, () => called = true, () => { }, required);

            vm.Mappings.First(m => m.PropertyName == "ItemNumber").SelectedColumn = "ItemNumber";
            // leave NameDescription unmapped

            vm.OkCommand.Execute(null);

            Assert.True(called);
        }

        [Fact]
        public void OkCommand_InvokesOnOk_WhenOnlyRequiredFieldsMapped()
        {
            var headers = new[] { "ItemNumber", "NameDescription", "Extra" };
            var properties = new[] { "ItemNumber", "NameDescription", "Extra" };
            var required = new[] { "ItemNumber", "NameDescription" };

            var called = false;
            var vm = new ImportMappingViewModel(headers, properties, () => called = true, () => { }, required);

            vm.Mappings.First(m => m.PropertyName == "ItemNumber").SelectedColumn = "ItemNumber";
            vm.Mappings.First(m => m.PropertyName == "NameDescription").SelectedColumn = "NameDescription";
            // leave Extra unmapped

            vm.OkCommand.Execute(null);

            Assert.True(called);
        }
    }
}
