using System;
using System.Collections.Generic;

namespace ToolManagementAppV2.Interfaces
{
    public interface IDialogService
    {
        void ShowInfo(string message, string title);
        bool ShowConfirmation(string message, string title);
        ToolModel? ShowEditToolDialog(ToolModel tool);
        void ShowToolDetails(ToolModel tool);
        (CustomerModel customer, DateTime dueDate)? ShowRentToolDialog(ToolModel tool, IEnumerable<CustomerModel> customers);
        CustomerModel? ShowAddCustomerDialog();
    }
}
