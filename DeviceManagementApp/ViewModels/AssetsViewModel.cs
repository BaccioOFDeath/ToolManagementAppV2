using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeviceManagementApp.Interfaces;
using DeviceManagementApp.Models;

namespace DeviceManagementApp.ViewModels
{
    public class AssetsViewModel : ObservableObject
    {
        readonly IAssetService _assetService;
        readonly IAssetAssignmentService _assignmentService;
        readonly IDialogService _dialogService;
        readonly IStaffService _staffService;

        public ObservableCollection<Asset> Assets { get; } = new();
        public ObservableCollection<KeyValuePair<int?, string>> Staff { get; } = new() { new(null, "All Staff") };

        public ICollectionView AssetsView { get; }

        private Asset? _selectedAsset;
        public Asset? SelectedAsset
        {
            get => _selectedAsset;
            set
            {
                if (SetProperty(ref _selectedAsset, value))
                {
                    DeleteAssetCommand.NotifyCanExecuteChanged();
                    AssignAssetCommand.NotifyCanExecuteChanged();
                    ReturnAssetCommand.NotifyCanExecuteChanged();
                }
            }
        }


        private int? _assignedUserFilter;
        public int? AssignedUserFilter
        {
            get => _assignedUserFilter;
            set { SetProperty(ref _assignedUserFilter, value); AssetsView.Refresh(); }
        }

        public IAsyncRelayCommand RefreshCommand { get; }
        public IAsyncRelayCommand AddAssetCommand { get; }
        public IAsyncRelayCommand DeleteAssetCommand { get; }
        public IAsyncRelayCommand AssignAssetCommand { get; }
        public IAsyncRelayCommand ReturnAssetCommand { get; }

        public AssetsViewModel(IAssetService assetService, IAssetAssignmentService assignmentService, IDialogService dialogService, IStaffService staffService)
        {
            _assetService = assetService;
            _assignmentService = assignmentService;
            _dialogService = dialogService;
            _staffService = staffService;
            AssetsView = CollectionViewSource.GetDefaultView(Assets);
            AssetsView.Filter = FilterAsset;
            RefreshCommand = new AsyncRelayCommand(LoadAssetsAsync);
            AddAssetCommand = new AsyncRelayCommand(AddAssetAsync);
            DeleteAssetCommand = new AsyncRelayCommand(DeleteAssetAsync, () => SelectedAsset != null);
            AssignAssetCommand = new AsyncRelayCommand(AssignAssetAsync, () => SelectedAsset != null);
            ReturnAssetCommand = new AsyncRelayCommand(ReturnAssetAsync, () => SelectedAsset != null && SelectedAsset.AssignedUserId != null);
        }

        public async Task LoadAssetsAsync()
        {
            Assets.Clear();
            foreach (var d in await _assetService.GetAssetsAsync())
                Assets.Add(d);
            Staff.Clear();
            Staff.Add(new(null, "All Staff"));
            foreach (var s in await _staffService.GetStaffAsync())
                Staff.Add(new(s.StaffId, s.Name));
        }

        async Task AddAssetAsync()
        {
            var asset = new Asset { Name = "New Asset" };
            await _assetService.AddOrUpdateAssetAsync(asset);
            Assets.Add(asset);
        }

        async Task DeleteAssetAsync()
        {
            if (SelectedAsset == null) return;
            await _assetService.DeleteAssetAsync(SelectedAsset.AssetId);
            Assets.Remove(SelectedAsset);
        }

        async Task AssignAssetAsync()
        {
            if (SelectedAsset == null || SelectedAsset.AssignedUserId == null) return;
            var assignment = new AssetAssignment
            {
                AssetId = SelectedAsset.AssetId,
                UserId = SelectedAsset.AssignedUserId.Value,
                AssignedDate = DateTime.UtcNow,
                DepartmentId = SelectedAsset.DepartmentId
            };
            await _assignmentService.AssignAsync(assignment);
            ReturnAssetCommand.NotifyCanExecuteChanged();
        }

        async Task ReturnAssetAsync()
        {
            if (SelectedAsset == null) return;
            await _assignmentService.ReturnAsync(SelectedAsset.AssetId);
            SelectedAsset.AssignedUserId = null;
            SelectedAsset.DepartmentId = null;
            ReturnAssetCommand.NotifyCanExecuteChanged();
            AssignAssetCommand.NotifyCanExecuteChanged();
            AssetsView.Refresh();
        }

        bool FilterAsset(object obj)
        {
            if (obj is not Asset a) return false;
            if (AssignedUserFilter.HasValue && a.AssignedUserId != AssignedUserFilter) return false;
            return true;
        }
    }
}
