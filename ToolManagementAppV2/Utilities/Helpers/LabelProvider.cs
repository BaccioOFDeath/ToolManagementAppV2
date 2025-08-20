using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using ToolManagementAppV2.Interfaces;

namespace ToolManagementAppV2.Utilities.Helpers
{
    public class LabelProvider : ObservableObject
    {
        public static LabelProvider Instance { get; } = new LabelProvider();

        LabelProvider() { }

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

        public void UpdateLabels(string singular, string plural)
        {
            ItemLabelSingular = singular;
            ItemLabelPlural = plural;
        }
    }
}
