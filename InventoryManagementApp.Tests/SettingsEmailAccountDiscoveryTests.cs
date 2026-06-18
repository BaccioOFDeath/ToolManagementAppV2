using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.ViewModels;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class SettingsEmailAccountDiscoveryTests
    {
        [Fact]
        public async Task SelectingOutlookProvider_LoadsDiscoveredAccountsAndAppliesFirstAccount()
        {
            var account = new EmailAccountOption("Work Account", "work@example.com", "work@example.com");
            var vm = CreateViewModel(new[] { account });

            vm.SelectedEmailProvider = "Outlook/Office 365";

            await WaitForAsync(() => vm.HasOutlookAccountOptions);

            Assert.True(vm.IsOutlookProvider);
            Assert.Equal("smtp.office365.com", vm.SmtpHost);
            Assert.Equal(587, vm.SmtpPort);
            Assert.Equal(account, vm.SelectedOutlookAccount);
            Assert.Equal("work@example.com", vm.SmtpUsername);
            Assert.Equal("work@example.com", vm.FromEmail);
            Assert.Contains("work@example.com", vm.FromEmailOptions);
        }

        [Fact]
        public void SelectedOutlookAccount_UpdatesSmtpUsernameAndSender()
        {
            var first = new EmailAccountOption("First", "first@example.com", "first@example.com");
            var second = new EmailAccountOption("Shared Mailbox", "shared@example.com", "shared");
            var vm = CreateViewModel(new[] { first, second });

            vm.SelectedOutlookAccount = second;

            Assert.Equal("shared@example.com", vm.SmtpUsername);
            Assert.Equal("shared@example.com", vm.FromEmail);
            Assert.Equal("shared@example.com", vm.SelectedFromEmail);
            Assert.Contains("shared@example.com", vm.FromEmailOptions);
            Assert.Equal("Not ready: enter the mailbox password or app password before testing.", vm.EmailConfigurationStatus);
            vm.SmtpPassword = "secret";
            Assert.Equal("Ready to test email delivery.", vm.EmailConfigurationStatus);
        }

        private static SettingsViewModel CreateViewModel(IReadOnlyList<EmailAccountOption> accounts)
            => new(
                new DummyFileDialogService(),
                new DummySettingsService(),
                new DummyDialogService(),
                new DummyThemeService(),
                emailAccountDiscoveryService: new StubEmailAccountDiscoveryService(accounts));

        private static async Task WaitForAsync(Func<bool> condition)
        {
            for (var i = 0; i < 100; i++)
            {
                if (condition())
                {
                    return;
                }

                await Task.Delay(10);
            }

            Assert.True(condition());
        }

        private sealed class StubEmailAccountDiscoveryService : IEmailAccountDiscoveryService
        {
            private readonly IReadOnlyList<EmailAccountOption> _accounts;

            public StubEmailAccountDiscoveryService(IReadOnlyList<EmailAccountOption> accounts)
            {
                _accounts = accounts;
            }

            public Task<IReadOnlyList<EmailAccountOption>> GetOutlookAccountsAsync(CancellationToken cancellationToken = default)
                => Task.FromResult(_accounts);
        }

        private sealed class DummyThemeService : IThemeService
        {
            public void ApplyTheme(string? theme) { }
        }

        private sealed class DummyFileDialogService : IFileDialogService
        {
            public string? OpenFile(string filter, string? initialDirectory = null) => null;
            public string? SaveFile(string filter, string? initialDirectory = null) => null;
            public string? BrowseFolder(string? initialDirectory = null) => null;
        }

        private sealed class DummyDialogService : IDialogService
        {
            public void ShowInfo(string message, string title) { }
            public bool ShowConfirmation(string message, string title) => false;
            public ItemModel? ShowEditItemDialog(ItemModel item) => null;
            public void ShowItemDetails(ItemModel item) { }
            public (CustomerModel customer, DateTime dueDate)? ShowRentItemDialog(ItemModel item, IEnumerable<CustomerModel> customers) => null;
            public CustomerModel? ShowAddCustomerDialog() => null;
            public CustomerModel? ShowEditCustomerDialog(CustomerModel customer) => null;
            public void ShowRentalsFilter(ManageRentalsViewModel viewModel) { }
            public void ShowRentalHistory(ItemModel item, IEnumerable<RentalModel> history) { }
            public Dictionary<string, string>? ShowImportMapping(IEnumerable<string> headers, IEnumerable<string> properties, IEnumerable<string>? requiredPropertyNames = null) => null;
            public Func<ItemModel, IEnumerable<string>>? ShowImageImportMapping() => null;
            public void ShowPrintPreview(System.Windows.Documents.FlowDocument document, string title, string description) { }
            public void ShowPrintLabelDialog() { }
        }

        private sealed class DummySettingsService : ISettingsService
        {
            public event EventHandler<IDictionary<ItemDetailField, bool>>? ItemDetailVisibilityChanged;
            public event EventHandler<double>? ItemCardSizeChanged;
            public Task SaveSettingAsync(string key, string value, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<string?> GetSettingAsync(string? key, CancellationToken cancellationToken = default) => Task.FromResult<string?>(null);
            public Task<Dictionary<string, string>> GetAllSettingsAsync(CancellationToken cancellationToken = default) => Task.FromResult(new Dictionary<string, string>());
            public Task UpdateSettingsAsync(Dictionary<string, string> settings, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task DeleteSettingAsync(string key, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<string?> GetThemeAsync(CancellationToken cancellationToken = default) => Task.FromResult<string?>(null);
            public Task SaveThemeAsync(string theme, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<int> GetPasswordIterationsAsync(CancellationToken cancellationToken = default) => Task.FromResult(0);
            public Task SavePasswordIterationsAsync(int iterations, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<int> GetAutoLogoutMinutesAsync(CancellationToken cancellationToken = default) => Task.FromResult(0);
            public Task SaveAutoLogoutMinutesAsync(int minutes, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<string> GetItemLabelSingularAsync(CancellationToken cancellationToken = default) => Task.FromResult(string.Empty);
            public Task SaveItemLabelSingularAsync(string label, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<string> GetItemLabelPluralAsync(CancellationToken cancellationToken = default) => Task.FromResult(string.Empty);
            public Task SaveItemLabelPluralAsync(string label, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<IDictionary<ItemDetailField, bool>> GetItemDetailVisibilityAsync(CancellationToken cancellationToken = default) => Task.FromResult<IDictionary<ItemDetailField, bool>>(new Dictionary<ItemDetailField, bool>());
            public Task SaveItemDetailVisibilityAsync(IDictionary<ItemDetailField, bool> visibility, CancellationToken cancellationToken = default)
            {
                ItemDetailVisibilityChanged?.Invoke(this, visibility);
                return Task.CompletedTask;
            }

            public Task<double> GetItemCardSizeAsync(CancellationToken cancellationToken = default) => Task.FromResult(1.0);
            public Task SaveItemCardSizeAsync(double size, CancellationToken cancellationToken = default)
            {
                ItemCardSizeChanged?.Invoke(this, size);
                return Task.CompletedTask;
            }
        }
    }
}
