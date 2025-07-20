using Xunit;
using System.Windows.Controls;
using System.Linq;

namespace ToolManagementAppV2.Tests.Tests
{
    public class SearchTabPreloadTests
    {
        [Fact]
        public void SearchResultsList_IsLoadedOnStartup()
        {
            var window = new ToolManagementAppV2.MainWindow();
            Assert.True(window.SearchResultsList.IsLoaded);
        }
    }
}
