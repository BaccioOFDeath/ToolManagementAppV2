using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ToolManagementAppV2.Interfaces;

namespace ToolManagementAppV2.Utilities
{
    public static class AsyncHelpers
    {
        public static async Task ExecuteSafelyAsync(
            Func<CancellationToken, Task> operation,
            ILogger logger,
            IDialogService dialog,
            string? userMessage = null,
            CancellationToken ct = default)
        {
            try
            {
                ct.ThrowIfCancellationRequested();
                await operation(ct).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Operation failed");
                if (!string.IsNullOrWhiteSpace(userMessage))
                {
                    await dialog.ShowInfoAsync(userMessage, "Error").ConfigureAwait(false);
                }
            }
        }
    }
}
