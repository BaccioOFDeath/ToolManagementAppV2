using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class OperationalEditWindowResponsiveContractTests
    {
        [Fact]
        public void MaintenanceEditWindow_UsesSafeScaledDesktopBoundsAndShrinkableHeader()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "MaintenanceEditWindow.xaml");

            Assert.Contains("Width=\"720\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Height=\"540\"", xaml, StringComparison.Ordinal);
            Assert.Contains("MinWidth=\"600\"", xaml, StringComparison.Ordinal);
            Assert.Contains("MinHeight=\"460\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition Width=\"*\" MinWidth=\"0\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("MaxWidth=\"220\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TextTrimming=\"CharacterEllipsis\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("Width=\"760\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("MinWidth=\"700\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void CalibrationEditWindow_UsesSafeScaledDesktopBoundsAndShrinkableHeader()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "CalibrationEditWindow.xaml");

            Assert.Contains("Width=\"720\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Height=\"520\"", xaml, StringComparison.Ordinal);
            Assert.Contains("MinWidth=\"600\"", xaml, StringComparison.Ordinal);
            Assert.Contains("MinHeight=\"460\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition Width=\"*\" MinWidth=\"0\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("MaxWidth=\"220\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TextTrimming=\"CharacterEllipsis\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("Width=\"760\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("MinWidth=\"700\"", xaml, StringComparison.Ordinal);
        }

        [Theory]
        [InlineData("MaintenanceEditWindow.xaml")]
        [InlineData("CalibrationEditWindow.xaml")]
        public void OperationalEditWindows_WrapSummaryCardsInsteadOfUsingFixedUniformGrid(string fileName)
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Windows", fileName);

            Assert.Contains("<WrapPanel Grid.Row=\"1\" Margin=\"0,0,0,8\">", xaml, StringComparison.Ordinal);
            Assert.Contains("MinWidth=\"170\" MaxWidth=\"235\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Margin=\"0,0,8,8\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<UniformGrid Grid.Row=\"1\" Columns=\"3\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("Margin=\"6,0,0,0\"", xaml, StringComparison.Ordinal);
        }

        [Theory]
        [InlineData("MaintenanceEditWindow.xaml")]
        [InlineData("CalibrationEditWindow.xaml")]
        public void OperationalEditWindows_KeepBodyShrinkableAndScrollable(string fileName)
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Windows", fileName);

            Assert.Contains("<Border Grid.Row=\"2\" Style=\"{StaticResource Card}\" Padding=\"0\" MinWidth=\"0\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<ScrollViewer Grid.Row=\"1\" VerticalScrollBarVisibility=\"Auto\" HorizontalScrollBarVisibility=\"Disabled\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition Width=\"1.45*\" MinWidth=\"0\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition Width=\"8\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition Width=\"0.9*\" MinWidth=\"240\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("Style=\"{StaticResource AdminHandoffCard}\" Margin=\"0\" MinWidth=\"0\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<ColumnDefinition Width=\"2*\"/>", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<ColumnDefinition Width=\"1.15*\"/>", xaml, StringComparison.Ordinal);
        }

        [Theory]
        [InlineData("MaintenanceEditWindow.xaml", "<ColumnDefinition Width=\"105\"/>")]
        [InlineData("CalibrationEditWindow.xaml", "<ColumnDefinition Width=\"108\"/>")]
        public void OperationalEditWindows_ReduceFixedFormColumnPressure(string fileName, string labelColumnMarker)
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Windows", fileName);

            Assert.Contains(labelColumnMarker, xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition Width=\"*\" MinWidth=\"0\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition Width=\"12\"/>", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<ColumnDefinition Width=\"120\"/>", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<ColumnDefinition Width=\"124\"/>", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<ColumnDefinition Width=\"18\"/>", xaml, StringComparison.Ordinal);
        }

        [Theory]
        [InlineData("MaintenanceEditWindow.xaml", "MaintenanceRecord.ItemNumber", "MaintenanceRecord.Notes")]
        [InlineData("CalibrationEditWindow.xaml", "CalibrationRecord.ItemNumber", "CalibrationRecord.Notes")]
        public void OperationalEditWindows_BoundNotesAndPreservePrimaryBindings(string fileName, string identityBinding, string notesBinding)
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Windows", fileName);

            Assert.Contains(identityBinding, xaml, StringComparison.Ordinal);
            Assert.Contains(notesBinding, xaml, StringComparison.Ordinal);
            Assert.Contains("AcceptsReturn=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TextWrapping=\"Wrap\"", xaml, StringComparison.Ordinal);
            Assert.Contains("MinHeight=\"160\"", xaml, StringComparison.Ordinal);
            Assert.Contains("VerticalScrollBarVisibility=\"Auto\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<controls:SaveCancelBar Grid.Row=\"3\" Margin=\"0,10,0,0\"/>", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("MinHeight=\"210\"", xaml, StringComparison.Ordinal);
        }

        private static string ReadRepoFile(params string[] parts)
        {
            var directory = AppContext.BaseDirectory;

            while (!string.IsNullOrEmpty(directory))
            {
                var candidate = Path.Combine(directory, Path.Combine(parts));
                if (File.Exists(candidate))
                    return File.ReadAllText(candidate);

                var parent = Directory.GetParent(directory);
                if (parent is null)
                    break;

                directory = parent.FullName;
            }

            throw new FileNotFoundException($"Could not find repository file: {Path.Combine(parts)}");
        }
    }
}
