using ToolManagementAppV2.ViewModels;
using Xunit;

namespace ToolManagementAppV2.Tests.ViewModels
{
    public class PasswordPromptViewModelTests
    {
        [Fact]
        public void OkCommand_Succeeds_WhenPasswordValid()
        {
            bool success = false;
            string? error = null;
            var vm = new PasswordPromptViewModel(() => success = true, () => { }, m => error = m)
            {
                ValidatePassword = p => p == "secret"
            };
            vm.EnteredPassword = "secret";

            vm.OkCommand.Execute(null);

            Assert.True(success);
            Assert.Null(error);
        }

        [Fact]
        public void OkCommand_ShowsError_WhenPasswordInvalid()
        {
            bool success = false;
            string? error = null;
            var vm = new PasswordPromptViewModel(() => success = true, () => { }, m => error = m)
            {
                ValidatePassword = p => p == "secret"
            };
            vm.EnteredPassword = "wrong";

            vm.OkCommand.Execute(null);

            Assert.False(success);
            Assert.Equal("Incorrect password. Please try again.", error);
        }
    }
}
