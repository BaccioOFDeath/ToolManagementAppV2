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
    public class ItemImportExportTests : IDisposable
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

        private List<ItemModel> GetSampleItems()
        {
            return new List<ItemModel>
            {
                new ItemModel
                {
                    ItemID = 1,
                    ItemNumber = "ITEM-001",
                    Name = "Test Item 1",
                    Location = "Shelf A",
                    Brand = "TestBrand",
                    QuantityOnHand = 5,
                    IsRentalItem = true
                },
                new ItemModel
                {
                    ItemID = 2,
                    ItemNumber = "ITEM-002",
                    Name = "Test Item 2",
                    Location = "Shelf B",
                    Brand = "AnotherBrand",
                    QuantityOnHand = 10,
                    IsRentalItem = false
                }
            };
        }

        [Fact]
        public async Task ItemJsonExporter_ExportsAndImportsCorrectly()
        {
            // Arrange
            var items = GetSampleItems();
            var exporter = new ItemJsonExporter();
            var importer = new ItemJsonImporter();
            var filePath = CreateTempFile(".json");

            // Act - Export
            await exporter.ExportAsync(filePath, items, CancellationToken.None);

            // Assert - File exists
            Assert.True(File.Exists(filePath));

            // Act - Import
            var (importedItems, skippedRows) = await importer.ImportAsync(filePath, CancellationToken.None);
            var importedList = importedItems.ToList();

            // Assert - Data integrity
            Assert.Equal(2, importedList.Count);
            Assert.Empty(skippedRows);
            Assert.Equal("ITEM-001", importedList[0].ItemNumber);
            Assert.Equal("Test Item 1", importedList[0].Name);
            Assert.Equal("ITEM-002", importedList[1].ItemNumber);
            Assert.Equal("Test Item 2", importedList[1].Name);
        }

        [Fact]
        public async Task ItemXmlExporter_ExportsAndImportsCorrectly()
        {
            // Arrange
            var items = GetSampleItems();
            var exporter = new ItemXmlExporter();
            var importer = new ItemXmlImporter();
            var filePath = CreateTempFile(".xml");

            // Act - Export
            await exporter.ExportAsync(filePath, items, CancellationToken.None);

            // Assert - File exists
            Assert.True(File.Exists(filePath));

            // Act - Import
            var (importedItems, skippedRows) = await importer.ImportAsync(filePath, CancellationToken.None);
            var importedList = importedItems.ToList();

            // Assert - Data integrity
            Assert.Equal(2, importedList.Count);
            Assert.Empty(skippedRows);
            Assert.Equal("ITEM-001", importedList[0].ItemNumber);
            Assert.Equal("Test Item 1", importedList[0].Name);
        }

        [Fact]
        public async Task ItemCsvExporter_ExportsCorrectly()
        {
            // Arrange
            var items = GetSampleItems();
            var exporter = new ItemCsvExporter();
            var filePath = CreateTempFile(".csv");

            // Act
            await exporter.ExportAsync(filePath, items, CancellationToken.None);

            // Assert
            Assert.True(File.Exists(filePath));
            var content = await File.ReadAllTextAsync(filePath);
            Assert.Contains("ITEM-001", content);
            Assert.Contains("Test Item 1", content);
        }

        [Fact]
        public async Task ItemJsonImporter_SkipsInvalidItems()
        {
            // Arrange
            var filePath = CreateTempFile(".json");
            var json = @"[
                {""ItemNumber"": ""VALID-001"", ""Name"": ""Valid Item""},
                {""Name"": ""Missing ItemNumber""},
                {""ItemNumber"": ""VALID-002"", ""Name"": ""Another Valid Item""}
            ]";
            await File.WriteAllTextAsync(filePath, json);
            var importer = new ItemJsonImporter();

            // Act
            var (importedItems, skippedRows) = await importer.ImportAsync(filePath, CancellationToken.None);
            var importedList = importedItems.ToList();

            // Assert
            Assert.Equal(2, importedList.Count);
            Assert.Single(skippedRows);
            Assert.Contains(2, skippedRows); // Second item (index 1) should be skipped
        }

        [Fact]
        public void ItemJsonExporter_HasCorrectProperties()
        {
            // Arrange
            var exporter = new ItemJsonExporter();

            // Assert
            Assert.Equal(".json", exporter.FileExtension);
            Assert.Equal("JSON Files|*.json", exporter.FileFilter);
            Assert.Equal("JSON", exporter.FormatName);
        }

        [Fact]
        public void ItemXmlExporter_HasCorrectProperties()
        {
            // Arrange
            var exporter = new ItemXmlExporter();

            // Assert
            Assert.Equal(".xml", exporter.FileExtension);
            Assert.Equal("XML Files|*.xml", exporter.FileFilter);
            Assert.Equal("XML", exporter.FormatName);
        }

        [Fact]
        public void ItemCsvExporter_HasCorrectProperties()
        {
            // Arrange
            var exporter = new ItemCsvExporter();

            // Assert
            Assert.Equal(".csv", exporter.FileExtension);
            Assert.Equal("CSV Files|*.csv", exporter.FileFilter);
            Assert.Equal("CSV", exporter.FormatName);
        }
    }
}
