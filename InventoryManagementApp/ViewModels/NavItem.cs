using System.Windows.Input;

namespace InventoryManagementApp.ViewModels
{
    public class NavItem
    {
        public NavItem(string title, ICommand command)
        {
            Title = title;
            Command = command;
        }

        public string Title { get; }
        public ICommand Command { get; }
        public int DisplayNumber { get; set; }
    }
}
