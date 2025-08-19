using System.Linq;
using ToolManagementAppV2.ViewModels;
using Xunit;

namespace ToolManagementAppV2.Tests.ViewModels
{
    public class ImportMappingViewModelTests
    {
        [Fact]
        public void Constructor_PopulatesMappingsWithProvidedLists()
        {
            var headers = new[] { "ToolNumber", "NameDescription", "SerialNumber" };
            var properties = new[] { "ToolNumber", "NameDescription" };

            var vm = new ImportMappingViewModel(headers, properties, () => { }, () => { });

            Assert.Equal(headers, vm.ColumnHeaders);
            Assert.Equal(properties, vm.Mappings.Select(m => m.PropertyName));
            foreach (var mapping in vm.Mappings)
            {
                Assert.Equal(headers, mapping.AvailableColumns);
                Assert.Null(mapping.SelectedColumn);
            }
        }
    }
}
