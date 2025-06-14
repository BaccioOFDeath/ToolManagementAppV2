using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Windows.Controls;
using ToolManagementAppV2;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Users;
using ToolManagementAppV2.Services.Tools;
using ToolManagementAppV2.Services.Customers;
using ToolManagementAppV2.Services.Rentals;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.Utilities.Converters;
using ToolManagementAppV2.Utilities.Helpers;
using ToolManagementAppV2.Models.Domain;
using Xunit;
using System.Linq;

namespace ToolManagementAppV2.Tests
{
    public class ConsoleLoggingTests
    {
        [Fact]
        public void PathHelper_InvalidPath_LogsException()
        {
            var sw = new StringWriter();
            var original = Console.Out;
            Console.SetOut(sw);
            try
            {
                var result = PathHelper.GetAbsolutePath("invalid|path");
                Assert.Null(result);
            }
            finally
            {
                Console.SetOut(original);
            }
            Assert.NotEqual(string.Empty, sw.ToString());
        }

        [Fact]
        public void NullToDefaultImageConverter_InvalidResource_LogsException()
        {
            var converter = new NullToDefaultImageConverter();
            var method = typeof(NullToDefaultImageConverter).GetMethod("LoadFromResource", BindingFlags.NonPublic | BindingFlags.Instance);
            var sw = new StringWriter();
            var original = Console.Out;
            Console.SetOut(sw);
            try
            {
                method.Invoke(converter, new object[] { "NoSuchFile.png" });
            }
            finally
            {
                Console.SetOut(original);
            }
            Assert.NotEqual(string.Empty, sw.ToString());
        }

        [Fact]
        public void ShowError_LogsException()
        {
            var window = (MainWindow)FormatterServices.GetUninitializedObject(typeof(MainWindow));
            var method = typeof(MainWindow).GetMethod("ShowError", BindingFlags.NonPublic | BindingFlags.Instance);
            var sw = new StringWriter();
            var original = Console.Out;
            Console.SetOut(sw);
            try
            {
                method.Invoke(window, new object[] { "Error", new Exception("fail") });
            }
            finally
            {
                Console.SetOut(original);
            }
            Assert.Contains("fail", sw.ToString());
        }

        [Fact]
        public void RefreshUserList_InvalidPhoto_LogsException()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                IUserService userService = new UserService(db);
                userService.AddUser(new User { UserName = "u", Password = "p", UserPhotoPath = "pack://application:,,,/Resources/NoFile.png" });
                var window = (MainWindow)FormatterServices.GetUninitializedObject(typeof(MainWindow));
                var list = new ListView();
                typeof(MainWindow).GetField("_userService", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(window, userService);
                typeof(MainWindow).GetField("UserList", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(window, list);

                var sw = new StringWriter();
                var original = Console.Out;
                Console.SetOut(sw);
                try
                {
                    typeof(MainWindow).GetMethod("RefreshUserList", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(window, null);
                }
                finally
                {
                    Console.SetOut(original);
                }
                Assert.NotEqual(string.Empty, sw.ToString());
            }
            finally
            {
                if (File.Exists(dbPath)) File.Delete(dbPath);
            }
        }

        [Fact]
        public void BackupDatabase_LogsException()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                var sw = new StringWriter();
                var original = Console.Out;
                Console.SetOut(sw);
                Assert.Throws<IOException>(() => db.BackupDatabase("invalid|path.db"));
                Console.SetOut(original);
                Assert.NotEqual(string.Empty, sw.ToString());
            }
            finally
            {
                if (File.Exists(dbPath)) File.Delete(dbPath);
            }
        }

        [Fact]
        public void RentTool_LogsException()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                IToolService toolService = new ToolService(db);
                ICustomerService customerService = new CustomerService(db);
                IRentalService rentalService = new RentalService(db);

                toolService.AddTool(new Tool { ToolNumber = "T1", NameDescription = "Hammer", QuantityOnHand = 0 });
                var tool = toolService.GetAllTools().First();
                customerService.AddCustomer(new Customer { Company = "Acme" });
                var cust = customerService.GetAllCustomers().First();

                var sw = new StringWriter();
                var original = Console.Out;
                Console.SetOut(sw);
                rentalService.RentTool(tool.ToolID, cust.CustomerID, DateTime.Today, DateTime.Today.AddDays(1));
                Console.SetOut(original);
                Assert.NotEqual(string.Empty, sw.ToString());
            }
            finally
            {
                if (File.Exists(dbPath)) File.Delete(dbPath);
            }
        }

        [Fact]
        public void RentToolWithTransaction_LogsException()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                IToolService toolService = new ToolService(db);
                ICustomerService customerService = new CustomerService(db);
                IRentalService rentalService = new RentalService(db);

                toolService.AddTool(new Tool { ToolNumber = "T2", NameDescription = "Wrench", QuantityOnHand = 0 });
                var tool = toolService.GetAllTools().First();
                customerService.AddCustomer(new Customer { Company = "Beta" });
                var cust = customerService.GetAllCustomers().First();

                var sw = new StringWriter();
                var original = Console.Out;
                Console.SetOut(sw);
                rentalService.RentToolWithTransaction(tool.ToolID, cust.CustomerID, DateTime.Today, DateTime.Today.AddDays(1));
                Console.SetOut(original);
                Assert.NotEqual(string.Empty, sw.ToString());
            }
            finally
            {
                if (File.Exists(dbPath)) File.Delete(dbPath);
            }
        }

        [Fact]
        public void ReturnTool_LogsException()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                IRentalService rentalService = new RentalService(db);

                var sw = new StringWriter();
                var original = Console.Out;
                Console.SetOut(sw);
                rentalService.ReturnTool(1, DateTime.Today);
                Console.SetOut(original);
                Assert.NotEqual(string.Empty, sw.ToString());
            }
            finally
            {
                if (File.Exists(dbPath)) File.Delete(dbPath);
            }
        }

        [Fact]
        public void ReturnToolWithTransaction_LogsException()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                IRentalService rentalService = new RentalService(db);

                var sw = new StringWriter();
                var original = Console.Out;
                Console.SetOut(sw);
                rentalService.ReturnToolWithTransaction(1, DateTime.Today);
                Console.SetOut(original);
                Assert.NotEqual(string.Empty, sw.ToString());
            }
            finally
            {
                if (File.Exists(dbPath)) File.Delete(dbPath);
            }
        }
    }
}
