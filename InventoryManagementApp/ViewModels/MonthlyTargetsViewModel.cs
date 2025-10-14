using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models.Domain;

namespace InventoryManagementApp.ViewModels
{
    public partial class MonthlyTargetsViewModel : ObservableObject
    {
        readonly IMonthlyTargetService _monthlyTargetService;
        readonly ISettingsService _settingsService;
        readonly IDialogService _dialogService;
        CancellationTokenSource? _loadCts;

        public ObservableCollection<MonthlyTargetEntryViewModel> MonthlyTargets { get; } = new();
        public ObservableCollection<FinancialYearOption> FinancialYears { get; } = new();
        public ObservableCollection<MonthOption> StartMonths { get; } = new();

        [ObservableProperty]
        private FinancialYearOption? selectedFinancialYear;

        partial void OnSelectedFinancialYearChanged(FinancialYearOption? value)
        {
            SaveTargetsCommand.NotifyCanExecuteChanged();
            if (value != null)
                _ = LoadTargetsForYearAsync(value.StartYear);
        }

        [ObservableProperty]
        private MonthOption? selectedStartMonth;

        partial void OnSelectedStartMonthChanged(MonthOption? value)
        {
            if (value != null)
            {
                BuildFinancialYears(value.Month);
                if (SelectedFinancialYear != null)
                    _ = LoadTargetsForYearAsync(SelectedFinancialYear.StartYear);
            }
        }

        [ObservableProperty]
        private bool isBusy;

        partial void OnIsBusyChanged(bool value)
        {
            SaveTargetsCommand.NotifyCanExecuteChanged();
        }

        [ObservableProperty]
        private string statusMessage = string.Empty;

        public IAsyncRelayCommand SaveTargetsCommand { get; }
        public IAsyncRelayCommand InitializeCommand { get; }

        public MonthlyTargetsViewModel(IMonthlyTargetService monthlyTargetService,
                                       ISettingsService settingsService,
                                       IDialogService dialogService)
        {
            _monthlyTargetService = monthlyTargetService ?? throw new ArgumentNullException(nameof(monthlyTargetService));
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

            SaveTargetsCommand = new AsyncRelayCommand(SaveTargetsAsync, () => !IsBusy && SelectedFinancialYear != null);
            InitializeCommand = new AsyncRelayCommand(InitializeAsync);
        }

        async Task InitializeAsync()
        {
            if (IsBusy)
                return;

            IsBusy = true;
            try
            {
                var startMonthSetting = await _settingsService.GetSettingAsync("FiscalYearStartMonth");
                var startMonth = int.TryParse(startMonthSetting, NumberStyles.Integer, CultureInfo.InvariantCulture, out var month)
                    ? Math.Clamp(month, 1, 12)
                    : 7;

                PopulateStartMonths(startMonth);
                BuildFinancialYears(startMonth);
                var currentYear = SelectedFinancialYear?.StartYear;
                if (currentYear.HasValue)
                    await LoadTargetsForYearAsync(currentYear.Value);
            }
            finally
            {
                IsBusy = false;
            }
        }

        void PopulateStartMonths(int selectedMonth)
        {
            StartMonths.Clear();
            for (var month = 1; month <= 12; month++)
            {
                var option = new MonthOption(month);
                StartMonths.Add(option);
                if (month == selectedMonth)
                    SelectedStartMonth = option;
            }
        }

        void BuildFinancialYears(int startMonth)
        {
            var today = DateTime.Today;
            var startYear = today.Month >= startMonth ? today.Year : today.Year - 1;
            var existingSelection = SelectedFinancialYear?.StartYear;
            FinancialYears.Clear();
            for (var year = startYear - 1; year <= startYear + 2; year++)
            {
                FinancialYears.Add(new FinancialYearOption(year, startMonth));
            }

            var selected = FinancialYears.FirstOrDefault(f => f.StartYear == (existingSelection ?? startYear));
            SelectedFinancialYear = selected ?? FinancialYears.FirstOrDefault();
        }

        async Task LoadTargetsForYearAsync(int financialYearStart)
        {
            if (SelectedStartMonth == null)
                return;

            _loadCts?.Cancel();
            _loadCts?.Dispose();
            _loadCts = new CancellationTokenSource();
            var token = _loadCts.Token;

            try
            {
                IsBusy = true;
                StatusMessage = "Loading targets...";
                var targets = await _monthlyTargetService.GetTargetsAsync(financialYearStart, token);
                var targetMap = targets.ToDictionary(t => t.MonthOffset);
                MonthlyTargets.Clear();

                var month = SelectedStartMonth.Month;
                var year = financialYearStart;
                for (var offset = 0; offset < 12; offset++)
                {
                    var entry = new MonthlyTargetEntryViewModel
                    {
                        MonthOffset = offset,
                        Month = month,
                        Year = year,
                        TargetAmount = targetMap.TryGetValue(offset, out var target) ? target.TargetAmount : 0m
                    };
                    entry.UpdateDisplay();
                    MonthlyTargets.Add(entry);
                    month++;
                    if (month > 12)
                    {
                        month = 1;
                        year++;
                    }
                }

                StatusMessage = string.Empty;
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                IsBusy = false;
                if (ReferenceEquals(_loadCts?.Token, token))
                {
                    _loadCts.Dispose();
                    _loadCts = null;
                }
                if (StatusMessage == "Loading targets...")
                    StatusMessage = string.Empty;
            }
        }

        async Task SaveTargetsAsync()
        {
            if (SelectedFinancialYear == null || SelectedStartMonth == null)
                return;

            if (IsBusy)
                return;

            IsBusy = true;
            try
            {
                StatusMessage = "Saving targets...";
                var targets = MonthlyTargets.Select(entry => new MonthlyTarget
                {
                    FinancialYearStart = SelectedFinancialYear.StartYear,
                    MonthOffset = entry.MonthOffset,
                    Month = entry.Month,
                    Year = entry.Year,
                    TargetAmount = entry.TargetAmount
                }).ToList();

                await _monthlyTargetService.SaveTargetsAsync(SelectedFinancialYear.StartYear, targets);

                await _settingsService.SaveSettingAsync("FiscalYearStartMonth",
                    SelectedStartMonth.Month.ToString(CultureInfo.InvariantCulture));

                _dialogService.ShowInfo("Monthly targets saved successfully.", "Monthly Targets");
                StatusMessage = string.Empty;
            }
            finally
            {
                if (StatusMessage == "Saving targets...")
                    StatusMessage = string.Empty;
                IsBusy = false;
            }
        }

        public record FinancialYearOption(int StartYear, int StartMonth)
        {
            public string DisplayName { get; } = $"FY {StartYear}/{StartYear + 1}";
        }

        public record MonthOption(int Month)
        {
            public string DisplayName { get; } = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Month);
        }
    }
}
