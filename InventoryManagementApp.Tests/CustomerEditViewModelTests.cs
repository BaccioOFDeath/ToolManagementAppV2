using InventoryManagementApp.ViewModels;
using Xunit;

namespace InventoryManagementApp.Tests;

public class CustomerEditViewModelTests
{
    [Fact]
    public void SaveCommand_WithMissingContact_ShowsValidationAndDoesNotSave()
    {
        var saved = false;
        var customer = new CustomerModel
        {
            Company = "Pickerill Automotive And Tyres",
            Phone = "078472255"
        };
        var vm = new CustomerEditViewModel(customer, () => saved = true, () => { });

        vm.SaveCommand.Execute(null);

        Assert.False(saved);
        Assert.Contains("Contact is required", vm.StatusMessage);
    }

    [Fact]
    public void SaveCommand_WithRequiredFields_TrimsAndSaves()
    {
        var saved = false;
        var customer = new CustomerModel
        {
            Company = " Pickerill Automotive And Tyres ",
            Contact = " Service Desk ",
            Phone = " 078472255 ",
            Email = " garett@sdeuropean.co.nz ",
            Address = " 1 Norton Road "
        };
        var vm = new CustomerEditViewModel(customer, () => saved = true, () => { });

        vm.SaveCommand.Execute(null);

        Assert.True(saved);
        Assert.Equal("Pickerill Automotive And Tyres", customer.Company);
        Assert.Equal("Service Desk", customer.Contact);
        Assert.Equal("078472255", customer.Phone);
        Assert.Equal("garett@sdeuropean.co.nz", customer.Email);
        Assert.Equal("1 Norton Road", customer.Address);
    }
}
