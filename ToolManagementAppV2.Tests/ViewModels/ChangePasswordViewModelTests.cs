using ToolManagementAppV2.ViewModels;
using Xunit;

public class ChangePasswordViewModelTests
{
    [Fact]
    public void SaveCommand_ShowsError_OnMismatch()
    {
        bool saved = false;
        var vm = new ChangePasswordViewModel(() => saved = true, () => { });
        vm.NewPassword = "Valid1!";
        vm.ConfirmPassword = "Other1!";
        vm.SaveCommand.Execute(null);
        Assert.False(saved);
        Assert.Equal("Passwords do not match.", vm.ValidationMessage);
    }

    [Fact]
    public void SaveCommand_CallsSave_WhenValid()
    {
        bool saved = false;
        var vm = new ChangePasswordViewModel(() => saved = true, () => { });
        vm.NewPassword = "Valid1!";
        vm.ConfirmPassword = "Valid1!";
        vm.SaveCommand.Execute(null);
        Assert.True(saved);
        Assert.True(string.IsNullOrEmpty(vm.ValidationMessage));
    }

    [Fact]
    public void SaveCommand_ShowsError_ForWeakPassword()
    {
        bool saved = false;
        var vm = new ChangePasswordViewModel(() => saved = true, () => { });
        vm.NewPassword = "short";
        vm.ConfirmPassword = "short";
        vm.SaveCommand.Execute(null);
        Assert.False(saved);
        Assert.False(string.IsNullOrEmpty(vm.ValidationMessage));
    }
}
