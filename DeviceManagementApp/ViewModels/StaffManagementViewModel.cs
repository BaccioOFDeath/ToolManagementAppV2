using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeviceManagementApp.Interfaces;
using DeviceManagementApp.Models;
using Microsoft.VisualBasic;

namespace DeviceManagementApp.ViewModels
{
    public class StaffManagementViewModel : ObservableObject
    {
        readonly IStaffService _staffService;
        readonly IDialogService _dialogService;
        List<Staff> _allStaff = new();

        public ObservableCollection<Staff> StaffMembers { get; } = new();

        private Staff? _selectedStaff;
        public Staff? SelectedStaff
        {
            get => _selectedStaff;
            set
            {
                if (SetProperty(ref _selectedStaff, value))
                {
                    EditStaffCommand.NotifyCanExecuteChanged();
                }
            }
        }

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        public IAsyncRelayCommand LoadStaffCommand { get; }
        public IAsyncRelayCommand AddStaffCommand { get; }
        public IAsyncRelayCommand EditStaffCommand { get; }
        public IAsyncRelayCommand<Staff> EditStaffFromRowCommand { get; }
        public IAsyncRelayCommand<Staff> DeleteStaffFromRowCommand { get; }
        public IRelayCommand SearchStaffCommand { get; }
        public IRelayCommand ClearSearchCommand { get; }

        public StaffManagementViewModel(IStaffService staffService, IDialogService dialogService)
        {
            _staffService = staffService;
            _dialogService = dialogService;

            LoadStaffCommand = new AsyncRelayCommand(LoadStaffAsync);
            AddStaffCommand = new AsyncRelayCommand(AddStaffAsync);
            EditStaffCommand = new AsyncRelayCommand(() => EditStaffAsync(SelectedStaff), () => SelectedStaff != null);
            EditStaffFromRowCommand = new AsyncRelayCommand<Staff>(EditStaffAsync);
            DeleteStaffFromRowCommand = new AsyncRelayCommand<Staff>(DeleteStaffAsync);
            SearchStaffCommand = new RelayCommand(SearchStaff);
            ClearSearchCommand = new RelayCommand(ClearSearch);
        }

        public async Task LoadStaffAsync()
        {
            _allStaff = new List<Staff>(await _staffService.GetStaffAsync().ConfigureAwait(false));
            StaffMembers.Clear();
            foreach (var s in _allStaff)
                StaffMembers.Add(s);
        }

        async Task AddStaffAsync()
        {
            var name = Interaction.InputBox("Enter staff name:", "Add Staff", string.Empty);
            if (string.IsNullOrWhiteSpace(name))
                return;
            var staff = new Staff { Name = name };
            try
            {
                staff.StaffId = await _staffService.AddStaffAsync(staff).ConfigureAwait(false);
                _allStaff.Add(staff);
                StaffMembers.Add(staff);
            }
            catch (Exception ex)
            {
                _dialogService.ShowInfo($"Failed to add staff: {ex.Message}", "Staff");
            }
        }

        async Task EditStaffAsync(Staff? staff)
        {
            if (staff == null)
                return;
            var name = Interaction.InputBox("Enter staff name:", "Edit Staff", staff.Name);
            if (string.IsNullOrWhiteSpace(name))
                return;
            staff.Name = name;
            try
            {
                await _staffService.UpdateStaffAsync(staff).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _dialogService.ShowInfo($"Failed to update staff: {ex.Message}", "Staff");
            }
        }

        async Task DeleteStaffAsync(Staff? staff)
        {
            if (staff == null)
                return;
            if (!_dialogService.ShowConfirmation("Delete selected staff?", "Staff"))
                return;
            try
            {
                await _staffService.DeleteStaffAsync(staff.StaffId).ConfigureAwait(false);
                _allStaff.Remove(staff);
                StaffMembers.Remove(staff);
                if (ReferenceEquals(SelectedStaff, staff))
                    SelectedStaff = null;
            }
            catch (Exception ex)
            {
                _dialogService.ShowInfo($"Failed to delete staff: {ex.Message}", "Staff");
            }
        }

        void SearchStaff()
        {
            var term = SearchText?.Trim();
            IEnumerable<Staff> results = _allStaff;
            if (!string.IsNullOrEmpty(term))
                results = _allStaff.Where(s => s.Name.Contains(term, StringComparison.OrdinalIgnoreCase));
            StaffMembers.Clear();
            foreach (var s in results)
                StaffMembers.Add(s);
        }

        void ClearSearch()
        {
            SearchText = string.Empty;
            StaffMembers.Clear();
            foreach (var s in _allStaff)
                StaffMembers.Add(s);
        }
    }
}
