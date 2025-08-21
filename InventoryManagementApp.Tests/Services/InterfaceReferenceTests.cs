using System.IO;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Services.Core;
using InventoryManagementApp.Services.Items;
using InventoryManagementApp.Services.Customers;
using InventoryManagementApp.Services.Rentals;
using InventoryManagementApp.Services.Users;
using InventoryManagementApp.Services.Settings;
using Xunit;

namespace InventoryManagementApp.Tests.Services
{
    public class InterfaceReferenceTests
    {
        [Fact]
        public void Services_CanBeReferenced_ByInterface()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                IItemService itemService = new ItemService(db);
                ICustomerService custSvc = new CustomerService(db);
                IRentalService rentalSvc = new RentalService(db, itemService);
                IUserService userSvc = new UserService(db, new ApplicationUserContext());
                ISettingsService settingsSvc = new SettingsService(db);

                Assert.NotNull(itemService);
                Assert.NotNull(custSvc);
                Assert.NotNull(rentalSvc);
                Assert.NotNull(userSvc);
                Assert.NotNull(settingsSvc);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }
    }
}
