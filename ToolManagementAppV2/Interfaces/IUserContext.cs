using ToolManagementAppV2.Models.Domain;

namespace ToolManagementAppV2.Interfaces
{
    public interface IUserContext
    {
        User? CurrentUser { get; set; }
    }
}
