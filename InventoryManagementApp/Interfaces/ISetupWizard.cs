using System.Threading.Tasks;

namespace InventoryManagementApp.Interfaces
{
    public interface ISetupWizard
    {
        Task<SetupWizardResult?> RunAsync();
    }

    public record SetupWizardResult(string Password, bool IsRandom);
}
