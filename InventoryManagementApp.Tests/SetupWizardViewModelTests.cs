using InventoryManagementApp.ViewModels;
using InventoryManagementApp.Interfaces;
using Xunit;

public class SetupWizardViewModelTests
{
    [Fact]
    public void Defaults_AreCorrect()
    {
        var vm = new SetupWizardViewModel(new DummyFileDialogService(), () => { }, () => { });
        Assert.Equal(string.Empty, vm.ApplicationName);
        Assert.Equal("Item", vm.ItemLabelSingular);
        Assert.Equal("Items", vm.ItemLabelPlural);
        Assert.Equal(string.Empty, vm.CompanyLogoPath);
    }

    private sealed class DummyFileDialogService : IFileDialogService
    {
        public string? OpenFile(string filter) => null;
        public string? SaveFile(string filter) => null;
    }
}
