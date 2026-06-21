using System.Threading.Tasks;

namespace InventoryManagementApp.Interfaces
{
    public interface ISetupWizard
    {
        Task<SetupWizardResult?> RunAsync();
    }

    public record SetupWizardResult(
        string Password,
        string ApplicationName,
        string ItemLabelSingular,
        string ItemLabelPlural,
        string CompanyLogoPath,
        string ThemeProfilePath = "");
}
