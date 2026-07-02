using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class UserServiceSaveNormalizationContractTests
    {
        [Fact]
        public void AddUserNormalizesProfileAndPermissionTextBeforeFirstUserCheckAndInsert()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Users", "UserService.cs");
            var method = ExtractMethod(
                source,
                "public async Task AddUserAsync",
                "public async Task UpdateUserAsync");

            Assert.Contains("NormalizeUserForSave(user);", method, StringComparison.Ordinal);
            Assert.True(
                method.IndexOf("NormalizeUserForSave(user);", StringComparison.Ordinal) < method.IndexOf("GetAllUsersAsync(CancellationToken.None)", StringComparison.Ordinal),
                "User creation should normalize profile/access text before first-user checks, authorization branching, and insert work.");
            Assert.Contains("new SqliteParameter(\"@UserName\", user.UserName)", method, StringComparison.Ordinal);
            Assert.Contains("new SqliteParameter(\"@Photo\",    ToDbNullableText(user.UserPhotoPath))", method, StringComparison.Ordinal);
            Assert.Contains("new SqliteParameter(\"@Email\",    ToDbNullableText(user.Email))", method, StringComparison.Ordinal);
            Assert.Contains("new SqliteParameter(\"@Phone\",    ToDbNullableText(user.Phone))", method, StringComparison.Ordinal);
            Assert.Contains("new SqliteParameter(\"@Mobile\",   ToDbNullableText(user.Mobile))", method, StringComparison.Ordinal);
            Assert.Contains("new SqliteParameter(\"@Address\",  ToDbNullableText(user.Address))", method, StringComparison.Ordinal);
            Assert.Contains("new SqliteParameter(\"@Role\",     ToDbNullableText(user.Role))", method, StringComparison.Ordinal);
            Assert.Contains("new SqliteParameter(\"@Permissions\", ToDbNullableText(user.Permissions))", method, StringComparison.Ordinal);
            Assert.DoesNotContain("(object)user.Email ?? DBNull.Value", method, StringComparison.Ordinal);
            Assert.DoesNotContain("(object)user.Permissions ?? DBNull.Value", method, StringComparison.Ordinal);
        }

        [Fact]
        public void UpdateUserNormalizesProfileAndPermissionTextBeforeExistingUserReadAndUpdate()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Users", "UserService.cs");
            var method = ExtractMethod(
                source,
                "public async Task UpdateUserAsync",
                "public async Task<bool> ChangeUserPasswordAsync");

            Assert.Contains("NormalizeUserForSave(user);", method, StringComparison.Ordinal);
            Assert.True(
                method.IndexOf("NormalizeUserForSave(user);", StringComparison.Ordinal) < method.IndexOf("GetUserByIDAsync(user.UserID, CancellationToken.None)", StringComparison.Ordinal),
                "User updates should normalize profile/access text before existing-row reads and persisted update parameters.");
            Assert.Contains("new SqliteParameter(\"@UserName\", user.UserName)", method, StringComparison.Ordinal);
            Assert.Contains("new SqliteParameter(\"@Photo\",    ToDbNullableText(user.UserPhotoPath))", method, StringComparison.Ordinal);
            Assert.Contains("new SqliteParameter(\"@Email\",    ToDbNullableText(user.Email))", method, StringComparison.Ordinal);
            Assert.Contains("new SqliteParameter(\"@Phone\",    ToDbNullableText(user.Phone))", method, StringComparison.Ordinal);
            Assert.Contains("new SqliteParameter(\"@Mobile\",   ToDbNullableText(user.Mobile))", method, StringComparison.Ordinal);
            Assert.Contains("new SqliteParameter(\"@Address\",  ToDbNullableText(user.Address))", method, StringComparison.Ordinal);
            Assert.Contains("new SqliteParameter(\"@Role\",     ToDbNullableText(user.Role))", method, StringComparison.Ordinal);
            Assert.Contains("new SqliteParameter(\"@Permissions\", ToDbNullableText(user.Permissions))", method, StringComparison.Ordinal);
            Assert.DoesNotContain("(object)user.UserPhotoPath ?? DBNull.Value", method, StringComparison.Ordinal);
            Assert.DoesNotContain("(object)user.Role ?? DBNull.Value", method, StringComparison.Ordinal);
        }

        [Fact]
        public void UserSaveNormalizerCoversProfileAndAccessFields()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Users", "UserService.cs");
            var normalizer = ExtractMethod(
                source,
                "private static void NormalizeUserForSave",
                "private static string NormalizePermissionsForSave");

            Assert.Contains("user.UserName = NormalizeRequiredText(user.UserName);", normalizer, StringComparison.Ordinal);
            Assert.Contains("user.UserPhotoPath = NormalizeOptionalText(user.UserPhotoPath);", normalizer, StringComparison.Ordinal);
            Assert.Contains("user.Email = NormalizeOptionalText(user.Email);", normalizer, StringComparison.Ordinal);
            Assert.Contains("user.Phone = NormalizeOptionalText(user.Phone);", normalizer, StringComparison.Ordinal);
            Assert.Contains("user.Mobile = NormalizeOptionalText(user.Mobile);", normalizer, StringComparison.Ordinal);
            Assert.Contains("user.Address = NormalizeOptionalText(user.Address);", normalizer, StringComparison.Ordinal);
            Assert.Contains("user.Role = NormalizeOptionalText(user.Role);", normalizer, StringComparison.Ordinal);
            Assert.Contains("user.Permissions = NormalizePermissionsForSave(user.Permissions);", normalizer, StringComparison.Ordinal);
        }

        [Fact]
        public void UserPermissionNormalizerCanonicalizesPersistedAccessKeys()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Users", "UserService.cs");
            var normalizer = ExtractMethod(
                source,
                "private static string NormalizePermissionsForSave",
                "private static object ToDbNullableText");

            Assert.Contains("var normalized = NormalizeOptionalText(value);", normalizer, StringComparison.Ordinal);
            Assert.Contains("return string.Empty;", normalizer, StringComparison.Ordinal);
            Assert.Contains("User.BuildPermissions(normalized.Split(new[] { ';', ',', '|', ' ' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))", normalizer, StringComparison.Ordinal);
            Assert.DoesNotContain("return normalized;", normalizer, StringComparison.Ordinal);
        }

        [Fact]
        public void UserSaveStoresBlankOptionalTextAsDatabaseNull()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Users", "UserService.cs");
            var helper = ExtractMethod(
                source,
                "private static object ToDbNullableText",
                "private static string NormalizeRequiredText");

            Assert.Contains("var normalized = NormalizeOptionalText(value);", helper, StringComparison.Ordinal);
            Assert.Contains("return string.IsNullOrEmpty(normalized) ? DBNull.Value : normalized;", helper, StringComparison.Ordinal);
            Assert.Contains("private static string NormalizeOptionalText(string? value) => value?.Trim() ?? string.Empty;", source, StringComparison.Ordinal);
            Assert.Contains("private static string NormalizeRequiredText(string? value) => value?.Trim() ?? string.Empty;", source, StringComparison.Ordinal);
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
