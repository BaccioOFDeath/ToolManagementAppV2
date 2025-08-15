using ToolManagementAppV2.Models.Domain;

namespace ToolManagementAppV2.Models
{
    public record Result(bool Success, string? ErrorMessage = null);

    public record Result<T>(T? Value, bool Success, string? ErrorMessage = null)
        : Result(Success, ErrorMessage)
    {
        public List<ActivityLog>? Data { get; internal set; }
    }
}
