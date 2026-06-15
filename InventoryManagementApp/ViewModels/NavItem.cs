using System.Collections.Generic;
using System.Windows.Input;

namespace InventoryManagementApp.ViewModels
{
    public class NavItem
    {
        static readonly HashSet<string> TerminalSectionTitles = new()
        {
            "Dashboard",
            "Print Labels",
            "Activity Logs",
            "Import / Export",
            "Settings"
        };

        static int _nextDisplayNumber = 1;
        static string? _previousTitle;

        public NavItem(string title, ICommand command)
        {
            if (_previousTitle == null || TerminalSectionTitles.Contains(_previousTitle))
                _nextDisplayNumber = 1;

            Title = title;
            Command = command;
            DisplayNumber = _nextDisplayNumber++;
            _previousTitle = title;
        }

        public string Title { get; }
        public ICommand Command { get; }
        public int DisplayNumber { get; }
    }
}
