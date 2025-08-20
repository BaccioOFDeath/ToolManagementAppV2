using System;
using System.Linq;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.ViewModels.Rental;
using Xunit;

namespace ToolManagementAppV2.Tests.ViewModels
{
    public class RentItemPopupViewModelTests
    {
        [Fact]
        public void CheckOutCommand_SetsResultsAndRaisesClose()
        {
            var item = new ItemModel();
            var customer = new CustomerModel { CustomerID = 1, Company = "ACME" };
            var vm = new RentItemPopupViewModel(item, new[] { customer })
            {
                SelectedCustomer = customer,
                SelectedDueDate = DateTime.Today.AddDays(3)
            };

            bool closed = false;
            vm.RequestClose += (_, __) => closed = true;

            vm.CheckOutCommand.Execute(null);

            Assert.True(closed);
            Assert.Equal(customer, vm.SelectedCustomerResult);
            Assert.Equal(vm.SelectedDueDate, vm.SelectedDueDateResult);
        }
    }
}

