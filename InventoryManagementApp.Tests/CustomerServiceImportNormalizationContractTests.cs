using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class CustomerServiceImportNormalizationContractTests
    {
        [Fact]
        public void CsvCustomerImportNormalizesRowsBeforeValidationDuplicatesAndInsert()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Customers", "CustomerService.cs");
            var method = ExtractMethod(
                source,
                "async Task<CustomerImportResult> ImportCustomersFromCsvInternalAsync",
                "async Task ExportCustomersToCsvInternalAsync");

            Assert.Contains("var c = customers[i];", method, StringComparison.Ordinal);
            Assert.Contains("NormalizeImportedCustomer(c);", method, StringComparison.Ordinal);
            Assert.DoesNotContain("NormalizeCustomerForSave(c);", method, StringComparison.Ordinal);

            Assert.True(
                method.IndexOf("NormalizeImportedCustomer(c);", StringComparison.Ordinal) < method.IndexOf("var reason = GetSkipReason(c);", StringComparison.Ordinal),
                "CSV customer imports should trim rows before required-field validation.");
            Assert.True(
                method.IndexOf("NormalizeImportedCustomer(c);", StringComparison.Ordinal) < method.IndexOf("CustomerExistsAsync(conn, tran, c.Contact, c.Phone, c.Mobile", StringComparison.Ordinal),
                "CSV customer imports should trim contact and phone fields before persisted duplicate checks.");
            Assert.True(
                method.IndexOf("NormalizeImportedCustomer(c);", StringComparison.Ordinal) < method.IndexOf("TryReserveImportedCustomer(importedCustomerKeys, c)", StringComparison.Ordinal),
                "CSV customer imports should reserve duplicate keys from normalized row text.");
            Assert.True(
                method.IndexOf("NormalizeImportedCustomer(c);", StringComparison.Ordinal) < method.IndexOf("InsertCustomerAsync(conn, tran, c", StringComparison.Ordinal),
                "CSV customer imports should only insert normalized customer text.");
        }

        [Fact]
        public void GenericCustomerImportBuildsNormalizedModelsBeforeValidationDuplicatesAndInsert()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Customers", "CustomerService.cs");
            var method = ExtractMethod(
                source,
                "public async Task<int> ImportCustomersAsync",
                "public async Task ExportCustomersAsync");

            Assert.Contains("var customerModel = CreateImportedCustomerModel(customer);", method, StringComparison.Ordinal);
            Assert.DoesNotContain("Company = customer.Company ?? string.Empty", method, StringComparison.Ordinal);
            Assert.DoesNotContain("NormalizeCustomerForSave(customerModel);", method, StringComparison.Ordinal);

            Assert.True(
                method.IndexOf("var customerModel = CreateImportedCustomerModel(customer);", StringComparison.Ordinal) < method.IndexOf("var skipReason = GetSkipReason(customerModel);", StringComparison.Ordinal),
                "Generic customer imports should normalize rows before required-field validation.");
            Assert.True(
                method.IndexOf("var customerModel = CreateImportedCustomerModel(customer);", StringComparison.Ordinal) < method.IndexOf("CustomerExistsAsync(conn, transaction, customerModel.Contact, customerModel.Phone, customerModel.Mobile", StringComparison.Ordinal),
                "Generic customer imports should normalize contact and phone fields before persisted duplicate checks.");
            Assert.True(
                method.IndexOf("var customerModel = CreateImportedCustomerModel(customer);", StringComparison.Ordinal) < method.IndexOf("TryReserveImportedCustomer(importedCustomerKeys, customerModel)", StringComparison.Ordinal),
                "Generic customer imports should reserve duplicate keys from normalized row text.");
            Assert.True(
                method.IndexOf("var customerModel = CreateImportedCustomerModel(customer);", StringComparison.Ordinal) < method.IndexOf("InsertCustomerAsync(conn, transaction, customerModel", StringComparison.Ordinal),
                "Generic customer imports should only insert normalized customer text.");
        }

        [Fact]
        public void ImportedCustomerModelFactoryNormalizesAllImportedTextFields()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Customers", "CustomerService.cs");
            var factory = ExtractMethod(
                source,
                "static CustomerModel CreateImportedCustomerModel",
                "static void NormalizeImportedCustomer");
            var normalizer = ExtractMethod(
                source,
                "static void NormalizeImportedCustomer",
                "static string? NormalizeImportedText");

            Assert.Contains("Company = customer.Company,", factory, StringComparison.Ordinal);
            Assert.Contains("Email = customer.Email,", factory, StringComparison.Ordinal);
            Assert.Contains("Contact = customer.Contact,", factory, StringComparison.Ordinal);
            Assert.Contains("Phone = customer.Phone,", factory, StringComparison.Ordinal);
            Assert.Contains("Mobile = customer.Mobile,", factory, StringComparison.Ordinal);
            Assert.Contains("Address = customer.Address", factory, StringComparison.Ordinal);
            Assert.Contains("NormalizeImportedCustomer(customerModel);", factory, StringComparison.Ordinal);

            Assert.Contains("customer.Company = NormalizeImportedText(customer.Company) ?? string.Empty;", normalizer, StringComparison.Ordinal);
            Assert.Contains("customer.Email = NormalizeImportedText(customer.Email) ?? string.Empty;", normalizer, StringComparison.Ordinal);
            Assert.Contains("customer.Contact = NormalizeImportedText(customer.Contact) ?? string.Empty;", normalizer, StringComparison.Ordinal);
            Assert.Contains("customer.Phone = NormalizeImportedText(customer.Phone) ?? string.Empty;", normalizer, StringComparison.Ordinal);
            Assert.Contains("customer.Mobile = NormalizeImportedText(customer.Mobile) ?? string.Empty;", normalizer, StringComparison.Ordinal);
            Assert.Contains("customer.Address = NormalizeImportedText(customer.Address) ?? string.Empty;", normalizer, StringComparison.Ordinal);
            Assert.Contains("static string? NormalizeImportedText(string? value) => value?.Trim();", source, StringComparison.Ordinal);
        }

        [Fact]
        public void CustomerSaveAndDuplicateKeyPathsShareImportedTextNormalizationRules()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Customers", "CustomerService.cs");
            var saveNormalizer = ExtractMethod(
                source,
                "static void NormalizeCustomerForSave",
                "static CustomerModel CreateImportedCustomerModel");
            var duplicateKeys = ExtractMethod(
                source,
                "static IEnumerable<string> BuildCustomerDuplicateKeys",
                "static string? GetSkipReason");

            Assert.Contains("NormalizeImportedCustomer(customer);", saveNormalizer, StringComparison.Ordinal);
            Assert.Contains("var contact = (customer.Contact ?? string.Empty).Trim();", duplicateKeys, StringComparison.Ordinal);
            Assert.Contains("var phone = (customer.Phone ?? string.Empty).Trim();", duplicateKeys, StringComparison.Ordinal);
            Assert.Contains("var mobile = (customer.Mobile ?? string.Empty).Trim();", duplicateKeys, StringComparison.Ordinal);
            Assert.Contains("yield return $\"{contact}|phone:{phone}\";", duplicateKeys, StringComparison.Ordinal);
            Assert.Contains("yield return $\"{contact}|mobile:{mobile}\";", duplicateKeys, StringComparison.Ordinal);
        }

        private static string ExtractMethod(string source, string startMarker, string endMarker)
        {
            var start = source.IndexOf(startMarker, StringComparison.Ordinal);
            Assert.True(start >= 0, $"Could not find method start marker: {startMarker}");

            var end = source.IndexOf(endMarker, start, StringComparison.Ordinal);
            Assert.True(end > start, $"Could not find method end marker: {endMarker}");

            return source[start..end];
        }

        private static string ReadRepoFile(params string[] parts)
        {
            var directory = AppContext.BaseDirectory;

            while (!string.IsNullOrEmpty(directory))
            {
                var candidate = Path.Combine(directory, Path.Combine(parts));
                if (File.Exists(candidate))
                    return NormalizeLineEndings(File.ReadAllText(candidate));

                var parent = Directory.GetParent(directory);
                if (parent is null)
                    break;

                directory = parent.FullName;
            }

            throw new FileNotFoundException($"Could not find repository file: {Path.Combine(parts)}");
        }

        private static string NormalizeLineEndings(string text) =>
            text.Replace("\r\n", "\n", StringComparison.Ordinal).Replace("\r", "\n", StringComparison.Ordinal);
    }
}
