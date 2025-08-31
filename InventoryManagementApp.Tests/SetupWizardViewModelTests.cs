using InventoryManagementApp.ViewModels;
using Xunit;

public class SetupWizardViewModelTests
{
    [Fact]
    public void Defaults_AreCorrect()
    {
        var vm = new SetupWizardViewModel(() => { }, () => { });
        Assert.Equal(string.Empty, vm.ApplicationName);
        Assert.Equal("Item", vm.ItemLabelSingular);
        Assert.Equal("Items", vm.ItemLabelPlural);
    }
}
