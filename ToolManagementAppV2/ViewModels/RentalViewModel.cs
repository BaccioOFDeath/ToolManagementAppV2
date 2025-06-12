using CommunityToolkit.Mvvm.ComponentModel;

namespace ToolManagementAppV2.ViewModels
{
    internal class RentalViewModel : ObservableObject
    {
        private RentalModel _rental;
        public RentalModel Rental
        {
            get => _rental;
            set => SetProperty(ref _rental, value);
        }

        public RentalViewModel(RentalModel rental)
        {
            _rental = rental;
        }

        public string StatusSummary => $"{_rental.Status} (Due: {_rental.DueDate:yyyy-MM-dd})";
    }
}
