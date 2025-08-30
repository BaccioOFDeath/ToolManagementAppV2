using InventoryManagementApp.ViewModels;
using Xunit;

public class SetupWizardViewModelTests
{
    [Fact]
    public void Defaults_AreCorrect()
    {
        var vm = new SetupWizardViewModel(() => { }, () => { }, _ => { });
        Assert.Equal(string.Empty, vm.ApplicationName);
        Assert.Equal("Item", vm.ItemLabelSingular);
        Assert.Equal("Items", vm.ItemLabelPlural);
        Assert.False(vm.IsRandom);
    }

    [Fact]
    public void GenerateCommand_SetsPasswordAndFlags()
    {
        string? generated = null;
        var vm = new SetupWizardViewModel(() => { }, () => { }, s => generated = s);
        vm.GenerateCommand.Execute(null);
        Assert.True(vm.IsRandom);
        Assert.Equal(generated, vm.NewPassword);
        Assert.Equal(generated, vm.ConfirmPassword);
    }
}
