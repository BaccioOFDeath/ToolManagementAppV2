using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class RentalConfigurationNormalizationContractTests
    {
        [Fact]
        public void SingleLineEmailCompanyAndSmsSettingsNormalizeOnSaveAndRead()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Settings", "RentalConfigurationService.cs");

            AssertSetterNormalizes(source, "SetSmtpHostAsync", "SmtpHostKey");
            AssertSetterNormalizes(source, "SetSmtpUsernameAsync", "SmtpUsernameKey");
            AssertSetterNormalizes(source, "SetFromEmailAsync", "FromEmailKey");
            AssertSetterNormalizes(source, "SetFromNameAsync", "FromNameKey");
            AssertSetterNormalizes(source, "SetContactInfoAsync", "ContactInfoKey");
            AssertSetterNormalizes(source, "SetCompanyNameAsync", "CompanyNameKey");
            AssertSetterNormalizes(source, "SetCompanyAddressAsync", "CompanyAddressKey");
            AssertSetterNormalizes(source, "SetCompanyPhoneAsync", "CompanyPhoneKey");
            AssertSetterNormalizes(source, "SetBackupDirectoryAsync", "BackupDirectoryKey");
            AssertSetterNormalizes(source, "SetSmsProviderAsync", "SmsProviderKey");
            AssertSetterNormalizes(source, "SetSmsSenderAsync", "SmsSenderKey");

            AssertGetterUsesDefaultBoundary(source, "GetSmtpHostAsync", "SmtpHostKey", "smtp.example.com");
            AssertGetterUsesDefaultBoundary(source, "GetSmtpUsernameAsync", "SmtpUsernameKey", "string.Empty");
            AssertGetterUsesDefaultBoundary(source, "GetFromEmailAsync", "FromEmailKey", "rentals@example.com");
            AssertGetterUsesDefaultBoundary(source, "GetFromNameAsync", "FromNameKey", "Equipment Rentals");
            AssertGetterUsesDefaultBoundary(source, "GetContactInfoAsync", "ContactInfoKey", "Contact us for more information");
            AssertGetterUsesDefaultBoundary(source, "GetCompanyNameAsync", "CompanyNameKey", "Equipment Rentals");
            AssertGetterUsesDefaultBoundary(source, "GetCompanyAddressAsync", "CompanyAddressKey", "string.Empty");
            AssertGetterUsesDefaultBoundary(source, "GetCompanyPhoneAsync", "CompanyPhoneKey", "string.Empty");
            AssertGetterUsesDefaultBoundary(source, "GetBackupDirectoryAsync", "BackupDirectoryKey", "Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)");
            AssertGetterUsesDefaultBoundary(source, "GetSmsProviderAsync", "SmsProviderKey", "None");
            AssertGetterUsesDefaultBoundary(source, "GetSmsSenderAsync", "SmsSenderKey", "string.Empty");
        }

        [Fact]
        public void ReminderAndOverdueTemplatesNormalizeEdgesAndFallbackWhenBlank()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Settings", "RentalConfigurationService.cs");

            AssertSetterNormalizes(source, "SetEmailSignatureAsync", "EmailSignatureKey", "NormalizeMultilineSetting");
            AssertSetterNormalizes(source, "SetReminderSubjectTemplateAsync", "ReminderSubjectTemplateKey");
            AssertSetterNormalizes(source, "SetReminderBodyTemplateAsync", "ReminderBodyTemplateKey", "NormalizeMultilineSetting");
            AssertSetterNormalizes(source, "SetOverdueSubjectTemplateAsync", "OverdueSubjectTemplateKey");
            AssertSetterNormalizes(source, "SetOverdueBodyTemplateAsync", "OverdueBodyTemplateKey", "NormalizeMultilineSetting");

            AssertGetterUsesDefaultBoundary(source, "GetEmailSignatureAsync", "EmailSignatureKey", "DefaultEmailSignature", "GetMultilineSettingOrDefault");
            AssertGetterUsesDefaultBoundary(source, "GetReminderSubjectTemplateAsync", "ReminderSubjectTemplateKey", "DefaultReminderSubjectTemplate");
            AssertGetterUsesDefaultBoundary(source, "GetReminderBodyTemplateAsync", "ReminderBodyTemplateKey", "DefaultReminderBodyTemplate", "GetMultilineSettingOrDefault");
            AssertGetterUsesDefaultBoundary(source, "GetOverdueSubjectTemplateAsync", "OverdueSubjectTemplateKey", "DefaultOverdueSubjectTemplate");
            AssertGetterUsesDefaultBoundary(source, "GetOverdueBodyTemplateAsync", "OverdueBodyTemplateKey", "DefaultOverdueBodyTemplate", "GetMultilineSettingOrDefault");
        }

        [Fact]
        public void FromEmailOptionsAreNullSafeTrimmedAndDeduplicated()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Settings", "RentalConfigurationService.cs");
            var setter = ExtractMember(source, "public async Task SetFromEmailOptionsAsync", "public async Task<string> GetEmailSignatureAsync");
            var getter = ExtractMember(source, "public async Task<IReadOnlyList<string>> GetFromEmailOptionsAsync", "public async Task SetFromEmailOptionsAsync");

            Assert.Contains("var normalized = (options ?? Enumerable.Empty<string>())", setter, StringComparison.Ordinal);
            Assert.Contains(".Where(email => !string.IsNullOrWhiteSpace(email))", setter, StringComparison.Ordinal);
            Assert.Contains(".Select(email => email.Trim())", setter, StringComparison.Ordinal);
            Assert.Contains(".Distinct(StringComparer.OrdinalIgnoreCase)", setter, StringComparison.Ordinal);
            Assert.Contains("JsonSerializer.Serialize(normalized)", setter, StringComparison.Ordinal);

            Assert.Contains("options.Insert(0, currentFromEmail);", getter, StringComparison.Ordinal);
            Assert.Contains(".Select(email => email.Trim())", getter, StringComparison.Ordinal);
            Assert.Contains(".Distinct(StringComparer.OrdinalIgnoreCase)", getter, StringComparison.Ordinal);
        }

        [Fact]
        public void PasswordAndApiKeyValuesRemainUntrimmedButNullSafe()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Settings", "RentalConfigurationService.cs");
            var passwordSetter = ExtractMember(source, "public async Task SetSmtpPasswordAsync", "public async Task<string> GetFromEmailAsync");
            var apiKeySetter = ExtractMember(source, "public async Task SetSmsApiKeyAsync", "public async Task<string> GetSmsSenderAsync");

            Assert.Contains("SaveSettingAsync(SmtpPasswordKey, password ?? string.Empty", passwordSetter, StringComparison.Ordinal);
            Assert.Contains("SaveSettingAsync(SmsApiKey, apiKey ?? string.Empty", apiKeySetter, StringComparison.Ordinal);
            Assert.DoesNotContain("NormalizeSingleLineSetting(password)", passwordSetter, StringComparison.Ordinal);
            Assert.DoesNotContain("NormalizeSingleLineSetting(apiKey)", apiKeySetter, StringComparison.Ordinal);
        }

        [Fact]
        public void ConfigurationNormalizationHelpersTrimEdgesAndFallbackOnBlankValues()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Settings", "RentalConfigurationService.cs");
            var helpers = ExtractMember(source, "private static string GetSingleLineSettingOrDefault", "    }\n}");

            Assert.Contains("private static string GetSingleLineSettingOrDefault(string? value, string defaultValue)", helpers, StringComparison.Ordinal);
            Assert.Contains("var normalized = NormalizeSingleLineSetting(value);", helpers, StringComparison.Ordinal);
            Assert.Contains("return string.IsNullOrWhiteSpace(normalized) ? defaultValue : normalized;", helpers, StringComparison.Ordinal);
            Assert.Contains("private static string GetMultilineSettingOrDefault(string? value, string defaultValue)", helpers, StringComparison.Ordinal);
            Assert.Contains("var normalized = NormalizeMultilineSetting(value);", helpers, StringComparison.Ordinal);
            Assert.Contains("private static string NormalizeSingleLineSetting(string? value) => value?.Trim() ?? string.Empty;", helpers, StringComparison.Ordinal);
            Assert.Contains("private static string NormalizeMultilineSetting(string? value) => value?.Trim() ?? string.Empty;", helpers, StringComparison.Ordinal);
        }

        private static void AssertSetterNormalizes(string source, string methodName, string settingKey, string normalizer = "NormalizeSingleLineSetting")
        {
            var method = ExtractMember(source, $"public async Task {methodName}", "public async Task");
            Assert.Contains($"SaveSettingAsync({settingKey}, {normalizer}(", method, StringComparison.Ordinal);
        }

        private static void AssertGetterUsesDefaultBoundary(
            string source,
            string methodName,
            string settingKey,
            string defaultValue,
            string helper = "GetSingleLineSettingOrDefault")
        {
            var method = ExtractMember(source, $"public async Task<string> {methodName}", "public async Task");
            Assert.Contains($"GetSettingAsync({settingKey}", method, StringComparison.Ordinal);
            Assert.Contains($"return {helper}(value, {FormatExpectedDefault(defaultValue)});", method, StringComparison.Ordinal);
        }

        private static string FormatExpectedDefault(string expected) => expected switch
        {
            "string.Empty" => "string.Empty",
            "Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)" => "Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)",
            var value when value.StartsWith("Default", StringComparison.Ordinal) => value,
            _ => $"\"{expected}\""
        };

        private static string ExtractMember(string source, string startMarker, string endMarker)
        {
            var start = source.IndexOf(startMarker, StringComparison.Ordinal);
            Assert.True(start >= 0, $"Could not find start marker: {startMarker}");

            var end = source.IndexOf(endMarker, start + startMarker.Length, StringComparison.Ordinal);
            if (end < 0)
            {
                end = source.IndexOf("\n        private static", start + startMarker.Length, StringComparison.Ordinal);
            }

            Assert.True(end > start, $"Could not find end marker after: {startMarker}");
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
