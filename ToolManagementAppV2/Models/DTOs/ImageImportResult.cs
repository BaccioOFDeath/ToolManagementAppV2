namespace ToolManagementAppV2.Models.ImportExport
{
    public class ImageImportResult
    {
        public int ImportedCount { get; set; }
        public List<string> UnmatchedFiles { get; } = new();
        public List<string> ConflictingFiles { get; } = new();
    }
}
