namespace Infrastructure.DataMigration.Models;

public sealed class MigrationResult
{
    public string TableName { get; init; } = string.Empty;
    public int TotalReadFromSource { get; set; }
    public int AlreadyExistsInTarget { get; set; }
    public int Inserted { get; set; }
    public int SkippedDueToError { get; set; }
    public List<RowError> Errors { get; } = new();
}
