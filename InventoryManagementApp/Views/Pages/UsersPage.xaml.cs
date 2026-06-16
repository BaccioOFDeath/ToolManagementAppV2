using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.ViewModels;
using WpfMessageBox = System.Windows.MessageBox;
using WpfPrintDialog = System.Windows.Controls.PrintDialog;

namespace InventoryManagementApp.Views.Pages
{
    /// <summary>
    /// Page for viewing and managing users.
    /// The <see cref="DataContext"/> is expected to be a <see cref="UserManagementViewModel"/>.
    /// </summary>
    public partial class UsersPage : Page
    {
        public UsersPage(UserManagementViewModel? viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }

        /// <summary>
        /// Convenience accessor for the strongly typed view model.
        /// Returns null if the DataContext is not correctly set.
        /// </summary>
        public UserManagementViewModel? ViewModel =>
            DataContext as UserManagementViewModel;

        private void UserRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenSelectedUser_Click(sender, e);
        }

        private void UserRow_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGridRow row && !row.IsSelected)
            {
                row.IsSelected = true;
                e.Handled = true;
            }
        }

        private void OpenSelectedUser_Click(object sender, RoutedEventArgs e)
        {
            if (UsersDataGrid.SelectedItem is not UserModel user)
            {
                WpfMessageBox.Show("Select a user row first.", "User Directory", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            WpfMessageBox.Show(FormatUserDetail(user), $"User Detail - {user.UserName}", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CopySelectedUser_Click(object sender, RoutedEventArgs e)
        {
            if (UsersDataGrid.SelectedItem is not UserModel user)
            {
                WpfMessageBox.Show("Select a user row first.", "User Directory", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            Clipboard.SetText(FormatUserDetail(user));
        }

        private async void ResetSelectedUser_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel == null || UsersDataGrid.SelectedItem is not UserModel user)
            {
                WpfMessageBox.Show("Select a user row first.", "User Directory", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            await ViewModel.ResetPasswordFromRowCommand.ExecuteAsync(user);
        }

        private void PrintUsers_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel == null || ViewModel.Users.Count == 0)
            {
                WpfMessageBox.Show("There are no users to print.", "User Directory", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var printDialog = new WpfPrintDialog();
            if (printDialog.ShowDialog() != true)
                return;

            var document = BuildPrintDocument(ViewModel.Users.ToList(), $"Visible users: {ViewModel.Users.Count}");
            document.PageWidth = printDialog.PrintableAreaWidth;
            document.PageHeight = printDialog.PrintableAreaHeight;
            document.PagePadding = new Thickness(36);
            document.ColumnWidth = printDialog.PrintableAreaWidth;
            printDialog.PrintDocument(((IDocumentPaginatorSource)document).DocumentPaginator, "User Directory");
        }

        private static string FormatUserDetail(UserModel user)
        {
            return $"User #: {user.UserID}{Environment.NewLine}" +
                   $"Name: {user.UserName}{Environment.NewLine}" +
                   $"Role: {ResolveRole(user)}{Environment.NewLine}" +
                   $"Active: {FormatBool(user.IsActive)}{Environment.NewLine}" +
                   $"Admin: {FormatBool(user.IsAdmin)}{Environment.NewLine}" +
                   $"Password expired: {FormatBool(user.PasswordExpired)}{Environment.NewLine}" +
                   $"Created: {FormatDate(user.CreatedAt)}{Environment.NewLine}{Environment.NewLine}" +
                   $"Email: {ValueOrDash(user.Email)}{Environment.NewLine}" +
                   $"Phone: {ValueOrDash(user.Phone)}{Environment.NewLine}" +
                   $"Mobile: {ValueOrDash(user.Mobile)}{Environment.NewLine}" +
                   $"Address: {ValueOrDash(user.Address)}{Environment.NewLine}{Environment.NewLine}" +
                   "Next steps: edit profile details, upload a current photo, reset the password if the user is blocked, or review activity logs for recent account actions.";
        }

        private static FlowDocument BuildPrintDocument(IReadOnlyCollection<UserModel> users, string summary)
        {
            var document = new FlowDocument
            {
                FontFamily = new System.Windows.Media.FontFamily("Segoe UI"),
                FontSize = 10
            };

            document.Blocks.Add(new Paragraph(new Run("User Directory"))
            {
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 4)
            });
            document.Blocks.Add(new Paragraph(new Run($"Printed {DateTime.Now:g} - {summary}"))
            {
                FontSize = 10,
                Margin = new Thickness(0, 0, 0, 10)
            });

            var table = new Table { CellSpacing = 0 };
            foreach (var width in new[] { 55.0, 140.0, 95.0, 210.0, 115.0, 115.0, 70.0 })
                table.Columns.Add(new TableColumn { Width = new GridLength(width) });

            var rowGroup = new TableRowGroup();
            table.RowGroups.Add(rowGroup);

            var header = new TableRow { FontWeight = FontWeights.SemiBold };
            rowGroup.Rows.Add(header);
            AddCell(header, "ID");
            AddCell(header, "User");
            AddCell(header, "Role");
            AddCell(header, "Email");
            AddCell(header, "Phone");
            AddCell(header, "Mobile");
            AddCell(header, "Active");

            foreach (var user in users)
            {
                var row = new TableRow();
                rowGroup.Rows.Add(row);
                AddCell(row, user.UserID.ToString());
                AddCell(row, user.UserName);
                AddCell(row, ResolveRole(user));
                AddCell(row, user.Email);
                AddCell(row, user.Phone);
                AddCell(row, user.Mobile);
                AddCell(row, FormatBool(user.IsActive));
            }

            document.Blocks.Add(table);
            return document;
        }

        private static void AddCell(TableRow row, string text)
        {
            row.Cells.Add(new TableCell(new Paragraph(new Run(text ?? string.Empty))
            {
                Margin = new Thickness(2)
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
    }
}