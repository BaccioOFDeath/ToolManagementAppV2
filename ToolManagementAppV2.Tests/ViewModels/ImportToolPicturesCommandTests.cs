using System;
using System.IO;
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
using System.Linq;

namespace ToolManagementAppV2.Tests.ViewModels
{
    class TestToolService : ToolService
    {
        public bool ImportCalled { get; private set; }
        public TestToolService(DatabaseService db) : base(db) { }
        public override ImageImportResult ImportToolImages(string folderPath, Func<ToolModel, string> keySelector)
        {
            ImportCalled = true;
            return new ImageImportResult();
        }
    }

    class TestMainViewModel : MainViewModel
    {
        readonly string _folder;
        public TestMainViewModel(IToolService t, IUserService u, ICustomerService c, IRentalService r, ISettingsService s, ActivityLogService a, string folder)
            : base(t, u, c, r, s, a)
        {
            _folder = folder;
        }

        protected override bool ShowFolderDialog(out string folder) { folder = _folder; return true; }
        protected override bool ShowImageImportOptions(out Func<ToolModel, string> selector) { selector = t => t.ToolNumber; return true; }
        protected override void ShowInfo(string msg) { }
    }

    public class ImportToolPicturesCommandTests
    {
        [Fact]
        public void CommandCallsService()
        {
            var dbPath = Path.GetTempFileName();
            var imgDir = Path.GetTempPath();
            try
            {
                var db = new DatabaseService(dbPath);
                var toolService = new TestToolService(db);
                IUserService userService = new UserService(db);
                ICustomerService custService = new CustomerService(db);
                IRentalService rentalService = new RentalService(db, toolService);
                ISettingsService settingsService = new SettingsService(db);
                ActivityLogService logService = new ActivityLogService(db);
                var vm = new TestMainViewModel(toolService, userService, custService, rentalService, settingsService, logService, imgDir);
                vm.ImportToolPicturesCommand.Execute(null);
                Assert.True(toolService.ImportCalled);
            }
            finally
            {
                if (File.Exists(dbPath)) File.Delete(dbPath);
            }
        }
    }
}
