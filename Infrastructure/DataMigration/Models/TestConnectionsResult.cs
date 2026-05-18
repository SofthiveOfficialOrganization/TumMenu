namespace Infrastructure.DataMigration.Models;

public sealed class TestConnectionsResult
{
    public bool SourceOk { get; set; }
    public bool TargetOk { get; set; }
    public string? SourceError { get; set; }
    public string? TargetError { get; set; }
}
