using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class CustomerEditWindowResponsiveContractTests
    {
        [Fact]
        public void CustomerEditWindow_UsesCompactBoundedResponsiveShell()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "CustomerEditWindow.xaml");
            var code = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "CustomerEditWindow.xaml.cs");

            Assert.Contains("Width=\"860\" Height=\"700\"", xaml, StringComparison.Ordinal);
            Assert.Contains("MinWidth=\"620\" MinHeight=\"500\"", xaml, StringComparison.Ordinal);
            Assert.Contains("UseLayoutRounding=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("SnapsToDevicePixels=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<Grid Margin=\"12\" ClipToBounds=\"True\">", xaml, StringComparison.Ordinal);
            Assert.Contains("this.UseResponsiveDefaultSize(860, 700);", code, StringComparison.Ordinal);
            Assert.DoesNotContain("Width=\"720\" Height=\"580\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("MinWidth=\"600\" MinHeight=\"500\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("this.UseResponsiveDefaultSize(900, 760);", code, StringComparison.Ordinal);
        }

        [Fact]
        public void CustomerEditWindow_WrapsStepsAndDetailSections()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "CustomerEditWindow.xaml");

            Assert.Contains("<Style x:Key=\"CustomerEditorStepCard\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<Setter Property=\"MinWidth\" Value=\"150\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<Setter Property=\"MaxWidth\" Value=\"215\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<Style x:Key=\"CustomerEditorSectionCard\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<Setter Property=\"MinWidth\" Value=\"248\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<Setter Property=\"MaxWidth\" Value=\"360\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<WrapPanel Grid.Row=\"1\" Margin=\"0,0,0,2\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<WrapPanel>", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<Grid Margin=\"12\" MinWidth=\"0\">", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<ColumnDefinition Width=\"1.1*\" MinWidth=\"0\"/>", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void CustomerEditWindow_BoundsHeaderFieldsAndScrollableBody()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "CustomerEditWindow.xaml");

            Assert.Contains("MaxHeight=\"116\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition Width=\"*\" MinWidth=\"0\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("TextTrimming=\"CharacterEllipsis\"", xaml, StringComparison.Ordinal);
            Assert.Contains("MaxHeight=\"42\"", xaml, StringComparison.Ordinal);
            Assert.Contains("MaxWidth=\"170\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<ScrollViewer Grid.Row=\"1\"", xaml, StringComparison.Ordinal);
            Assert.Contains("CanContentScroll=\"False\"", xaml, StringComparison.Ordinal);
            Assert.Contains("PanningMode=\"VerticalOnly\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("CanContentScroll=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("IsEnabled=\"{Binding CanEditCustomer}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition Width=\"88\" MinWidth=\"74\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition Width=\"78\" MinWidth=\"68\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("MinHeight=\"72\"", xaml, StringComparison.Ordinal);
            Assert.Contains("MaxHeight=\"128\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<ColumnDefinition Width=\"104\"/>", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<ColumnDefinition Width=\"82\"/>", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("MinHeight=\"96\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void CustomerEditWindow_ShowsReadinessAndSavingOverlay()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "CustomerEditWindow.xaml");

            Assert.Contains("Text=\"{Binding SaveReadinessText}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Text=\"{Binding StatusMessage}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Visibility=\"{Binding IsSaving, Converter={StaticResource BoolToVis}}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Text=\"Saving customer profile\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Directory fields are paused until the customer update finishes.", xaml, StringComparison.Ordinal);
            Assert.Contains("<controls:SaveCancelBar Grid.Row=\"3\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("Text=\"Customer profile ready\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void CustomerEditViewModel_GatesSaveAndReportsReadiness()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "CustomerEditViewModel.cs");

            Assert.Contains("using System.ComponentModel;", source, StringComparison.Ordinal);
            Assert.Contains("private bool _isSaving;", source, StringComparison.Ordinal);
            Assert.Contains("public bool IsSaving", source, StringComparison.Ordinal);
            Assert.Contains("public bool CanEditCustomer => !IsSaving;", source, StringComparison.Ordinal);
            Assert.Contains("public bool CanSaveCustomer => !IsSaving && HasRequiredCustomerDetails();", source, StringComparison.Ordinal);
            Assert.Contains("public bool HasValidationMessage", source, StringComparison.Ordinal);
            Assert.Contains("public string SaveReadinessText", source, StringComparison.Ordinal);
            Assert.Contains("Customer.PropertyChanged += Customer_PropertyChanged;", source, StringComparison.Ordinal);
            Assert.Contains("SaveCommand = new RelayCommand(Save, () => CanSaveCustomer);", source, StringComparison.Ordinal);
            Assert.Contains("CancelCommand = new RelayCommand(onCancel, () => CanEditCustomer);", source, StringComparison.Ordinal);
            Assert.Contains("IsSaving = true;", source, StringComparison.Ordinal);
            Assert.Contains("IsSaving = false;", source, StringComparison.Ordinal);
            Assert.Contains("SaveCommand.NotifyCanExecuteChanged();", source, StringComparison.Ordinal);
            Assert.Contains("CancelCommand.NotifyCanExecuteChanged();", source, StringComparison.Ordinal);
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

        private static string NormalizeLineEndings(string text) =>
            text.Replace("\r\n", "\n", StringComparison.Ordinal).Replace("\r", "\n", StringComparison.Ordinal);
    }
}
