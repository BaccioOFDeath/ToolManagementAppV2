using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class DashboardPageLoadingSafetyContractTests
    {
        [Fact]
        public void DashboardPage_BlocksContextMenusWhileRowsRefresh()
        {
            var codeBehind = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "DashboardPage.xaml.cs");

            Assert.Contains("AddHandler(ContextMenuService.ContextMenuOpeningEvent, new ContextMenuEventHandler(DashboardPage_ContextMenuOpening), true);", codeBehind, StringComparison.Ordinal);
            Assert.Contains("private void DashboardPage_ContextMenuOpening(object sender, ContextMenuEventArgs e)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("if (_isLoadingDashboard)\n            {\n                e.Handled = true;\n            }", codeBehind, StringComparison.Ordinal);
        }

        [Fact]
        public void DashboardPage_ClearsStaleLoadStatusWhenUnloaded()
        {
            var codeBehind = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "DashboardPage.xaml.cs");

            Assert.Contains("private void DashboardPage_Unloaded(object sender, RoutedEventArgs e)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("_loadCts?.Cancel();\n            _loadCts?.Dispose();\n            _loadCts = null;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("_isLoadingDashboard = false;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("SetDashboardInteractiveActionsEnabled(true);", codeBehind, StringComparison.Ordinal);
            Assert.Contains("SetDashboardLoadStatus(null, showRetry: false);", codeBehind, StringComparison.Ordinal);
            Assert.Contains("Cursor = null;", codeBehind, StringComparison.Ordinal);
        }

        [Fact]
        public void DashboardPage_UsesIterativeVisualTraversalWhenTogglingActions()
        {
            var codeBehind = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "DashboardPage.xaml.cs");

            Assert.Contains("private static IEnumerable<DependencyObject> EnumerateVisualDescendants(DependencyObject parent)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("var pending = new Stack<DependencyObject>();", codeBehind, StringComparison.Ordinal);
            Assert.Contains("pending.Push(parent);", codeBehind, StringComparison.Ordinal);
            Assert.Contains("while (pending.Count > 0)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("var current = pending.Pop();", codeBehind, StringComparison.Ordinal);
            Assert.Contains("for (var index = childCount - 1; index >= 0; index--)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("pending.Push(child);", codeBehind, StringComparison.Ordinal);
            Assert.DoesNotContain("foreach (var descendant in EnumerateVisualDescendants(child))", codeBehind, StringComparison.Ordinal);
        }

        private static string ReadRepoFile(params string[] parts)
        {
            var directory = AppContext.BaseDirectory;

            while (!string.IsNullOrEmpty(directory))
            {
                var candidate = Path.Combine(directory, Path.Combine(parts));
                if (File.Exists(candidate))
                    return NormalizeLineEndings(File.ReadAllText(candidate));

                var parent = Directory.GetParent(directory);
                if (parent is null)
                    break;

                directory = parent.FullName;
            }

            throw new FileNotFoundException($"Could not find repository file: {Path.Combine(parts)}");
        }

        private static string NormalizeLineEndings(string text)
            => text.Replace("\r\n", "\n");
    }
}
