using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.ViewModels;
using InventoryManagementApp.Views.Windows;
using WpfMessageBox = System.Windows.MessageBox;

namespace InventoryManagementApp.Views.Pages
{
    /// <summary>
    /// Page for viewing and managing users.
    /// The <see cref="DataContext"/> is expected to be a <see cref="UserManagementViewModel"/>.
    /// </summary>
    public partial class UsersPage : Page
    {
        private const int MaxUsersPrintRows = 250;

        public UsersPage(UserManagementViewModel? viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }

        public UserManagementViewModel? ViewModel =>
            DataContext as UserManagementViewModel;

        private void UserRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenSelectedUser_Click(sender, e);
        }

        private void UserRow_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            GridContextMenuSelection.SelectRow(sender, e);
        }

        private void OpenSelectedUser_Click(object sender, RoutedEventArgs e)
        {
            UiActionGuard.Run(this, "User Directory", () =>
            {
                if (UsersDataGrid.SelectedItem is not UserModel user)
                {
                    WpfMessageBox.Show("Select a user row first.", "User Directory", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                DetailDialogWindow.ShowDialogFor(
                    Window.GetWindow(this),
                    $"User Detail - {user.UserName}",
                    "User Detail",
                    FormatUserDetail(user),
                    "Review identity, access, lockout, and contact context before changing account settings.",
                    ResolveRole(user),
                    "Close returns to Users with the selected account ready for edit, reset, copy, print, or access review.");
            });
        }

        private void CopySelectedUser_Click(object sender, RoutedEventArgs e)
        {
            UiActionGuard.Run(this, "User Directory", () =>
            {
                if (UsersDataGrid.SelectedItem is not UserModel user)
                {
                    WpfMessageBox.Show("Select a user row first.", "User Directory", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                System.Windows.Clipboard.SetText(FormatUserDetail(user));
            });
        }

        private void ResetSelectedUser_Click(object sender, RoutedEventArgs e)
        {
            UiActionGuard.RunAsync(this, "User Directory", async () =>
            {
                if (ViewModel == null || UsersDataGrid.SelectedItem is not UserModel user)
                {
                    WpfMessageBox.Show("Select a user row first.", "User Directory", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                await ViewModel.ResetPasswordFromRowCommand.ExecuteAsync(user);
            });
        }

        private void PrintUsers_Click(object sender, RoutedEventArgs e)
        {
            UiActionGuard.Run(this, "User Directory", () =>
            {
                if (ViewModel == null || ViewModel.Users.Count == 0)
                {
                    WpfMessageBox.Show("There are no users to print.", "User Directory", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var totalVisibleCount = ViewModel.Users.Count;
                var printRows = ViewModel.Users.Take(MaxUsersPrintRows).ToList();
                var summary = $"Visible users: {totalVisibleCount}; printed rows: {printRows.Count}; omitted rows: {Math.Max(0, totalVisibleCount - printRows.Count)}";
                var document = BuildPrintDocument(printRows, totalVisibleCount, summary);
                ShowPrintPreview(document, "User Directory", "Review the current account directory, access coverage, lockout state, and any omitted rows before filing an admin handoff.");
            });
        }

        private static string FormatUserDetail(UserModel user)
        {
            return $"User #: {user.UserID}{Environment.NewLine}" +
                   $"Name: {user.UserName}{Environment.NewLine}" +
                   $"Role: {ResolveRole(user)}{Environment.NewLine}" +
                   $"Active: {FormatBool(user.IsActive)}{Environment.NewLine}" +
                   $"Admin: {FormatBool(user.IsAdmin)}{Environment.NewLine}" +
                   $"Access: {ValueOrDash(user.AccessSummary)}{Environment.NewLine}" +
                   $"Lockout: {ValueOrDash(user.LockoutStatus)}{Environment.NewLine}" +
                   $"Password expired: {FormatBool(user.PasswordExpired)}{Environment.NewLine}" +
                   $"Created: {FormatDate(user.CreatedAt)}{Environment.NewLine}{Environment.NewLine}" +
                   $"Email: {ValueOrDash(user.Email)}{Environment.NewLine}" +
                   $"Phone: {ValueOrDash(user.Phone)}{Environment.NewLine}" +
                   $"Mobile: {ValueOrDash(user.Mobile)}{Environment.NewLine}" +
                   $"Address: {ValueOrDash(user.Address)}{Environment.NewLine}{Environment.NewLine}" +
                   "Next steps: edit profile details, tick the app sections this user can access, upload a current photo, reset the password if the user is blocked, or review activity logs for recent account actions.";
        }

        private static FlowDocument BuildPrintDocument(IReadOnlyCollection<UserModel> users, int totalVisibleCount, string summary)
        {
            var document = new FlowDocument
            {
                FontFamily = new System.Windows.Media.FontFamily("Segoe UI"),
                FontSize = 10,
                PagePadding = new Thickness(32)
            };

            document.Blocks.Add(new Paragraph(new Run("User Directory"))
            {
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 4)
            });
            document.Blocks.Add(new Paragraph(new Run($"Printed {DateTime.Now:g}"))
            {
                FontSize = 10,
                Margin = new Thickness(0, 0, 0, 8)
            });

            document.Blocks.Add(BuildSummarySection(summary, totalVisibleCount, users.Count, Math.Max(0, totalVisibleCount - users.Count)));

            if (users.Count == 0)
            {
                document.Blocks.Add(new Paragraph(new Run("No user rows were available for this print packet."))
                {
                    Margin = new Thickness(0, 0, 0, 10),
                    FontStyle = FontStyles.Italic
                });
                return document;
            }

            var table = new Table { CellSpacing = 0 };
            table.Columns.Add(new TableColumn { Width = new GridLength(0.12, GridUnitType.Star) });
            table.Columns.Add(new TableColumn { Width = new GridLength(0.18, GridUnitType.Star) });
            table.Columns.Add(new TableColumn { Width = new GridLength(0.14, GridUnitType.Star) });
            table.Columns.Add(new TableColumn { Width = new GridLength(0.26, GridUnitType.Star) });
            table.Columns.Add(new TableColumn { Width = new GridLength(0.20, GridUnitType.Star) });
            table.Columns.Add(new TableColumn { Width = new GridLength(0.10, GridUnitType.Star) });

            var rowGroup = new TableRowGroup();
            table.RowGroups.Add(rowGroup);

            var header = new TableRow { FontWeight = FontWeights.SemiBold, Background = System.Windows.Media.Brushes.LightGray };
            rowGroup.Rows.Add(header);
            AddCell(header, "User ID", true);
            AddCell(header, "User / Role", true);
            AddCell(header, "Security", true);
            AddCell(header, "Access", true);
            AddCell(header, "Contact", true);
            AddCell(header, "Active", true);

            var index = 0;
            foreach (var user in users)
            {
                var row = new TableRow();
                if (index % 2 == 1)
                    row.Background = System.Windows.Media.Brushes.WhiteSmoke;

                rowGroup.Rows.Add(row);
                AddCell(row, user.UserID.ToString());
                AddCell(row, $"{ValueOrNotRecorded(user.UserName)}\n{ResolveRole(user)}");
                AddCell(row, $"{ValueOrNotRecorded(user.LockoutStatus)}\nPassword expired: {FormatBool(user.PasswordExpired)}");
                AddCell(row, ValueOrNotRecorded(user.AccessSummary));
                AddCell(row, $"{ValueOrNotRecorded(user.Email)}\n{ValueOrNotRecorded(user.Phone)}");
                AddCell(row, FormatBool(user.IsActive));
                index++;
            }

            document.Blocks.Add(table);
            document.Blocks.Add(new Paragraph(new Run("Review access coverage, lockout state, disabled accounts, and any omitted rows before changing permissions or filing this directory packet."))
            {
                FontSize = 10,
                FontStyle = FontStyles.Italic,
                Margin = new Thickness(0, 10, 0, 0)
            });

            return document;
        }

        private static Table BuildSummarySection(string summary, int totalVisibleCount, int printedRowCount, int omittedRowCount)
        {
            var table = new Table { CellSpacing = 0, Margin = new Thickness(0, 0, 0, 10) };
            table.Columns.Add(new TableColumn { Width = new GridLength(0.25, GridUnitType.Star) });
            table.Columns.Add(new TableColumn { Width = new GridLength(0.75, GridUnitType.Star) });

            var group = new TableRowGroup();
            table.RowGroups.Add(group);
            AddSummaryLine(group, "Print Packet", summary);
            AddSummaryLine(group, "Total Visible Rows", totalVisibleCount.ToString());
            AddSummaryLine(group, "Printed Rows", printedRowCount.ToString());
            AddSummaryLine(group, "Omitted Rows", omittedRowCount == 0 ? "None" : $"{omittedRowCount} rows omitted to keep preview responsive");
            AddSummaryLine(group, "Large Directory Limit", $"First {MaxUsersPrintRows} visible rows");

            return table;
        }

        private static void AddSummaryLine(TableRowGroup group, string label, string value)
        {
            var row = new TableRow();
            group.Rows.Add(row);
            AddCell(row, label, true);
            AddCell(row, ValueOrNotRecorded(value));
        }

        private static void ShowPrintPreview(FlowDocument document, string title, string description)
        {
            new PrintPreviewWindow().ShowPreview(document, title, description);
        }

        private static void AddCell(TableRow row, string text, bool isHeader = false)
        {
            row.Cells.Add(new TableCell(new Paragraph(new Run(text ?? string.Empty))
            {
                Margin = new Thickness(2),
                FontWeight = isHeader ? FontWeights.SemiBold : FontWeights.Normal
            })
            {
                BorderBrush = System.Windows.Media.Brushes.Gray,
                BorderThickness = new Thickness(0, 0, 0, 0.5),
                Padding = new Thickness(3, 2, 3, 2)
            });
        }

        private static string ResolveRole(UserModel user)
        {
            if (!string.IsNullOrWhiteSpace(user.Role))
                return user.Role;

            return user.IsAdmin ? "Admin" : "User";
        }

        private static string FormatBool(bool value) => value ? "Yes" : "No";

        private static string FormatDate(DateTime? value) => value.HasValue ? value.Value.ToString("yyyy-MM-dd") : "-";

        private static string ValueOrDash(string? value) => string.IsNullOrWhiteSpace(value) ? "-" : value;

        private static string ValueOrNotRecorded(string? value) => string.IsNullOrWhiteSpace(value) ? "Not recorded" : value.Trim();
    }
}