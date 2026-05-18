namespace Infrastructure.DataMigration.Models;

public sealed class MigrationRequest
{
    public string Source { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string TableName { get; init; } = string.Empty;
}
