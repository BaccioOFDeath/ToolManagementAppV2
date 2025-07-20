using System.Windows;
using ToolManagementAppV2;
using ToolManagementAppV2.ViewModels;
using Xunit;

namespace ToolManagementAppV2.Tests.Tests
{
    public class SearchResultsResizeTests
    {
        [Fact]
        public void SearchResultsList_SizeChanged_UpdatesTileWidth()
        {
            var window = new MainWindow();
            var vm = Assert.IsType<MainViewModel>(window.DataContext);

            var args = new SizeChangedEventArgs(FrameworkElement.SizeChangedEvent, new Size(0, 0), new Size(1000, 0));
            window.SearchResultsList_SizeChanged(window.SearchResultsList, args);

            Assert.Equal(180, vm.SearchTileWidth);
        }
    }
}
