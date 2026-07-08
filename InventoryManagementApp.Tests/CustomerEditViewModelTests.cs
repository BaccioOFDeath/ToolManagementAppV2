using InventoryManagementApp.ViewModels;
using Xunit;

namespace InventoryManagementApp.Tests;

public class CustomerEditViewModelTests
{
    [Fact]
    public void SaveCommand_WithMissingContact_IsDisabledAndShowsReadiness()
    {
        var saved = false;
        var customer = new CustomerModel
        {
            Company = "Pickerill Automotive And Tyres",
            Phone = "078472255"
        };
        var vm = new CustomerEditViewModel(customer, () => saved = true, () => { });

        Assert.False(vm.SaveCommand.CanExecute(null));
        vm.SaveCommand.Execute(null);

        Assert.False(saved);
        Assert.Contains("Contact is required", vm.StatusMessage);
        Assert.True(vm.HasValidationMessage);
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

        Assert.True(vm.SaveCommand.CanExecute(null));
        vm.SaveCommand.Execute(null);

        Assert.True(saved);
        Assert.Equal("Pickerill Automotive And Tyres", customer.Company);
        Assert.Equal("Service Desk", customer.Contact);
        Assert.Equal("078472255", customer.Phone);
        Assert.Equal("garett@sdeuropean.co.nz", customer.Email);
        Assert.Equal("1 Norton Road", customer.Address);
        Assert.False(vm.IsSaving);
        Assert.Contains("ready", vm.StatusMessage);
    }

    [Fact]
    public void SaveCommand_ReevaluatesWhenRequiredFieldsChange()
    {
        var customer = new CustomerModel();
        var vm = new CustomerEditViewModel(customer, () => { }, () => { });

        Assert.False(vm.SaveCommand.CanExecute(null));
        Assert.Contains("company", vm.SaveReadinessText);

        customer.Company = "Pickerill Automotive And Tyres";
        Assert.False(vm.SaveCommand.CanExecute(null));
        Assert.Contains("primary contact", vm.SaveReadinessText);

        customer.Contact = "Service Desk";
        Assert.False(vm.SaveCommand.CanExecute(null));
        Assert.Contains("phone or mobile", vm.SaveReadinessText);

        customer.Mobile = "0215550101";
        Assert.True(vm.SaveCommand.CanExecute(null));
        Assert.Contains("ready", vm.SaveReadinessText);
    }

    [Fact]
    public void SaveCommand_DisablesSaveAndCancelWhileSaveCallbackRuns()
    {
        CustomerEditViewModel? vm = null;
        var wasSavingInsideCallback = false;
        var couldSaveInsideCallback = true;
        var couldCancelInsideCallback = true;
        var customer = new CustomerModel
        {
            Company = "Pickerill Automotive And Tyres",
            Contact = "Service Desk",
            Phone = "078472255"
        };

        vm = new CustomerEditViewModel(customer, () =>
        {
            wasSavingInsideCallback = vm!.IsSaving;
            couldSaveInsideCallback = vm.SaveCommand.CanExecute(null);
            couldCancelInsideCallback = vm.CancelCommand.CanExecute(null);
        }, () => { });

        vm.SaveCommand.Execute(null);

        Assert.True(wasSavingInsideCallback);
        Assert.False(couldSaveInsideCallback);
        Assert.False(couldCancelInsideCallback);
        Assert.False(vm.IsSaving);
        Assert.True(vm.SaveCommand.CanExecute(null));
        Assert.True(vm.CancelCommand.CanExecute(null));
    }

    [Fact]
    public void CustomerFieldChangeRefreshesReadinessAndCommandState()
    {
        var customer = new CustomerModel
        {
            Company = "Pickerill Automotive And Tyres",
            Contact = "Service Desk"
        };
        var vm = new CustomerEditViewModel(customer, () => { }, () => { });

        Assert.False(vm.SaveCommand.CanExecute(null));
        Assert.Contains("phone or mobile", vm.StatusMessage);

        customer.Phone = "078472255";

        Assert.False(vm.HasValidationMessage);
        Assert.True(vm.SaveCommand.CanExecute(null));
        Assert.Contains("ready", vm.StatusMessage);
    }
}
