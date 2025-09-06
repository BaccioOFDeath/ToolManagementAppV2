using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using DeviceManagementApp.Interfaces;

namespace DeviceManagementApp.Resources
{
    public class LabelProvider : ObservableObject
    {
        public static LabelProvider Instance { get; } = new LabelProvider();

        private LabelProvider() { }

        private string _pageLabel = string.Empty;
        public string PageLabel
        {
            get => _pageLabel;
            private set => SetProperty(ref _pageLabel, value);
        }

        private string _tooltipLabel = string.Empty;
        public string TooltipLabel
        {
            get => _tooltipLabel;
            private set => SetProperty(ref _tooltipLabel, value);
        }

        private string _itemLabelSingular = "Item";
        public string ItemLabelSingular
        {
            get => _itemLabelSingular;
            private set => SetProperty(ref _itemLabelSingular, value);
        }

        private string _itemLabelPlural = "Items";
        public string ItemLabelPlural
        {
            get => _itemLabelPlural;
            private set => SetProperty(ref _itemLabelPlural, value);
        }

        public async Task InitializeAsync(ISettingsService settingsService)
        {
            var singular = await settingsService.GetItemLabelSingularAsync().ConfigureAwait(false);
            var plural = await settingsService.GetItemLabelPluralAsync().ConfigureAwait(false);
            if (!string.IsNullOrWhiteSpace(singular))
                ItemLabelSingular = singular;
            if (!string.IsNullOrWhiteSpace(plural))
                ItemLabelPlural = plural;
        }

        public void UpdateLabels(string pageLabel, string tooltipLabel, string singular, string plural)
        {
            PageLabel = pageLabel;
            TooltipLabel = tooltipLabel;
            ItemLabelSingular = singular;
            ItemLabelPlural = plural;
        }
    }
}
