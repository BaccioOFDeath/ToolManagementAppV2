using System.IO;
using System.Linq;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Customers;
using ToolManagementAppV2.Services.Rentals;
using ToolManagementAppV2.Services.Settings;
using ToolManagementAppV2.Services.Tools;
using ToolManagementAppV2.Services.Users;
using ToolManagementAppV2.ViewModels;
using ToolManagementAppV2.Interfaces;
using Xunit;
using System;

namespace ToolManagementAppV2.Tests.ViewModels
{
    public class AvatarSelectionTests
    {
        [Fact]
        public void ApplyAvatar_StoresRelativePath()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                IToolService toolService = new ToolService(db);
                IUserService userService = new UserService(db);
                ICustomerService customerService = new CustomerService(db);
                IRentalService rentalService = new RentalService(db);
                ISettingsService settingsService = new SettingsService(db);

                var user = new User { UserName = "u", Password = "p" };
                userService.AddUser(user);
                var added = userService.GetAllUsers().First();

                var vm = new MainViewModel(toolService, userService, customerService, rentalService, settingsService);
                var avatar = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Avatars", "1.png");

                vm.ApplyAvatar(added, avatar);

                var updated = userService.GetUserByID(added.UserID);
                Assert.Equal(Path.Combine("Resources", "Avatars", "1.png"), updated.UserPhotoPath);
                Assert.NotNull(updated.PhotoBitmap);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }
    }
}
