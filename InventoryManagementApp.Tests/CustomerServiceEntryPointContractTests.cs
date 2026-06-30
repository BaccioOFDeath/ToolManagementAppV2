using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class CustomerServiceEntryPointContractTests
    {
        [Fact]
        public void CustomerCrudAndQueryHelpersHonorCancellationBeforeSqlWork()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Customers", "CustomerService.cs");

            AssertCancellationGuardBeforeConnection(
                source,
                "async Task AddCustomerInternalAsync",
                "async Task UpdateCustomerInternalAsync");
            AssertCancellationGuardBeforeSqlAndConnection(
                source,
                "async Task UpdateCustomerInternalAsync",
                "async Task DeleteCustomerInternalAsync");
            AssertCancellationGuardBeforeSqlAndConnection(
                source,
                "async Task DeleteCustomerInternalAsync",
                "async Task<CustomerModel?> GetCustomerByIDInternalAsync");
            AssertCancellationGuardBeforeSqlAndConnection(
                source,
                "async Task<CustomerModel?> GetCustomerByIDInternalAsync",
                "async Task<List<CustomerModel>> GetAllCustomersInternalAsync");
            AssertCancellationGuardBeforeSqlAndConnection(
                source,
                "async Task<List<CustomerModel>> GetAllCustomersInternalAsync",
                "async Task<int> CountCustomersInternalAsync");
            AssertCancellationGuardBeforeSqlAndConnection(
                source,
                "async Task<int> CountCustomersInternalAsync",
                "async Task<List<CustomerModel>> SearchCustomersInternalAsync");
            AssertCancellationGuardBeforeSqlAndConnection(
                source,
                "async Task<List<CustomerModel>> SearchCustomersInternalAsync",
                "async Task<CustomerImportResult> ImportCustomersFromCsvInternalAsync");
        }

        [Fact]
        public void DirectCustomerSavesNormalizeAndValidateRequiredFieldsBeforeAuthorizationAndWriteWork()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Customers", "CustomerService.cs");
            var addMethod = ExtractMethod(
                source,
                "public Task AddCustomerAsync",
                "public Task UpdateCustomerAsync");
            var updateMethod = ExtractMethod(
                source,
                "public Task UpdateCustomerAsync",
                "public Task DeleteCustomerAsync");

            Assert.Contains("NormalizeCustomerForSave(customer);", addMethod, StringComparison.Ordinal);
            Assert.Contains("ValidateCustomerRequiredFields(customer);", addMethod, StringComparison.Ordinal);
            Assert.Contains("NormalizeCustomerForSave(customer);", updateMethod, StringComparison.Ordinal);
            Assert.Contains("ValidateCustomerRequiredFields(customer);", updateMethod, StringComparison.Ordinal);

            Assert.True(
                addMethod.IndexOf("NormalizeCustomerForSave(customer);", StringComparison.Ordinal) < addMethod.IndexOf("ValidateCustomerRequiredFields(customer);", StringComparison.Ordinal),
                "Direct customer adds should normalize text before required-field validation.");
            Assert.True(
                addMethod.IndexOf("ValidateCustomerRequiredFields(customer);", StringComparison.Ordinal) < addMethod.IndexOf("_auth.EnsureAdmin();", StringComparison.Ordinal),
                "Invalid direct customer adds should fail before authorization or database work.");
            Assert.True(
                addMethod.IndexOf("ValidateCustomerRequiredFields(customer);", StringComparison.Ordinal) < addMethod.IndexOf("return AddCustomerInternalAsync", StringComparison.Ordinal),
                "Invalid direct customer adds should not reach insert work.");
            Assert.True(
                updateMethod.IndexOf("if (customer.CustomerID < 1)", StringComparison.Ordinal) < updateMethod.IndexOf("NormalizeCustomerForSave(customer);", StringComparison.Ordinal),
                "Invalid update customer IDs should still fail before normalizing save fields.");
            Assert.True(
                updateMethod.IndexOf("NormalizeCustomerForSave(customer);", StringComparison.Ordinal) < updateMethod.IndexOf("ValidateCustomerRequiredFields(customer);", StringComparison.Ordinal),
                "Direct customer updates should normalize text before required-field validation.");
            Assert.True(
                updateMethod.IndexOf("ValidateCustomerRequiredFields(customer);", StringComparison.Ordinal) < updateMethod.IndexOf("_auth.EnsureAdmin();", StringComparison.Ordinal),
                "Invalid direct customer updates should fail before authorization or database work.");
            Assert.True(
                updateMethod.IndexOf("ValidateCustomerRequiredFields(customer);", StringComparison.Ordinal) < updateMethod.IndexOf("return UpdateCustomerInternalAsync", StringComparison.Ordinal),
                "Invalid direct customer updates should not reach update write work.");
        }

        [Fact]
        public void CustomerValidationReusesImportRequiredFieldRulesAndTrimsPersistedText()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Customers", "CustomerService.cs");

            Assert.Contains("static void NormalizeCustomerForSave(CustomerModel customer)", source, StringComparison.Ordinal);
            Assert.Contains("customer.Company = (customer.Company ?? string.Empty).Trim();", source, StringComparison.Ordinal);
            Assert.Contains("customer.Email = (customer.Email ?? string.Empty).Trim();", source, StringComparison.Ordinal);
            Assert.Contains("customer.Contact = (customer.Contact ?? string.Empty).Trim();", source, StringComparison.Ordinal);
            Assert.Contains("customer.Phone = (customer.Phone ?? string.Empty).Trim();", source, StringComparison.Ordinal);
            Assert.Contains("customer.Mobile = (customer.Mobile ?? string.Empty).Trim();", source, StringComparison.Ordinal);
            Assert.Contains("customer.Address = (customer.Address ?? string.Empty).Trim();", source, StringComparison.Ordinal);
            Assert.Contains("static void ValidateCustomerRequiredFields(CustomerModel customer)", source, StringComparison.Ordinal);
            Assert.Contains("var reason = GetSkipReason(customer);", source, StringComparison.Ordinal);
            Assert.Contains("throw new ArgumentException(reason, nameof(customer));", source, StringComparison.Ordinal);
            Assert.Contains("if (string.IsNullOrWhiteSpace(c.Company)) reasons.Add(\"Company missing\");", source, StringComparison.Ordinal);
            Assert.Contains("if (string.IsNullOrWhiteSpace(c.Contact)) reasons.Add(\"Contact missing\");", source, StringComparison.Ordinal);
            Assert.Contains("if (string.IsNullOrWhiteSpace(c.Phone) && string.IsNullOrWhiteSpace(c.Mobile)) reasons.Add(\"Phone and Mobile missing\");", source, StringComparison.Ordinal);
        }

        [Fact]
        public void CustomerImportAndExportHelpersHonorCancellationBeforeDatabaseWork()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Customers", "CustomerService.cs");

            var csvImportMethod = ExtractMethod(
                source,
                "async Task<CustomerImportResult> ImportCustomersFromCsvInternalAsync",
                "async Task ExportCustomersToCsvInternalAsync");
            Assert.Contains("cancellationToken.ThrowIfCancellationRequested();", csvImportMethod, StringComparison.Ordinal);
            Assert.True(
                csvImportMethod.IndexOf("cancellationToken.ThrowIfCancellationRequested();", StringComparison.Ordinal) < csvImportMethod.IndexOf("CsvHelperUtil.LoadCustomersFromCsvAsync", StringComparison.Ordinal),
                "CSV import should honor already-cancelled callers before file or database work begins.");
            Assert.True(
                csvImportMethod.LastIndexOf("cancellationToken.ThrowIfCancellationRequested();", csvImportMethod.IndexOf("using var conn = _dbService.CreateConnection();", StringComparison.Ordinal), StringComparison.Ordinal) >= 0,
                "CSV import should re-check cancellation before opening the import transaction connection.");

            AssertCancellationGuardBeforeMethodCall(
                source,
                "async Task ExportCustomersToCsvInternalAsync",
                "async Task InsertCustomerAsync",
                "GetAllCustomersInternalAsync");
            AssertCancellationGuardBeforeSql(
                source,
                "async Task InsertCustomerAsync",
                "async Task<bool> CustomerExistsAsync");
            AssertCancellationGuardBeforeSql(
                source,
                "async Task<bool> CustomerExistsAsync",
                "static async Task EnsureCustomerRowExistsAsync");
            AssertCancellationGuardBeforeSql(
                source,
                "static async Task EnsureCustomerRowExistsAsync",
                "static void EnsureCustomerCreateSucceeded");
        }

        [Fact]
        public void AlternateCustomerImportExportEntrypointsHonorCancellationBeforeConnectionOrExporterWork()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Customers", "CustomerService.cs");

            var importMethod = ExtractMethod(
                source,
                "public async Task<int> ImportCustomersAsync",
                "public async Task ExportCustomersAsync");
            Assert.Contains("cancellationToken.ThrowIfCancellationRequested();", importMethod, StringComparison.Ordinal);
            Assert.True(
                importMethod.IndexOf("cancellationToken.ThrowIfCancellationRequested();", StringComparison.Ordinal) < importMethod.IndexOf("importer.ImportAsync", StringComparison.Ordinal),
                "Generic customer import should honor already-cancelled callers before importer work begins.");
            Assert.True(
                importMethod.LastIndexOf("cancellationToken.ThrowIfCancellationRequested();", importMethod.IndexOf("using var conn = _dbService.CreateConnection();", StringComparison.Ordinal), StringComparison.Ordinal) >= 0,
                "Generic customer import should re-check cancellation before opening the database connection.");

            AssertCancellationGuardBeforeMethodCall(
                source,
                "public async Task ExportCustomersAsync",
                "    }\n}",
                "GetAllCustomersAsync");
        }

        [Fact]
        public void CustomerCreatesCheckInsertedRowsBeforeAssigningNewCustomerId()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Customers", "CustomerService.cs");
            var method = ExtractMethod(
                source,
                "async Task InsertCustomerAsync",
                "async Task<bool> CustomerExistsAsync");
            var insertSql = method[..method.IndexOf("var p = new[]", StringComparison.Ordinal)];

            Assert.DoesNotContain("SELECT last_insert_rowid();", insertSql, StringComparison.Ordinal);
            Assert.Contains("var insertedRows = await cmd.ExecuteNonQueryAsync(cancellationToken);", method, StringComparison.Ordinal);
            Assert.Contains("EnsureCustomerCreateSucceeded(insertedRows);", method, StringComparison.Ordinal);
            Assert.Contains("using var idCmd = new SqliteCommand(\"SELECT last_insert_rowid();\", conn, tran);", method, StringComparison.Ordinal);
            Assert.Contains("customer.CustomerID = Convert.ToInt32(await idCmd.ExecuteScalarAsync(cancellationToken));", method, StringComparison.Ordinal);
            Assert.Contains("if (customer.CustomerID < 1)", method, StringComparison.Ordinal);
            Assert.Contains("throw new InvalidOperationException(\"Unable to create customer.\");", method, StringComparison.Ordinal);
            Assert.Contains("static void EnsureCustomerCreateSucceeded(int affectedRows)", source, StringComparison.Ordinal);
            Assert.DoesNotContain("customer.CustomerID = Convert.ToInt32(await cmd.ExecuteScalarAsync", method, StringComparison.Ordinal);

            Assert.True(
                method.IndexOf("var insertedRows = await cmd.ExecuteNonQueryAsync(cancellationToken);", StringComparison.Ordinal) < method.IndexOf("EnsureCustomerCreateSucceeded(insertedRows);", StringComparison.Ordinal),
                "Customer creation should capture affected rows before checking the insert result.");
            Assert.True(
                method.IndexOf("EnsureCustomerCreateSucceeded(insertedRows);", StringComparison.Ordinal) < method.IndexOf("using var idCmd = new SqliteCommand(\"SELECT last_insert_rowid();\", conn, tran);", StringComparison.Ordinal),
                "Failed customer creates should stop before reading a new customer id.");
            Assert.True(
                method.IndexOf("if (customer.CustomerID < 1)", StringComparison.Ordinal) > method.IndexOf("customer.CustomerID = Convert.ToInt32(await idCmd.ExecuteScalarAsync(cancellationToken));", StringComparison.Ordinal),
                "Customer creation should reject invalid inserted ids before returning success.");
        }

        [Fact]
        public void CustomerImportsTrackDuplicateCustomersWithinEachImportBatch()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Customers", "CustomerService.cs");
            var csvImportMethod = ExtractMethod(
                source,
                "async Task<CustomerImportResult> ImportCustomersFromCsvInternalAsync",
                "async Task ExportCustomersToCsvInternalAsync");
            var genericImportMethod = ExtractMethod(
                source,
                "public async Task<int> ImportCustomersAsync",
                "public async Task ExportCustomersAsync");

            Assert.Contains("var importedCustomerKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);", csvImportMethod, StringComparison.Ordinal);
            Assert.Contains("if (!TryReserveImportedCustomer(importedCustomerKeys, c))", csvImportMethod, StringComparison.Ordinal);
            Assert.Contains("Row {row}: Duplicate customer in import file", csvImportMethod, StringComparison.Ordinal);
            Assert.Contains("var importedCustomerKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);", genericImportMethod, StringComparison.Ordinal);
            Assert.Contains("if (!TryReserveImportedCustomer(importedCustomerKeys, customerModel))", genericImportMethod, StringComparison.Ordinal);
            Assert.Contains("static bool TryReserveImportedCustomer(HashSet<string> importedCustomerKeys, CustomerModel customer)", source, StringComparison.Ordinal);
            Assert.Contains("static IEnumerable<string> BuildCustomerDuplicateKeys(CustomerModel customer)", source, StringComparison.Ordinal);
            Assert.Contains("yield return $\"{contact}|phone:{phone}\";", source, StringComparison.Ordinal);
            Assert.Contains("yield return $\"{contact}|mobile:{mobile}\";", source, StringComparison.Ordinal);

            Assert.True(
                csvImportMethod.IndexOf("if (await CustomerExistsAsync(conn, tran, c.Contact, c.Phone, c.Mobile, cancellationToken))", StringComparison.Ordinal) < csvImportMethod.IndexOf("if (!TryReserveImportedCustomer(importedCustomerKeys, c))", StringComparison.Ordinal),
                "CSV import should preserve the database duplicate check before reserving a customer identity for the current file.");
            Assert.True(
                csvImportMethod.IndexOf("if (!TryReserveImportedCustomer(importedCustomerKeys, c))", StringComparison.Ordinal) < csvImportMethod.IndexOf("await InsertCustomerAsync(conn, tran, c, cancellationToken);", StringComparison.Ordinal),
                "CSV import should reject duplicate rows from the same file before inserting into the transaction.");
            Assert.True(
                genericImportMethod.IndexOf("if (!TryReserveImportedCustomer(importedCustomerKeys, customerModel))", StringComparison.Ordinal) < genericImportMethod.IndexOf("await InsertCustomerAsync(conn, transaction, customerModel, cancellationToken);", StringComparison.Ordinal),
                "Generic customer import should reject duplicate rows from the same import batch before inserting.");
        }

        [Fact]
        public void CustomerImportsUseSingleTransactionForDuplicateChecksAndInsertWork()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Customers", "CustomerService.cs");
            var csvImportMethod = ExtractMethod(
                source,
                "async Task<CustomerImportResult> ImportCustomersFromCsvInternalAsync",
                "async Task ExportCustomersToCsvInternalAsync");
            var genericImportMethod = ExtractMethod(
                source,
                "public async Task<int> ImportCustomersAsync",
                "public async Task ExportCustomersAsync");
            var existsMethod = ExtractMethod(
                source,
                "async Task<bool> CustomerExistsAsync",
                "static async Task EnsureCustomerRowExistsAsync");

            Assert.Contains("using var conn = _dbService.CreateConnection();", csvImportMethod, StringComparison.Ordinal);
            Assert.Contains("using var tran = conn.BeginTransaction();", csvImportMethod, StringComparison.Ordinal);
            Assert.Contains("CustomerExistsAsync(conn, tran, c.Contact, c.Phone, c.Mobile, cancellationToken)", csvImportMethod, StringComparison.Ordinal);
            Assert.Contains("await InsertCustomerAsync(conn, tran, c, cancellationToken);", csvImportMethod, StringComparison.Ordinal);
            Assert.Contains("tran.Commit();", csvImportMethod, StringComparison.Ordinal);
            Assert.Contains("tran.Rollback();", csvImportMethod, StringComparison.Ordinal);

            Assert.Contains("using var conn = _dbService.CreateConnection();", genericImportMethod, StringComparison.Ordinal);
            Assert.Contains("using var transaction = conn.BeginTransaction();", genericImportMethod, StringComparison.Ordinal);
            Assert.Contains("CustomerExistsAsync(conn, transaction, customerModel.Contact, customerModel.Phone, customerModel.Mobile, cancellationToken)", genericImportMethod, StringComparison.Ordinal);
            Assert.Contains("await InsertCustomerAsync(conn, transaction, customerModel, cancellationToken);", genericImportMethod, StringComparison.Ordinal);
            Assert.Contains("transaction.Commit();", genericImportMethod, StringComparison.Ordinal);
            Assert.Contains("transaction.Rollback();", genericImportMethod, StringComparison.Ordinal);

            Assert.Contains("async Task<bool> CustomerExistsAsync(SqliteConnection conn, SqliteTransaction? transaction", existsMethod, StringComparison.Ordinal);
            Assert.Contains("using var cmd = new SqliteCommand(sql, conn, transaction);", existsMethod, StringComparison.Ordinal);
            Assert.DoesNotContain("_dbService.CreateConnection()", existsMethod, StringComparison.Ordinal);
            Assert.DoesNotContain("SqliteHelper.ExecuteScalarAsync(conn, sql", existsMethod, StringComparison.Ordinal);

            Assert.True(
                csvImportMethod.IndexOf("using var tran = conn.BeginTransaction();", StringComparison.Ordinal) < csvImportMethod.IndexOf("CustomerExistsAsync(conn, tran", StringComparison.Ordinal),
                "CSV customer imports should begin the transaction before duplicate lookups.");
            Assert.True(
                genericImportMethod.IndexOf("using var transaction = conn.BeginTransaction();", StringComparison.Ordinal) < genericImportMethod.IndexOf("CustomerExistsAsync(conn, transaction", StringComparison.Ordinal),
                "Generic customer imports should begin the transaction before duplicate lookups.");
        }

        [Fact]
        public void CustomerImportsNormalizeRowsBeforeDuplicateChecksAndInsertWork()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Customers", "CustomerService.cs");
            var csvImportMethod = ExtractMethod(
                source,
                "async Task<CustomerImportResult> ImportCustomersFromCsvInternalAsync",
                "async Task ExportCustomersToCsvInternalAsync");
            var genericImportMethod = ExtractMethod(
                source,
                "public async Task<int> ImportCustomersAsync",
                "public async Task ExportCustomersAsync");

            Assert.Contains("NormalizeCustomerForSave(c);", csvImportMethod, StringComparison.Ordinal);
            Assert.Contains("NormalizeCustomerForSave(customerModel);", genericImportMethod, StringComparison.Ordinal);
            Assert.True(
                csvImportMethod.IndexOf("NormalizeCustomerForSave(c);", StringComparison.Ordinal) < csvImportMethod.IndexOf("var reason = GetSkipReason(c);", StringComparison.Ordinal),
                "CSV imports should trim row text before required-field validation.");
            Assert.True(
                csvImportMethod.IndexOf("NormalizeCustomerForSave(c);", StringComparison.Ordinal) < csvImportMethod.IndexOf("CustomerExistsAsync(conn, tran, c.Contact, c.Phone, c.Mobile, cancellationToken)", StringComparison.Ordinal),
                "CSV imports should check duplicates using normalized customer identity fields.");
            Assert.True(
                csvImportMethod.IndexOf("NormalizeCustomerForSave(c);", StringComparison.Ordinal) < csvImportMethod.IndexOf("await InsertCustomerAsync(conn, tran, c, cancellationToken);", StringComparison.Ordinal),
                "CSV imports should insert normalized customer values.");
            Assert.True(
                genericImportMethod.IndexOf("NormalizeCustomerForSave(customerModel);", StringComparison.Ordinal) < genericImportMethod.IndexOf("var skipReason = GetSkipReason(customerModel);", StringComparison.Ordinal),
                "Generic imports should trim row text before required-field validation.");
            Assert.True(
                genericImportMethod.IndexOf("NormalizeCustomerForSave(customerModel);", StringComparison.Ordinal) < genericImportMethod.IndexOf("CustomerExistsAsync(conn, transaction, customerModel.Contact, customerModel.Phone, customerModel.Mobile, cancellationToken)", StringComparison.Ordinal),
                "Generic imports should check duplicates using normalized customer identity fields.");
            Assert.True(
                genericImportMethod.IndexOf("NormalizeCustomerForSave(customerModel);", StringComparison.Ordinal) < genericImportMethod.IndexOf("await InsertCustomerAsync(conn, transaction, customerModel, cancellationToken);", StringComparison.Ordinal),
                "Generic imports should insert normalized customer values.");
        }

        private static void AssertCancellationGuardBeforeSqlAndConnection(string source, string startMarker, string endMarker)
        {
            var method = ExtractMethod(source, startMarker, endMarker);

            Assert.Contains("cancellationToken.ThrowIfCancellationRequested();", method, StringComparison.Ordinal);
            Assert.True(
                method.IndexOf("cancellationToken.ThrowIfCancellationRequested();", StringComparison.Ordinal) < method.IndexOf("const string sql", StringComparison.Ordinal),
                $"Expected {startMarker} to honor cancellation before SQL construction.");
            Assert.True(
                method.IndexOf("cancellationToken.ThrowIfCancellationRequested();", StringComparison.Ordinal) < method.IndexOf("CreateConnection", StringComparison.Ordinal),
                $"Expected {startMarker} to honor cancellation before opening a database connection.");
        }

        private static void AssertCancellationGuardBeforeConnection(string source, string startMarker, string endMarker)
        {
            var method = ExtractMethod(source, startMarker, endMarker);

            Assert.Contains("cancellationToken.ThrowIfCancellationRequested();", method, StringComparison.Ordinal);
            Assert.True(
                method.IndexOf("cancellationToken.ThrowIfCancellationRequested();", StringComparison.Ordinal) < method.IndexOf("CreateConnection", StringComparison.Ordinal),
                $"Expected {startMarker} to honor cancellation before opening a database connection.");
        }

        private static void AssertCancellationGuardBeforeSql(string source, string startMarker, string endMarker)
        {
            var method = ExtractMethod(source, startMarker, endMarker);

            Assert.Contains("cancellationToken.ThrowIfCancellationRequested();", method, StringComparison.Ordinal);
            Assert.True(
                method.IndexOf("cancellationToken.ThrowIfCancellationRequested();", StringComparison.Ordinal) < method.IndexOf("const string sql", StringComparison.Ordinal),
                $"Expected {startMarker} to honor cancellation before SQL construction.");
        }

        private static void AssertCancellationGuardBeforeMethodCall(string source, string startMarker, string endMarker, string methodCall)
        {
            var method = ExtractMethod(source, startMarker, endMarker);

            Assert.Contains("cancellationToken.ThrowIfCancellationRequested();", method, StringComparison.Ordinal);
            Assert.True(
                method.IndexOf("cancellationToken.ThrowIfCancellationRequested();", StringComparison.Ordinal) < method.IndexOf(methodCall, StringComparison.Ordinal),
                $"Expected {startMarker} to honor cancellation before {methodCall}.");
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
