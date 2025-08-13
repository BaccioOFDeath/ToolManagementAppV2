using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Data.SQLite;
using ToolManagementAppV2.Models;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Models.ImportExport;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Customers;
using ToolManagementAppV2.Services.Rentals;
using ToolManagementAppV2.Services.Tools;
using ToolManagementAppV2.Interfaces;
using Xunit;

namespace ToolManagementAppV2.Tests.Services
{
    public class RentalServiceTests
    {
        [Fact]
        public void GetRentalHistoryForTool_ReturnsHistory()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                IToolService toolService = new ToolService(db);
                ICustomerService customerService = new CustomerService(db);
                IRentalService rentalService = new RentalService(db, toolService);

                var tool = new Tool { ToolNumber = "T1", NameDescription = "Hammer", QuantityOnHand = 5 };
                toolService.AddTool(tool);
                var addedTool = toolService.GetAllTools().First();

                var cust = new Customer { Company = "Acme" };
                customerService.AddCustomer(cust);
                var addedCust = customerService.GetAllCustomers().First();

                rentalService.RentTool(addedTool.ToolID, addedCust.CustomerID, DateTime.Today, DateTime.Today.AddDays(1));
                rentalService.RentTool(addedTool.ToolID, addedCust.CustomerID, DateTime.Today.AddDays(2), DateTime.Today.AddDays(3));

                var history = rentalService.GetRentalHistoryForTool(addedTool.ToolID);
                Assert.Equal(2, history.Count);
                Assert.True(history[0].RentalDate > history[1].RentalDate);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void RentTool_NoAvailability_Throws()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                IToolService toolService = new ToolService(db);
                ICustomerService customerService = new CustomerService(db);
                IRentalService rentalService = new RentalService(db, toolService);

                var tool = new Tool { ToolNumber = "T1", NameDescription = "Hammer", QuantityOnHand = 0 };
                toolService.AddTool(tool);
                var addedTool = toolService.GetAllTools().First();

                customerService.AddCustomer(new Customer { Company = "Acme" });
                var cust = customerService.GetAllCustomers().First();

                Assert.Throws<InvalidOperationException>(() =>
                    rentalService.RentTool(addedTool.ToolID, cust.CustomerID, DateTime.Today, DateTime.Today.AddDays(1)));
                Assert.Empty(rentalService.GetAllRentals());
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void RentTool_NoAvailability_Throws_WithHelper()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                IToolService toolService = new ToolService(db);
                ICustomerService customerService = new CustomerService(db);
                IRentalService rentalService = new RentalService(db, toolService);

                var tool = new Tool { ToolNumber = "T2", NameDescription = "Wrench", QuantityOnHand = 0 };
                toolService.AddTool(tool);
                var addedTool = toolService.GetAllTools().First();

                customerService.AddCustomer(new Customer { Company = "Beta" });
                var cust = customerService.GetAllCustomers().First();

                Assert.Throws<InvalidOperationException>(() =>
                    rentalService.RentTool(addedTool.ToolID, cust.CustomerID, DateTime.Today, DateTime.Today.AddDays(1)));
                Assert.Empty(rentalService.GetAllRentals());
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void ReturnTool_InvalidRentalID_Throws()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                var toolService = new ToolService(db);
                IRentalService rentalService = new RentalService(db, toolService);

                Assert.Throws<InvalidOperationException>(() => rentalService.ReturnTool(1, DateTime.Today));
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void ReturnTool_InvalidRentalID_Throws_WithHelper()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                var toolService = new ToolService(db);
                IRentalService rentalService = new RentalService(db, toolService);

                Assert.Throws<InvalidOperationException>(() => rentalService.ReturnTool(1, DateTime.Today));
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void ExtendRental_InvalidRentalID_Throws()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                var toolService = new ToolService(db);
                var rentalService = new RentalService(db, toolService);

                Assert.Throws<InvalidOperationException>(() =>
                    rentalService.ExtendRental(1, DateTime.Today.AddDays(1)));
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void ExtendRental_ReturnedRental_Throws()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                var toolService = new ToolService(db);
                var customerService = new CustomerService(db);
                var rentalService = new RentalService(db, toolService);

                var tool = new Tool { ToolNumber = "T1", NameDescription = "Hammer", QuantityOnHand = 1 };
                toolService.AddTool(tool);
                var addedTool = toolService.GetAllTools().First();

                customerService.AddCustomer(new Customer { Company = "Acme" });
                var cust = customerService.GetAllCustomers().First();

                rentalService.RentTool(addedTool.ToolID, cust.CustomerID, DateTime.Today, DateTime.Today.AddDays(1));
                var rental = rentalService.GetAllRentals().First();

                rentalService.ReturnTool(rental.RentalID, DateTime.Today);

                Assert.Throws<InvalidOperationException>(() =>
                    rentalService.ExtendRental(rental.RentalID, DateTime.Today.AddDays(2)));
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void DeleteRental_RemovesRecord()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                var toolService = new ToolService(db);
                var customerService = new CustomerService(db);
                var rentalService = new RentalService(db, toolService);

                var tool = new Tool { ToolNumber = "T1", NameDescription = "Hammer", QuantityOnHand = 1 };
                toolService.AddTool(tool);
                var addedTool = toolService.GetAllTools().First();

                customerService.AddCustomer(new Customer { Company = "Acme" });
                var cust = customerService.GetAllCustomers().First();

                rentalService.RentTool(addedTool.ToolID, cust.CustomerID, DateTime.Today, DateTime.Today.AddDays(1));
                var rental = rentalService.GetAllRentals().First();

                rentalService.DeleteRental(rental.RentalID);

                Assert.Empty(rentalService.GetAllRentals());
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void DeleteRental_InvalidRentalID_Throws()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                var toolService = new ToolService(db);
                var rentalService = new RentalService(db, toolService);

                Assert.Throws<InvalidOperationException>(() => rentalService.DeleteRental(1));
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void GetActiveRentals_ReturnsCustomerAndToolDetails()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                var toolService = new ToolService(db);
                var customerService = new CustomerService(db);
                var rentalService = new RentalService(db, toolService);

                var tool = new Tool { ToolNumber = "T1", NameDescription = "Hammer", QuantityOnHand = 1, Location = "A1", ToolImagePath = "path" };
                toolService.AddTool(tool);
                var addedTool = toolService.GetAllTools().First();

                var customer = new Customer { Company = "Acme", Contact = "Bob", Email = "b@c.com", Phone = "111", Mobile = "222", Address = "Addr" };
                customerService.AddCustomer(customer);
                var addedCust = customerService.GetAllCustomers().First();

                rentalService.RentTool(addedTool.ToolID, addedCust.CustomerID, DateTime.Today, DateTime.Today.AddDays(1));

                var rentals = rentalService.GetActiveRentals();
                var r = rentals.First();
                Assert.Equal("A1", r.ToolLocation);
                Assert.Equal("path", r.ToolImagePath);
                Assert.Equal("Bob", r.CustomerContact);
                Assert.Equal("b@c.com", r.CustomerEmail);
                Assert.Equal("111", r.CustomerPhone);
                Assert.Equal("222", r.CustomerMobile);
                Assert.Equal("Addr", r.CustomerAddress);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void RentAndReturnTool_UpdatesQuantities()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                var toolService = new ToolService(db);
                var customerService = new CustomerService(db);
                var rentalService = new RentalService(db, toolService);

                toolService.AddTool(new Tool { ToolNumber = "T1", NameDescription = "Hammer", QuantityOnHand = 1 });
                var tool = toolService.GetAllTools().First();

                customerService.AddCustomer(new Customer { Company = "Acme" });
                var cust = customerService.GetAllCustomers().First();

                rentalService.RentTool(tool.ToolID, cust.CustomerID, DateTime.Today, DateTime.Today.AddDays(1));

                var rented = toolService.GetToolByID(tool.ToolID);
                Assert.Equal(0, rented.QuantityOnHand);
                Assert.Equal(1, rented.RentedQuantity);

                var rental = rentalService.GetAllRentals().First();
                rentalService.ReturnTool(rental.RentalID, DateTime.Today);

                var returned = toolService.GetToolByID(tool.ToolID);
                Assert.Equal(1, returned.QuantityOnHand);
                Assert.Equal(0, returned.RentedQuantity);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void ExtendRental_Failure_RollsBack()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                var toolService = new ToolService(db);
                var customerService = new CustomerService(db);
                var rentalService = new RentalService(db, toolService);

                toolService.AddTool(new Tool { ToolNumber = "T1", NameDescription = "Hammer", QuantityOnHand = 1 });
                var tool = toolService.GetAllTools().First();

                customerService.AddCustomer(new Customer { Company = "Acme" });
                var cust = customerService.GetAllCustomers().First();

                rentalService.RentTool(tool.ToolID, cust.CustomerID, DateTime.Today.AddDays(-2), DateTime.Today.AddDays(-1));
                var rental = rentalService.GetAllRentals().First();
                var originalDue = rental.DueDate;

                var failingToolService = new FailingToolService();
                var rentalService2 = new RentalService(db, failingToolService);

                Assert.Throws<InvalidOperationException>(() =>
                    rentalService2.ExtendRental(rental.RentalID, DateTime.Today.AddDays(1)));

                var after = rentalService2.GetAllRentals().First();
                Assert.Equal(originalDue, after.DueDate);

                var toolAfter = toolService.GetToolByID(tool.ToolID);
                Assert.Equal(0, toolAfter.QuantityOnHand);
                Assert.Equal(1, toolAfter.RentedQuantity);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        class FailingToolService : IToolService
        {
            public List<int> ImportToolsFromCsv(string filePath, IDictionary<string, string> map) => throw new NotImplementedException();
            public void ExportToolsToCsv(string filePath) => throw new NotImplementedException();
            public List<ToolModel> GetAllTools() => new();
            public void AddTool(ToolModel tool) => throw new NotImplementedException();
            public void UpdateTool(ToolModel tool) => throw new NotImplementedException();
            public void DeleteTool(int toolID) => throw new NotImplementedException();
            public ToolModel GetToolByID(int toolID) => throw new NotImplementedException();
            public List<ToolModel> SearchTools(string? searchText) => new();
            public void ToggleToolCheckOutStatus(int toolID, string currentUser) => throw new NotImplementedException();
            public List<ToolModel> GetToolsCheckedOutBy(string userName) => new();
            public void UpdateToolImage(int toolID, string imagePath) => throw new NotImplementedException();
            public ImageImportResult ImportToolImages(string folderPath, Func<ToolModel, IEnumerable<string>> keySelector) => new();
            public void UpdateToolQuantities(int toolID, int qtyChange, bool isRental, SQLiteConnection? conn = null, SQLiteTransaction? tx = null)
                => throw new InvalidOperationException("fail");
        }
    }
}
