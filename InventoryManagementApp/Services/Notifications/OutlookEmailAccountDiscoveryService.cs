using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Win32;

namespace InventoryManagementApp.Services.Notifications
{
    public sealed class OutlookEmailAccountDiscoveryService : IEmailAccountDiscoveryService
    {
        private static readonly string[] OutlookProfileRoots =
        {
            @"Software\Microsoft\Office\16.0\Outlook\Profiles",
            @"Software\Microsoft\Office\15.0\Outlook\Profiles",
            @"Software\Microsoft\Office\14.0\Outlook\Profiles",
            @"Software\Microsoft\Windows NT\CurrentVersion\Windows Messaging Subsystem\Profiles"
        };

        private readonly ILogger<OutlookEmailAccountDiscoveryService> _logger;

        public OutlookEmailAccountDiscoveryService(ILogger<OutlookEmailAccountDiscoveryService>? logger = null)
        {
            _logger = logger ?? NullLogger<OutlookEmailAccountDiscoveryService>.Instance;
        }

        public Task<IReadOnlyList<EmailAccountOption>> GetOutlookAccountsAsync(CancellationToken cancellationToken = default)
        {
            if (!OperatingSystem.IsWindows())
            {
                return Task.FromResult<IReadOnlyList<EmailAccountOption>>(Array.Empty<EmailAccountOption>());
            }

            try
            {
                var accounts = new Dictionary<string, EmailAccountOption>(StringComparer.OrdinalIgnoreCase);
                ReadComAccounts(accounts, cancellationToken);
                foreach (var rootPath in OutlookProfileRoots)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    using var root = Registry.CurrentUser.OpenSubKey(rootPath);
                    if (root == null)
                    {
                        continue;
                    }

                    ReadAccounts(root, accounts, cancellationToken);
                }

                return Task.FromResult<IReadOnlyList<EmailAccountOption>>(
                    accounts.Values.OrderBy(account => account.DisplayText, StringComparer.OrdinalIgnoreCase).ToList());
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to discover Outlook email accounts.");
                return Task.FromResult<IReadOnlyList<EmailAccountOption>>(Array.Empty<EmailAccountOption>());
            }
        }

        private static void ReadComAccounts(IDictionary<string, EmailAccountOption> accounts, CancellationToken cancellationToken)
        {
            Type? outlookType;
            try
            {
                outlookType = Type.GetTypeFromProgID("Outlook.Application");
            }
            catch
            {
                return;
            }

            if (outlookType == null)
            {
                return;
            }

            object? outlook = null;
            object? session = null;
            object? accountCollection = null;
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                outlook = Activator.CreateInstance(outlookType);
                session = outlookType.InvokeMember("Session", System.Reflection.BindingFlags.GetProperty, null, outlook, null);
                accountCollection = session?.GetType().InvokeMember("Accounts", System.Reflection.BindingFlags.GetProperty, null, session, null);
                var count = Convert.ToInt32(accountCollection?.GetType().InvokeMember("Count", System.Reflection.BindingFlags.GetProperty, null, accountCollection, null) ?? 0);

                for (var index = 1; index <= count; index++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var account = accountCollection!.GetType().InvokeMember("Item", System.Reflection.BindingFlags.InvokeMethod, null, accountCollection, new object[] { index });
                    if (account == null)
                    {
                        continue;
                    }

                    var email = ReadComString(account, "SmtpAddress");
                    if (!IsEmailAddress(email))
                    {
                        email = ReadComString(account, "UserName");
                    }

                    if (!IsEmailAddress(email))
                    {
                        ReleaseComObject(account);
                        continue;
                    }

                    email = email!.Trim();
                    if (!accounts.ContainsKey(email))
                    {
                        var displayName = ReadComString(account, "DisplayName") ?? ReadComString(account, "UserName") ?? email;
                        var userName = ReadComString(account, "UserName") ?? email;
                        accounts[email] = new EmailAccountOption(displayName.Trim(), email, userName.Trim());
                    }

                    ReleaseComObject(account);
                }
            }
            catch
            {
            }
            finally
            {
                ReleaseComObject(accountCollection);
                ReleaseComObject(session);
                ReleaseComObject(outlook);
            }
        }

        private static string? ReadComString(object source, string propertyName)
        {
            try
            {
                return source.GetType().InvokeMember(propertyName, System.Reflection.BindingFlags.GetProperty, null, source, null)?.ToString();
            }
            catch
            {
                return null;
            }
        }

        private static void ReleaseComObject(object? instance)
        {
            if (instance != null && System.Runtime.InteropServices.Marshal.IsComObject(instance))
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(instance);
            }
        }

        private static void ReadAccounts(RegistryKey key, IDictionary<string, EmailAccountOption> accounts, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var email = FirstValue(key, "SMTP Email Address", "Email", "User Email", "Account Email");
            if (email is { } emailValue && IsEmailAddress(emailValue))
            {
                emailValue = emailValue.Trim();
                if (!accounts.ContainsKey(emailValue))
                {
                    var displayName = FirstValue(key, "Account Name", "Display Name", "Account Display Name") ?? emailValue;
                    var userName = FirstValue(key, "SMTP User Name", "User Name", "Account Name") ?? emailValue;
                    accounts[emailValue] = new EmailAccountOption(displayName, emailValue, userName);
                }
            }

            foreach (var subKeyName in key.GetSubKeyNames())
            {
                using var subKey = key.OpenSubKey(subKeyName);
                if (subKey != null)
                {
                    ReadAccounts(subKey, accounts, cancellationToken);
                }
            }
        }

        private static string? FirstValue(RegistryKey key, params string[] names)
        {
            foreach (var name in names)
            {
                var value = ReadRegistryString(key.GetValue(name));
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value.Trim();
                }
            }

            return null;
        }

        private static string? ReadRegistryString(object? value)
        {
            return value switch
            {
                string text => text,
                byte[] bytes => DecodeRegistryBytes(bytes),
                _ => null
            };
        }

        private static string? DecodeRegistryBytes(byte[] bytes)
        {
            if (bytes.Length == 0)
            {
                return null;
            }

            var unicode = Encoding.Unicode.GetString(bytes).TrimEnd('\0', ' ');
            if (IsUsableText(unicode))
            {
                return unicode;
            }

            var ascii = Encoding.ASCII.GetString(bytes).TrimEnd('\0', ' ');
            return IsUsableText(ascii) ? ascii : null;
        }

        private static bool IsUsableText(string value)
            => value.Any(char.IsLetterOrDigit) && value.Count(c => c == '@') <= 1;

        private static bool IsEmailAddress(string? value)
            => !string.IsNullOrWhiteSpace(value) &&
               value.Contains('@', StringComparison.Ordinal) &&
               value.Contains('.', StringComparison.Ordinal);
    }
}
