using InventoryManagementApp.ViewModels;
using Xunit;

public class ChangePasswordViewModelTests
{
    [Fact]
    public void SaveCommand_ShowsError_OnMismatch()
    {
        bool saved = false;
        var vm = new ChangePasswordViewModel(() => saved = true, () => { });
        vm.NewPassword = "abc";
        vm.ConfirmPassword = "xyz";
        vm.SaveCommand.Execute(null);
        Assert.False(saved);
        Assert.Equal("Passwords do not match.", vm.ValidationMessage);
    }

    [Fact]
    public void SaveCommand_CallsSave_WhenValid()
    {
        bool saved = false;
        var vm = new ChangePasswordViewModel(() => saved = true, () => { });
        vm.NewPassword = "simple";
        vm.ConfirmPassword = "simple";
        vm.SaveCommand.Execute(null);
        Assert.True(saved);
        Assert.True(string.IsNullOrEmpty(vm.ValidationMessage));
    }

    [Fact]
    public void SaveCommand_ShowsError_ForEmptyPassword()
    {
        bool saved = false;
        var vm = new ChangePasswordViewModel(() => saved = true, () => { });
        vm.NewPassword = string.Empty;
        vm.ConfirmPassword = string.Empty;
        vm.SaveCommand.Execute(null);
        Assert.False(saved);
        Assert.Equal("Password cannot be empty.", vm.ValidationMessage);
    }
}
