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
        vm.SelectedReport = "Unknown Report";

        await vm.RunReportCommand.ExecuteAsync(null);

        Assert.Empty(vm.ReportLines);
        Assert.Equal("Unknown Report", vm.ReportTitle);
        Assert.Equal("The report returned no detail rows.", vm.ReportSummary);
        Assert.Equal("Unknown Report completed with no rows to action.", vm.ReportStatus);
    }

    [Fact]
    public void ReportsViewModel_ReportResults_AliasesReportLines()
    {
        var vm = new ReportsViewModel(new ReportService(null!, null!, null!, null!, null!));

        Assert.Same(vm.ReportLines, vm.ReportResults);
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
