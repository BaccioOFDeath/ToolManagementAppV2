using System.IO;
using System.Linq;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Customers;
using ToolManagementAppV2.Services.Rentals;
using ToolManagementAppV2.Services.Settings;
using ToolManagementAppV2.Services.Tools;
using ToolManagementAppV2.Services.Users;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.ViewModels;
using Xunit;

namespace ToolManagementAppV2.Tests.ViewModels
{
    public class MainViewModelTests
    {
        [Fact]
        public void SearchCommand_FiltersToolsBySearchTerm()
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

                toolService.AddTool(new Tool { ToolNumber = "T1", NameDescription = "Hammer" });
                toolService.AddTool(new Tool { ToolNumber = "T2", NameDescription = "Saw" });

                var vm = new MainViewModel(toolService, userService, customerService, rentalService, settingsService);

                vm.SearchTerm = "Ham";
                vm.SearchCommand.Execute(null);

                Assert.Single(vm.SearchResults);
                Assert.Equal("Hammer", vm.SearchResults.First().NameDescription);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void SearchCommand_EmptyTerm_ReturnsAllTools()
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

                toolService.AddTool(new Tool { ToolNumber = "T1", NameDescription = "Hammer" });
                toolService.AddTool(new Tool { ToolNumber = "T2", NameDescription = "Saw" });

                var vm = new MainViewModel(toolService, userService, customerService, rentalService, settingsService);

                vm.SearchTerm = string.Empty;
                vm.SearchCommand.Execute(null);

                Assert.Equal(2, vm.SearchResults.Count);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void ChooseProfilePicCommand_SetsAvatarPath()
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

                userService.AddUser(new User { UserName = "u", Password = "p", IsAdmin = true });
                var user = userService.GetAllUsers().First();

                var avatarDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Avatars");
                var avatar = Directory.GetFiles(avatarDir, "*.png").First();

                var vm = new TestMainViewModel(toolService, userService, customerService, rentalService, settingsService, avatar);

                Application.Current.Properties["CurrentUser"] = user;
                vm.ChooseProfilePicCommand.Execute(null);

                var updated = userService.GetAllUsers().First();
                Assert.StartsWith("Resources/Avatars/", updated.UserPhotoPath.Replace("\\", "/"));
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        class TestMainViewModel : MainViewModel
        {
            readonly string _avatar;
            public TestMainViewModel(IToolService t, IUserService u, ICustomerService c, IRentalService r, ISettingsService s, string avatar)
                : base(t, u, c, r, s) => _avatar = avatar;
            protected override string? SelectAvatar() => _avatar;
        }
    }
}
