using System.Collections.Generic;
using System.Linq;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.ViewModels.Rental;
using Xunit;

namespace ToolManagementAppV2.Tests.ViewModels
{
    public class RentalHistoryViewModelTests
    {
        [Fact]
        public void Constructor_SetsDisplayNameAndHistory()
        {
            var tool = new Tool { ToolNumber = "T1", NameDescription = "Hammer" };
            var rentals = new List<Rental>
            {
                new Rental { RentalID = 1, ToolID = tool.ToolID, CustomerID = 1 },
                new Rental { RentalID = 2, ToolID = tool.ToolID, CustomerID = 2 }
            };

            var vm = new RentalHistoryViewModel(tool, rentals);

            Assert.Equal("T1 - Hammer", vm.ToolDisplayName);
            Assert.Equal(2, vm.History.Count);
            Assert.Equal(rentals.First().RentalID, vm.History[0].RentalID);
        }
    }
}
