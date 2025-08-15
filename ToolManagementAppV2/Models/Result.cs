namespace ToolManagementAppV2.Models
{
    public record Result(bool Success, string? ErrorMessage = null);

    public record Result<T>(T? Value, bool Success, string? ErrorMessage = null)
        : Result(Success, ErrorMessage);
}
