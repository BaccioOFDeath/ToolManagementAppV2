using ToolManagementAppV2;
using ToolManagementAppV2.ViewModels;
using ToolManagementAppV2.Views;
using Xunit;

namespace ToolManagementAppV2.Tests.Tests
{
    public class NavigationCommandsTests
    {
        [Fact]
        public void OpenSearchToolsCommand_NavigatesToToolSearchPage()
        {
            var window = new MainWindow();
            var vm = Assert.IsType<MainViewModel>(window.DataContext);

            vm.OpenSearchToolsCommand.Execute(null);

            Assert.IsType<ToolSearchPage>(vm.CurrentPage);
        }
    }
}
