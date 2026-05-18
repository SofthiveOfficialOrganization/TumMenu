namespace Infrastructure.DataMigration.Models;

public sealed class TableInfo
{
    public string Name { get; init; } = string.Empty;
    public string SqlTableName { get; init; } = string.Empty;
    public bool HasIdentityPk { get; init; }
    public IReadOnlyList<string> PrimaryKeyProperties { get; init; } = Array.Empty<string>();
}
