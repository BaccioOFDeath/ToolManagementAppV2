using System.Threading.Tasks;

namespace ToolManagementAppV2.Interfaces
{
    public interface ISetupWizard
    {
        Task<SetupWizardResult?> RunAsync();
    }

    public record SetupWizardResult(string Password, bool IsRandom);
}
