using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Services.ImportExport;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class CustomerImportExportTests : IDisposable
    {
        private readonly List<string> _tempFiles = new();

        public void Dispose()
        {
            foreach (var file in _tempFiles.Where(File.Exists))
            {
                try { File.Delete(file); } catch { /* ignore */ }
            }
        }

        private string CreateTempFile(string extension)
        {
            var path = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}{extension}");
            _tempFiles.Add(path);
            return path;
        }

        private List<Customer> GetSampleCustomers()
        {
            return new List<Customer>
            {
                new Customer
                {
                    CustomerID = 1,
                    Company = "Test Company 1",
                    Contact = "John Doe",
                    Email = "john@test.com",
                    Phone = "123-456-7890",
                    Mobile = "098-765-4321",
                    Address = "123 Test St"
                },
                new Customer
                {
                    CustomerID = 2,
                    Company = "Test Company 2",
                    Contact = "Jane Smith",
                    Email = "jane@test.com",
                    Phone = "555-555-5555",
                    Mobile = "444-444-4444",
                    Address = "456 Demo Ave"
                }
            };
        }

        [Fact]
        public async Task CustomerJsonExporter_ExportsAndImportsCorrectly()
        {
            // Arrange
            var customers = GetSampleCustomers();
            var exporter = new CustomerJsonExporter();
            var importer = new CustomerJsonImporter();
            var filePath = CreateTempFile(".json");

            // Act - Export
            await exporter.ExportAsync(filePath, customers, CancellationToken.None);

            // Assert - File exists
            Assert.True(File.Exists(filePath));

            // Act - Import
            var (importedCustomers, skippedRows) = await importer.ImportAsync(filePath, CancellationToken.None);
            var importedList = importedCustomers.ToList();

            // Assert - Data integrity
            Assert.Equal(2, importedList.Count);
            Assert.Empty(skippedRows);
            Assert.Equal("Test Company 1", importedList[0].Company);
            Assert.Equal("John Doe", importedList[0].Contact);
            Assert.Equal("Test Company 2", importedList[1].Company);
            Assert.Equal("Jane Smith", importedList[1].Contact);
        }

        [Fact]
        public async Task CustomerXmlExporter_ExportsAndImportsCorrectly()
        {
            // Arrange
            var customers = GetSampleCustomers();
            var exporter = new CustomerXmlExporter();
            var importer = new CustomerXmlImporter();
            var filePath = CreateTempFile(".xml");

            // Act - Export
            await exporter.ExportAsync(filePath, customers, CancellationToken.None);

            // Assert - File exists
            Assert.True(File.Exists(filePath));

            // Act - Import
            var (importedCustomers, skippedRows) = await importer.ImportAsync(filePath, CancellationToken.None);
            var importedList = importedCustomers.ToList();

            // Assert - Data integrity
            Assert.Equal(2, importedList.Count);
            Assert.Empty(skippedRows);
            Assert.Equal("Test Company 1", importedList[0].Company);
            Assert.Equal("John Doe", importedList[0].Contact);
        }

        [Fact]
        public async Task CustomerCsvExporter_ExportsCorrectly()
        {
            // Arrange
            var customers = GetSampleCustomers();
            var exporter = new CustomerCsvExporter();
            var filePath = CreateTempFile(".csv");

            // Act
            await exporter.ExportAsync(filePath, customers, CancellationToken.None);

            // Assert
            Assert.True(File.Exists(filePath));
            var content = await File.ReadAllTextAsync(filePath);
            Assert.Contains("Test Company 1", content);
            Assert.Contains("John Doe", content);
        }

        [Fact]
        public async Task CustomerJsonImporter_SkipsInvalidCustomers()
        {
            // Arrange
            var filePath = CreateTempFile(".json");
            var json = @"[
                {""Company"": ""Valid Company"", ""Contact"": ""Valid Contact""},
                {""Email"": ""only@email.com""},
                {""Company"": ""Another Valid"", ""Contact"": ""Another Contact""}
            ]";
            await File.WriteAllTextAsync(filePath, json);
            var importer = new CustomerJsonImporter();

            // Act
            var (importedCustomers, skippedRows) = await importer.ImportAsync(filePath, CancellationToken.None);
            var importedList = importedCustomers.ToList();

            // Assert
            Assert.Equal(2, importedList.Count);
            Assert.Single(skippedRows);
            Assert.Contains(2, skippedRows); // Second customer (index 1) should be skipped
        }

        [Fact]
        public void CustomerJsonExporter_HasCorrectProperties()
        {
            // Arrange
            var exporter = new CustomerJsonExporter();

            // Assert
            Assert.Equal(".json", exporter.FileExtension);
            Assert.Equal("JSON Files|*.json", exporter.FileFilter);
            Assert.Equal("JSON", exporter.FormatName);
        }

        [Fact]
        public void CustomerXmlExporter_HasCorrectProperties()
        {
            // Arrange
            var exporter = new CustomerXmlExporter();

            // Assert
            Assert.Equal(".xml", exporter.FileExtension);
            Assert.Equal("XML Files|*.xml", exporter.FileFilter);
            Assert.Equal("XML", exporter.FormatName);
        }

        [Fact]
        public void CustomerCsvExporter_HasCorrectProperties()
        {
            // Arrange
            var exporter = new CustomerCsvExporter();

            // Assert
            Assert.Equal(".csv", exporter.FileExtension);
            Assert.Equal("CSV Files|*.csv", exporter.FileFilter);
            Assert.Equal("CSV", exporter.FormatName);
        }
    }
}
