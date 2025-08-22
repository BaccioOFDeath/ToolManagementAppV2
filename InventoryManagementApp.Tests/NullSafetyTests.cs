using System.Threading.Tasks;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.ViewModels;
using InventoryManagementApp.Services.Items;
using Xunit;

public class NullSafetyTests
{
    [Fact]
    public void SettingItem_DefaultsToEmptyStrings()
    {
        var item = new SettingItem();
        Assert.Equal(string.Empty, item.Key);
        Assert.Equal(string.Empty, item.Value);
    }

    [Fact]
    public void User_DefaultsToEmptyStrings()
    {
        var user = new User();
        Assert.Equal(string.Empty, user.UserName);
        Assert.Equal(string.Empty, user.PasswordHash);
    }

    [Fact]
    public async Task ReportsViewModel_RunReportCommand_SafeWithUnknownReport()
    {
        var vm = new ReportsViewModel(new ReportService(null!, null!, null!, null!, null!));
        await vm.RunReportCommand.ExecuteAsync(null);
        Assert.NotNull(vm.ReportResults);
        Assert.True(vm.ReportResults.Columns.Contains("Line"));
    }

    [Fact]
    public void CustomerManagementViewModel_InitializesFields()
    {
        var vm = new CustomerManagementViewModel(null!, null!);
        Assert.Equal(string.Empty, vm.NewCustomerName);
        Assert.Equal(string.Empty, vm.NewCustomerEmail);
        Assert.Null(vm.SelectedCustomer);
    }
}
