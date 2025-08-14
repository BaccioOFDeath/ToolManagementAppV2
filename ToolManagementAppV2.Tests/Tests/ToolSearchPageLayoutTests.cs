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
        public void ToolLists_UseHorizontalVirtualizingStackPanel()
        {
            var page = new ToolSearchPage();

            var handPanel = page.HandToolsList.ItemsPanel.LoadContent();
            var handStack = Assert.IsType<VirtualizingStackPanel>(handPanel);
            Assert.Equal(Orientation.Horizontal, handStack.Orientation);
            Assert.Equal(ScrollBarVisibility.Auto, ScrollViewer.GetHorizontalScrollBarVisibility(page.HandToolsList));
            Assert.Equal(ScrollBarVisibility.Disabled, ScrollViewer.GetVerticalScrollBarVisibility(page.HandToolsList));
            Assert.True(VirtualizingStackPanel.GetIsVirtualizing(page.HandToolsList));
            Assert.IsType<Grid>(page.HandToolsList.ItemTemplate.LoadContent());

            var powerPanel = page.PowerToolsList.ItemsPanel.LoadContent();
            var powerStack = Assert.IsType<VirtualizingStackPanel>(powerPanel);
            Assert.Equal(Orientation.Horizontal, powerStack.Orientation);
            Assert.Equal(ScrollBarVisibility.Auto, ScrollViewer.GetHorizontalScrollBarVisibility(page.PowerToolsList));
            Assert.Equal(ScrollBarVisibility.Disabled, ScrollViewer.GetVerticalScrollBarVisibility(page.PowerToolsList));
            Assert.True(VirtualizingStackPanel.GetIsVirtualizing(page.PowerToolsList));
        }

        [Fact]
        public void ToolLists_VirtualizeLargeCollections()
        {
            var page = new ToolSearchPage();
            page.HandToolsList.ItemsSource = Enumerable.Range(0, 1000)
                .Select(i => new Tool { ToolID = i, NameDescription = $"Tool {i}" })
                .ToList();
            page.PowerToolsList.ItemsSource = Enumerable.Range(0, 1000)
                .Select(i => new Tool { ToolID = i, NameDescription = $"Power {i}" })
                .ToList();

            page.HandToolsList.Measure(new Size(800, 300));
            page.HandToolsList.Arrange(new Rect(0, 0, 800, 300));
            page.HandToolsList.UpdateLayout();

            page.PowerToolsList.Measure(new Size(800, 300));
            page.PowerToolsList.Arrange(new Rect(0, 0, 800, 300));
            page.PowerToolsList.UpdateLayout();

            Assert.NotNull(page.HandToolsList.ItemContainerGenerator.ContainerFromIndex(0));
            Assert.Null(page.HandToolsList.ItemContainerGenerator.ContainerFromIndex(999));
            Assert.NotNull(page.PowerToolsList.ItemContainerGenerator.ContainerFromIndex(0));
            Assert.Null(page.PowerToolsList.ItemContainerGenerator.ContainerFromIndex(999));

            var handFirst = (FrameworkElement)page.HandToolsList.ItemContainerGenerator.ContainerFromIndex(0);
            var powerFirst = (FrameworkElement)page.PowerToolsList.ItemContainerGenerator.ContainerFromIndex(0);
            Assert.True(handFirst.ActualWidth > 0 && handFirst.ActualHeight > 0);
            Assert.True(powerFirst.ActualWidth > 0 && powerFirst.ActualHeight > 0);
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
