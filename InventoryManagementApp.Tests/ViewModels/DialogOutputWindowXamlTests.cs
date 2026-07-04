using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests.ViewModels
{
    public class DialogOutputWindowXamlTests
    {
        [Fact]
        public void MessageDialogs_UsePolishedHeadersFootersAndPreserveCommands()
        {
            var info = ReadRepositoryFile("InventoryManagementApp", "Views", "Windows", "InfoDialogWindow.xaml");
            var confirm = ReadRepositoryFile("InventoryManagementApp", "Views", "Windows", "ConfirmDialogWindow.xaml");
            var confirmCodeBehind = ReadRepositoryFile("InventoryManagementApp", "Views", "Windows", "ConfirmDialogWindow.xaml.cs");
            var input = ReadRepositoryFile("InventoryManagementApp", "Views", "Windows", "InputDialogWindow.xaml");

            Assert.Contains("Information Notice", info, StringComparison.Ordinal);
            Assert.Contains("Message reviewed when OK is selected.", info, StringComparison.Ordinal);
            Assert.Contains("OkCommand", info, StringComparison.Ordinal);

            Assert.Contains("Confirm Action", confirm, StringComparison.Ordinal);
            Assert.Contains("Action Review", confirm, StringComparison.Ordinal);
            Assert.Contains("CancelCommand", confirm, StringComparison.Ordinal);
            Assert.Contains("OkCommand", confirm, StringComparison.Ordinal);
            Assert.Contains("this.UseResponsiveDefaultSize(640, 480);", confirmCodeBehind, StringComparison.Ordinal);

            Assert.Contains("Input Required", input, StringComparison.Ordinal);
            Assert.Contains("Input is applied only after OK is selected.", input, StringComparison.Ordinal);
            Assert.Contains("InputText, UpdateSourceTrigger=PropertyChanged", input, StringComparison.Ordinal);
            Assert.Contains("CancelCommand", input, StringComparison.Ordinal);
            Assert.Contains("OkCommand", input, StringComparison.Ordinal);
        }

        [Fact]
        public void OutputAndMappingDialogs_UseWorkbenchStructureAndPreserveCommands()
        {
            var labels = ReadRepositoryFile("InventoryManagementApp", "Views", "Windows", "PrintLabelWindow.xaml");
            var mapping = ReadRepositoryFile("InventoryManagementApp", "Views", "Windows", "ImportMappingWindow.xaml");
            var imageMapping = ReadRepositoryFile("InventoryManagementApp", "Views", "Windows", "ImageImportMappingWindow.xaml");

            Assert.Contains("Label Output Workbench", labels, StringComparison.Ordinal);
            Assert.Contains("Queued Label Items", labels, StringComparison.Ordinal);
            Assert.Contains("PreviewCommand", labels, StringComparison.Ordinal);
            Assert.Contains("PrintCommand", labels, StringComparison.Ordinal);
            Assert.Contains("CloseCommand", labels, StringComparison.Ordinal);

            Assert.Contains("Import Mapping Workbench", mapping, StringComparison.Ordinal);
            Assert.Contains("01 Required Fields", mapping, StringComparison.Ordinal);
            Assert.Contains("Field Mapping Table", mapping, StringComparison.Ordinal);
            Assert.Contains("Import handoff", mapping, StringComparison.Ordinal);
            Assert.Contains("DesktopStatusFooter", mapping, StringComparison.Ordinal);
            Assert.Contains("DataContext.ColumnHeaders", mapping, StringComparison.Ordinal);
            Assert.Contains("SelectedColumn, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged", mapping, StringComparison.Ordinal);
            Assert.Contains("CancelCommand", mapping, StringComparison.Ordinal);
            Assert.Contains("OkCommand", mapping, StringComparison.Ordinal);

            Assert.Contains("Picture Matching Setup", imageMapping, StringComparison.Ordinal);
            Assert.Contains("Identifier Rules", imageMapping, StringComparison.Ordinal);
            Assert.Contains("Import Confidence", imageMapping, StringComparison.Ordinal);
            Assert.Contains("DesktopStatusFooter", imageMapping, StringComparison.Ordinal);
            Assert.Contains("UseItemNumber", imageMapping, StringComparison.Ordinal);
            Assert.Contains("UsePartNumber", imageMapping, StringComparison.Ordinal);
            Assert.Contains("UseName", imageMapping, StringComparison.Ordinal);
            Assert.Contains("CancelCommand", imageMapping, StringComparison.Ordinal);
            Assert.Contains("OkCommand", imageMapping, StringComparison.Ordinal);
        }

        [Fact]
        public void DetailDialog_UsesPolishedHandoffStructureAndCloseAction()
        {
            var detail = ReadRepositoryFile("InventoryManagementApp", "Views", "Windows", "DetailDialogWindow.xaml");
            var codeBehind = ReadRepositoryFile("InventoryManagementApp", "Views", "Windows", "DetailDialogWindow.xaml.cs");

            Assert.Contains("Workflow Detail", detail, StringComparison.Ordinal);
            Assert.Contains("Selected Row Handoff", detail, StringComparison.Ordinal);
            Assert.Contains("Close returns to the current screen with the same row context.", detail, StringComparison.Ordinal);
            Assert.Contains("Close_Click", detail, StringComparison.Ordinal);
            Assert.Contains("ShowDialogFor", codeBehind, StringComparison.Ordinal);
        }

        [Fact]
        public void EditDialogs_UsePolishedFormStructureAndPreserveBindings()
        {
            var item = ReadRepositoryFile("InventoryManagementApp", "Views", "Windows", "ItemEditWindow.xaml");
            var customer = ReadRepositoryFile("InventoryManagementApp", "Views", "Windows", "CustomerEditWindow.xaml");
            var user = ReadRepositoryFile("InventoryManagementApp", "Views", "Windows", "UsersEditWindow.xaml");
            var maintenance = ReadRepositoryFile("InventoryManagementApp", "Views", "Windows", "MaintenanceEditWindow.xaml");
            var calibration = ReadRepositoryFile("InventoryManagementApp", "Views", "Windows", "CalibrationEditWindow.xaml");
            var kit = ReadRepositoryFile("InventoryManagementApp", "Views", "Windows", "KitEditWindow.xaml");
            var kitItem = ReadRepositoryFile("InventoryManagementApp", "Views", "Windows", "KitItemEditWindow.xaml");
            var saveCancel = ReadRepositoryFile("InventoryManagementApp", "Views", "Controls", "SaveCancelBar.xaml");
            var customerCode = ReadRepositoryFile("InventoryManagementApp", "Views", "Windows", "CustomerEditWindow.xaml.cs");
            var itemCode = ReadRepositoryFile("InventoryManagementApp", "Views", "Windows", "ItemEditWindow.xaml.cs");
            var userCode = ReadRepositoryFile("InventoryManagementApp", "Views", "Windows", "UsersEditWindow.xaml.cs");
            var maintenanceCode = ReadRepositoryFile("InventoryManagementApp", "Views", "Windows", "MaintenanceEditWindow.xaml.cs");
            var calibrationCode = ReadRepositoryFile("InventoryManagementApp", "Views", "Windows", "CalibrationEditWindow.xaml.cs");
            var kitCode = ReadRepositoryFile("InventoryManagementApp", "Views", "Windows", "KitEditWindow.xaml.cs");
            var kitItemCode = ReadRepositoryFile("InventoryManagementApp", "Views", "Windows", "KitItemEditWindow.xaml.cs");
            var reservationCode = ReadRepositoryFile("InventoryManagementApp", "Views", "Windows", "ReservationEditWindow.xaml.cs");
            var rentCode = ReadRepositoryFile("InventoryManagementApp", "Views", "Windows", "RentItemPopupWindow.xaml.cs");

            Assert.Contains("Item Edit Workbench", item, StringComparison.Ordinal);
            Assert.Contains("Inventory Identity", item, StringComparison.Ordinal);
            Assert.Contains("Image and Availability", item, StringComparison.Ordinal);
            Assert.Contains("DesktopStatusFooter", item, StringComparison.Ordinal);
            Assert.Contains("ItemModel.ItemNumber, UpdateSourceTrigger=PropertyChanged", item, StringComparison.Ordinal);
            Assert.Contains("ItemModel.MissingComponentsNotes, UpdateSourceTrigger=PropertyChanged", item, StringComparison.Ordinal);
            Assert.Contains("BrowseImageCommand", item, StringComparison.Ordinal);
            Assert.Contains("RemoveImageCommand", item, StringComparison.Ordinal);

            Assert.Contains("Customer Profile Workbench", customer, StringComparison.Ordinal);
            Assert.Contains("01 Account", customer, StringComparison.Ordinal);
            Assert.Contains("Customer Operations Handoff", customer, StringComparison.Ordinal);
            Assert.Contains("DesktopStatusFooter", customer, StringComparison.Ordinal);
            Assert.Contains("Customer.Company, UpdateSourceTrigger=PropertyChanged", customer, StringComparison.Ordinal);
            Assert.Contains("Customer.Contact, UpdateSourceTrigger=PropertyChanged", customer, StringComparison.Ordinal);
            Assert.Contains("Customer.Email, UpdateSourceTrigger=PropertyChanged", customer, StringComparison.Ordinal);
            Assert.Contains("Customer.Address, UpdateSourceTrigger=PropertyChanged", customer, StringComparison.Ordinal);

            Assert.Contains("01 Identity", user, StringComparison.Ordinal);
            Assert.Contains("Permission Checklist", user, StringComparison.Ordinal);
            Assert.Contains("Profile Readiness", user, StringComparison.Ordinal);
            Assert.Contains("DesktopStatusFooter", user, StringComparison.Ordinal);
            Assert.Contains("BrowseImageCommand", user, StringComparison.Ordinal);
            Assert.Contains("RemoveImageCommand", user, StringComparison.Ordinal);
            Assert.Contains("EditingUser.UserName, UpdateSourceTrigger=PropertyChanged", user, StringComparison.Ordinal);
            Assert.Contains("EditingUser.IsAdmin, Mode=TwoWay", user, StringComparison.Ordinal);
            Assert.Contains("CanManageItems, Mode=TwoWay", user, StringComparison.Ordinal);
            Assert.Contains("CanUseSettings, Mode=TwoWay", user, StringComparison.Ordinal);
            Assert.Contains("WorkflowImpactSummary", user, StringComparison.Ordinal);
            Assert.Contains("SaveCancelBar", user, StringComparison.Ordinal);

            Assert.Contains("Maintenance Work Order", maintenance, StringComparison.Ordinal);
            Assert.Contains("Technician Handoff", maintenance, StringComparison.Ordinal);
            Assert.Contains("DesktopStatusFooter", maintenance, StringComparison.Ordinal);
            Assert.Contains("MaintenanceRecord.ItemNumber, UpdateSourceTrigger=PropertyChanged", maintenance, StringComparison.Ordinal);
            Assert.Contains("MaintenanceRecord.Notes, UpdateSourceTrigger=PropertyChanged", maintenance, StringComparison.Ordinal);
            Assert.Contains("MaintenanceTypeOptions", maintenance, StringComparison.Ordinal);
            Assert.Contains("StatusOptions", maintenance, StringComparison.Ordinal);

            Assert.Contains("Calibration Certificate", calibration, StringComparison.Ordinal);
            Assert.Contains("Verification Notes", calibration, StringComparison.Ordinal);
            Assert.Contains("DesktopStatusFooter", calibration, StringComparison.Ordinal);
            Assert.Contains("CalibrationRecord.CertificateNumber, UpdateSourceTrigger=PropertyChanged", calibration, StringComparison.Ordinal);
            Assert.Contains("CalibrationRecord.Notes, UpdateSourceTrigger=PropertyChanged", calibration, StringComparison.Ordinal);
            Assert.Contains("ResultOptions", calibration, StringComparison.Ordinal);

            Assert.Contains("Kit Setup", kit, StringComparison.Ordinal);
            Assert.Contains("01 Identity", kit, StringComparison.Ordinal);
            Assert.Contains("Kit Identity", kit, StringComparison.Ordinal);
            Assert.Contains("Operations Handoff", kit, StringComparison.Ordinal);
            Assert.Contains("DesktopStatusFooter", kit, StringComparison.Ordinal);
            Assert.Contains("Kit.KitNumber, UpdateSourceTrigger=PropertyChanged", kit, StringComparison.Ordinal);
            Assert.Contains("Kit.Description, UpdateSourceTrigger=PropertyChanged", kit, StringComparison.Ordinal);
            Assert.Contains("Kit.IsActive", kit, StringComparison.Ordinal);

            Assert.Contains("Kit Item Membership", kitItem, StringComparison.Ordinal);
            Assert.Contains("01 Item", kitItem, StringComparison.Ordinal);
            Assert.Contains("Membership Details", kitItem, StringComparison.Ordinal);
            Assert.Contains("Pick Sheet Handoff", kitItem, StringComparison.Ordinal);
            Assert.Contains("DesktopStatusFooter", kitItem, StringComparison.Ordinal);
            Assert.Contains("KitItem.ItemNumber, UpdateSourceTrigger=PropertyChanged", kitItem, StringComparison.Ordinal);
            Assert.Contains("KitItem.Quantity, UpdateSourceTrigger=PropertyChanged", kitItem, StringComparison.Ordinal);
            Assert.Contains("KitItem.IsOptional", kitItem, StringComparison.Ordinal);

            Assert.Contains("Review changes, then save or cancel", saveCancel, StringComparison.Ordinal);
            Assert.Contains("SaveCommand", saveCancel, StringComparison.Ordinal);
            Assert.Contains("CancelCommand", saveCancel, StringComparison.Ordinal);
            Assert.Contains("Width=\"104\"", saveCancel, StringComparison.Ordinal);
            Assert.Contains("UseResponsiveDefaultSize(900, 760)", customerCode, StringComparison.Ordinal);
            Assert.Contains("UseResponsiveDefaultSize(840, 720)", itemCode, StringComparison.Ordinal);
            Assert.Contains("UseResponsiveDefaultSize(1120, 880)", userCode, StringComparison.Ordinal);
            Assert.Contains("UseResponsiveDefaultSize(880, 720)", maintenanceCode, StringComparison.Ordinal);
            Assert.Contains("UseResponsiveDefaultSize(880, 720)", calibrationCode, StringComparison.Ordinal);
            Assert.Contains("UseResponsiveDefaultSize(880, 720)", kitCode, StringComparison.Ordinal);
            Assert.Contains("UseResponsiveDefaultSize(760, 660)", kitItemCode, StringComparison.Ordinal);
            Assert.Contains("UseResponsiveDefaultSize(820, 640)", reservationCode, StringComparison.Ordinal);
            Assert.Contains("UseResponsiveDefaultSize(1040, 760)", rentCode, StringComparison.Ordinal);
        }

        [Fact]
        public void RentCheckoutDialog_UsesPolishedHandoffStructureAndPreservesCommands()
        {
            var rent = ReadRepositoryFile("InventoryManagementApp", "Views", "Windows", "RentItemPopupWindow.xaml");

            Assert.Contains("Rental Checkout", rent, StringComparison.Ordinal);
            Assert.Contains("01 Customer directory", rent, StringComparison.Ordinal);
            Assert.Contains("Selected Customer Handoff", rent, StringComparison.Ordinal);
            Assert.Contains("Checkout Readiness", rent, StringComparison.Ordinal);
            Assert.Contains("DesktopStatusFooter", rent, StringComparison.Ordinal);
            Assert.Contains("CustomerSearchText, UpdateSourceTrigger=PropertyChanged", rent, StringComparison.Ordinal);
            Assert.Contains("ClearCustomerSearchCommand", rent, StringComparison.Ordinal);
            Assert.Contains("AddCustomerCommand", rent, StringComparison.Ordinal);
            Assert.Contains("SetRentalDaysCommand", rent, StringComparison.Ordinal);
            Assert.Contains("SelectedDueDate, Mode=TwoWay", rent, StringComparison.Ordinal);
            Assert.Contains("RentalDays, UpdateSourceTrigger=PropertyChanged", rent, StringComparison.Ordinal);
            Assert.Contains("CheckOutCommand", rent, StringComparison.Ordinal);
            Assert.Contains("CancelCommand", rent, StringComparison.Ordinal);
        }

        [Fact]
        public void RentalHistoryDialog_UsesWorkbenchStructureAndPreservesActions()
        {
            var history = ReadRepositoryFile("InventoryManagementApp", "Views", "Windows", "RentalHistoryWindow.xaml");

            Assert.Contains("Rental History Workbench", history, StringComparison.Ordinal);
            Assert.Contains("01 Current View", history, StringComparison.Ordinal);
            Assert.Contains("Rental Records", history, StringComparison.Ordinal);
            Assert.Contains("DesktopSectionActionStrip", history, StringComparison.Ordinal);
            Assert.Contains("DesktopStatusFooter", history, StringComparison.Ordinal);
            Assert.Contains("SearchText, UpdateSourceTrigger=PropertyChanged", history, StringComparison.Ordinal);
            Assert.Contains("SearchCommand", history, StringComparison.Ordinal);
            Assert.Contains("ClearSearchCommand", history, StringComparison.Ordinal);
            Assert.Contains("OpenDetailsCommand", history, StringComparison.Ordinal);
            Assert.Contains("ExportCsvCommand", history, StringComparison.Ordinal);
            Assert.Contains("CloseCommand", history, StringComparison.Ordinal);
            Assert.Contains("HistoryRow_MouseDoubleClick", history, StringComparison.Ordinal);
            Assert.Contains("HistoryRow_PreviewMouseRightButtonDown", history, StringComparison.Ordinal);
        }

        [Fact]
        public void SelectedRowDetailActions_RouteThroughPolishedDetailDialog()
        {
            var activity = ReadRepositoryFile("InventoryManagementApp", "Views", "Pages", "ActivityLogsPage.xaml.cs");
            var categories = ReadRepositoryFile("InventoryManagementApp", "Views", "Pages", "CategoriesPage.xaml.cs");
            var importExport = ReadRepositoryFile("InventoryManagementApp", "Views", "Pages", "ImportExportPage.xaml.cs");
            var users = ReadRepositoryFile("InventoryManagementApp", "Views", "Pages", "UsersPage.xaml.cs");

            Assert.Contains("DetailDialogWindow.ShowDialogFor", activity, StringComparison.Ordinal);
            Assert.Contains("Activity Detail", activity, StringComparison.Ordinal);
            Assert.DoesNotContain("WpfMessageBox.Show(FormatLogDetail", activity, StringComparison.Ordinal);

            Assert.Contains("DetailDialogWindow.ShowDialogFor", categories, StringComparison.Ordinal);
            Assert.Contains("Category Detail", categories, StringComparison.Ordinal);
            Assert.DoesNotContain("WpfMessageBox.Show(FormatCategoryDetail", categories, StringComparison.Ordinal);

            Assert.Contains("DetailDialogWindow.ShowDialogFor", importExport, StringComparison.Ordinal);
            Assert.Contains("Import / Export Result", importExport, StringComparison.Ordinal);
            Assert.DoesNotContain("WpfMessageBox.Show(log,", importExport, StringComparison.Ordinal);

            Assert.Contains("DetailDialogWindow.ShowDialogFor", users, StringComparison.Ordinal);
            Assert.Contains("User Detail", users, StringComparison.Ordinal);
            Assert.DoesNotContain("WpfMessageBox.Show(FormatUserDetail", users, StringComparison.Ordinal);
        }

        static string ReadRepositoryFile(params string[] relativePathParts)
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null && !File.Exists(Path.Combine(directory.FullName, "InventoryManagementApp.sln")))
                directory = directory.Parent;

            Assert.NotNull(directory);
            var path = Path.Combine(directory!.FullName, Path.Combine(relativePathParts));
            Assert.True(File.Exists(path), $"Expected repository file at {path}");
            return File.ReadAllText(path);
        }
    }
}
