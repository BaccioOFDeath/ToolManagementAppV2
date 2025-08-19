using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.Input;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Views;
using Xunit;

namespace ToolManagementAppV2.Tests.Tests
{
    public class ToolSearchPageLayoutTests
    {
        [Fact]
        public void ToolList_UsesHorizontalVirtualizingStackPanel()
        {
            var page = new ToolSearchPage();

            var panel = page.SearchResultsList.ItemsPanel.LoadContent();
            var stack = Assert.IsType<VirtualizingStackPanel>(panel);
            Assert.Equal(Orientation.Horizontal, stack.Orientation);
            Assert.Equal(ScrollBarVisibility.Auto, ScrollViewer.GetHorizontalScrollBarVisibility(page.SearchResultsList));
            Assert.Equal(ScrollBarVisibility.Disabled, ScrollViewer.GetVerticalScrollBarVisibility(page.SearchResultsList));
            Assert.True(VirtualizingStackPanel.GetIsVirtualizing(page.SearchResultsList));
            Assert.IsType<Border>(page.SearchResultsList.ItemTemplate.LoadContent());
        }

        [Fact]
        public void ToolList_VirtualizesLargeCollections()
        {
            var page = new ToolSearchPage();
            page.SearchResultsList.ItemsSource = Enumerable.Range(0, 1000)
                .Select(i => new Tool { ToolID = i, NameDescription = $"Tool {i}" })
                .ToList();

            page.SearchResultsList.Measure(new Size(800, 300));
            page.SearchResultsList.Arrange(new Rect(0, 0, 800, 300));
            page.SearchResultsList.UpdateLayout();

            Assert.NotNull(page.SearchResultsList.ItemContainerGenerator.ContainerFromIndex(0));
            Assert.Null(page.SearchResultsList.ItemContainerGenerator.ContainerFromIndex(999));

            var first = (FrameworkElement)page.SearchResultsList.ItemContainerGenerator.ContainerFromIndex(0);
            Assert.True(first.ActualWidth > 0 && first.ActualHeight > 0);
        }

        [Fact]
        public void SearchButton_BoundToSearchCommand()
        {
            var executed = false;
            var vm = new TestVm(() => executed = true);
            var page = new ToolSearchPage { DataContext = vm };

            var root = (Grid)page.Content;
            var border = (Border)root.Children[0];
            var innerGrid = (Grid)border.Child;
            var button = Assert.IsType<Button>(innerGrid.Children[2]);

            Assert.Same(vm.SearchCommand, button.Command);
            button.Command.Execute(null);
            Assert.True(executed);
        }

        class TestVm
        {
            public IRelayCommand SearchCommand { get; }
            public TestVm(Action onExecute) => SearchCommand = new RelayCommand(onExecute);
        }
    }
}
