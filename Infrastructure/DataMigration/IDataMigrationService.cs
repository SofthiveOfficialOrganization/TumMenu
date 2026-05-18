using Infrastructure.DataMigration.Models;

namespace Infrastructure.DataMigration;

public interface IDataMigrationService
{
    IReadOnlyList<TableInfo> ListTables();

    Task<TestConnectionsResult> TestConnectionsAsync(
        string source, string target, CancellationToken ct);

    Task<MigrationResult> MigrateTableAsync(
        MigrationRequest request, CancellationToken ct);
}
