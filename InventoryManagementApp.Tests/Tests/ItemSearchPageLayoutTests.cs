using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.Input;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Views.Pages;
using InventoryManagementApp.Views.Windows;
using Xunit;

namespace InventoryManagementApp.Tests.Tests
{
    public class ItemSearchPageLayoutTests
    {
        [Fact]
        public void SingleList_UsesHorizontalVirtualizingStackPanel()
        {
            var page = new ItemSearchPage();

            var panel = page.ItemsList.ItemsPanel.LoadContent();
            var stack = Assert.IsType<VirtualizingStackPanel>(panel);
            Assert.Equal(Orientation.Horizontal, stack.Orientation);
            Assert.Equal(ScrollBarVisibility.Auto, ScrollViewer.GetHorizontalScrollBarVisibility(page.ItemsList));
            Assert.Equal(ScrollBarVisibility.Disabled, ScrollViewer.GetVerticalScrollBarVisibility(page.ItemsList));
            Assert.True(VirtualizingStackPanel.GetIsVirtualizing(page.ItemsList));
            Assert.IsType<Border>(page.ItemsList.ItemTemplate.LoadContent());
        }

        [Fact]
        public void SingleList_VirtualizesLargeCollections()
        {
            var page = new ItemSearchPage();
            page.ItemsList.ItemsSource = Enumerable.Range(0, 1000)
                .Select(i => new ItemModel { ItemID = i, NameDescription = $"ItemModel {i}" })
                .ToList();

            page.ItemsList.Measure(new Size(800, 300));
            page.ItemsList.Arrange(new Rect(0, 0, 800, 300));
            page.ItemsList.UpdateLayout();

            Assert.NotNull(page.ItemsList.ItemContainerGenerator.ContainerFromIndex(0));
            Assert.Null(page.ItemsList.ItemContainerGenerator.ContainerFromIndex(999));

            var first = (FrameworkElement)page.ItemsList.ItemContainerGenerator.ContainerFromIndex(0);
            Assert.True(first.ActualWidth > 0 && first.ActualHeight > 0);
        }

        [Fact]
        public void SearchButton_BoundToSearchCommand()
        {
            var executed = false;
            var vm = new TestVm(() => executed = true);
            var page = new ItemSearchPage { DataContext = vm };

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
