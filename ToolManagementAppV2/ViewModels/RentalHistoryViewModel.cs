using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ToolManagementAppV2.Models.Domain;

namespace ToolManagementAppV2.ViewModels.Rental
{
    public class RentalHistoryViewModel : ObservableObject
    {
        public ObservableCollection<RentalModel> History { get; }
        public string ToolDisplayName { get; }

        public RentalHistoryViewModel(ToolModel? tool, IEnumerable<RentalModel>? history)
        {
            ToolDisplayName = tool != null
                ? $"{tool.ToolNumber} - {tool.NameDescription}"
                : "Rental History";

            History = new ObservableCollection<RentalModel>(history ?? Enumerable.Empty<RentalModel>());
        }
    }
}
