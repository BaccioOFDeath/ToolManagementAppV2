using System.IO;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Tools;
using ToolManagementAppV2.Services.Customers;
using ToolManagementAppV2.Services.Rentals;
using ToolManagementAppV2.Services.Users;
using ToolManagementAppV2.Services.Settings;
using Xunit;

namespace ToolManagementAppV2.Tests.Services
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
                IToolService toolSvc = new ToolService(db);
                ICustomerService custSvc = new CustomerService(db);
                IRentalService rentalSvc = new RentalService(db);
                IUserService userSvc = new UserService(db);
                ISettingsService settingsSvc = new SettingsService(db);

                Assert.NotNull(toolSvc);
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
