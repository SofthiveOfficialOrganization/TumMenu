using System.Linq.Expressions;
using System.Reflection;
using Infrastructure.DataMigration.Models;
using Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DataMigration;

public sealed class DataMigrationService : IDataMigrationService
{
    private readonly MigrationTableCatalog _catalog;

    public DataMigrationService(MigrationTableCatalog catalog)
    {
        _catalog = catalog;
    }

    public IReadOnlyList<TableInfo> ListTables() => _catalog.ListTables();

    public async Task<TestConnectionsResult> TestConnectionsAsync(
        string source, string target, CancellationToken ct)
    {
        var result = new TestConnectionsResult();

        (result.SourceOk, result.SourceError) = await PingAsync(source, ct);
        (result.TargetOk, result.TargetError) = await PingAsync(target, ct);

        return result;
    }

    public async Task<MigrationResult> MigrateTableAsync(MigrationRequest request, CancellationToken ct)
    {
        if (!_catalog.TryGet(request.TableName, out var info, out var clrType))
            throw new ArgumentException($"Unknown table '{request.TableName}'.", nameof(request));

        var method = typeof(DataMigrationService)
            .GetMethod(nameof(MigrateInternalAsync), BindingFlags.NonPublic | BindingFlags.Instance)!
            .MakeGenericMethod(clrType);

        var task = (Task<MigrationResult>)method.Invoke(this, new object[] { request, info, ct })!;
        return await task.ConfigureAwait(false);
    }

    private async Task<MigrationResult> MigrateInternalAsync<T>(
        MigrationRequest request, TableInfo info, CancellationToken ct)
        where T : class
    {
        const int BatchSize = 1000;

        var result = new MigrationResult { TableName = info.Name };

        await using var source = MigrationContextFactory.Create(request.Source);
        await using var target = MigrationContextFactory.Create(request.Target);

        var pkProps = info.PrimaryKeyProperties
            .Select(name => typeof(T).GetProperty(name)
                ?? throw new InvalidOperationException($"Property '{name}' not found on {typeof(T).Name}"))
            .ToList();

        string MakeKey(T row) => string.Join("|",
            pkProps.Select(p => p.GetValue(row)?.ToString() ?? ""));

        var allTargetRows = await target.Set<T>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .ToListAsync(ct)
            .ConfigureAwait(false);

        var existingKeys = new HashSet<string>(allTargetRows.Select(MakeKey), StringComparer.Ordinal);
        allTargetRows = null!;

        var orderBy = BuildOrderByExpression<T>(pkProps[0].Name);

        int offset = 0;
        while (true)
        {
            ct.ThrowIfCancellationRequested();

            var batch = await source.Set<T>()
                .IgnoreQueryFilters()
                .AsNoTracking()
                .OrderBy(orderBy)
                .Skip(offset)
                .Take(BatchSize)
                .ToListAsync(ct)
                .ConfigureAwait(false);

            if (batch.Count == 0) break;

            result.TotalReadFromSource += batch.Count;

            var toInsert = batch.Where(e => !existingKeys.Contains(MakeKey(e))).ToList();
            result.AlreadyExistsInTarget += batch.Count - toInsert.Count;

            if (toInsert.Count > 0)
            {
                try
                {
                    await InsertBatchAsync(target, info, toInsert, ct).ConfigureAwait(false);
                    result.Inserted += toInsert.Count;
                    foreach (var row in toInsert) existingKeys.Add(MakeKey(row));
                }
                catch
                {
                    foreach (var row in toInsert)
                    {
                        try
                        {
                            await InsertBatchAsync(target, info, new List<T> { row }, ct).ConfigureAwait(false);
                            result.Inserted++;
                            existingKeys.Add(MakeKey(row));
                        }
                        catch (Exception rowEx)
                        {
                            result.SkippedDueToError++;
                            result.Errors.Add(new RowError
                            {
                                Id = MakeKey(row),
                                Message = rowEx.GetBaseException().Message
                            });
                        }
                    }
                }
            }

            offset += batch.Count;
        }

        return result;
    }

    private static Expression<Func<T, object>> BuildOrderByExpression<T>(string propertyName)
    {
        var param = Expression.Parameter(typeof(T), "e");
        var prop = Expression.Property(param, propertyName);
        var converted = Expression.Convert(prop, typeof(object));
        return Expression.Lambda<Func<T, object>>(converted, param);
    }

    private static async Task InsertBatchAsync<T>(
        ApplicationDbContext target, TableInfo info, List<T> rows, CancellationToken ct)
        where T : class
    {
        var previousAutoDetect = target.ChangeTracker.AutoDetectChangesEnabled;
        target.ChangeTracker.AutoDetectChangesEnabled = false;

        await using var tx = await target.Database.BeginTransactionAsync(ct).ConfigureAwait(false);

        try
        {
            if (info.HasIdentityPk)
            {
                // SqlTableName comes from trusted EF metadata, not user input.
                // SQL Server doesn't accept table names as parameters in SET IDENTITY_INSERT, so raw is required.
#pragma warning disable EF1002
                await target.Database
                    .ExecuteSqlRawAsync($"SET IDENTITY_INSERT [{info.SqlTableName}] ON", ct)
                    .ConfigureAwait(false);
#pragma warning restore EF1002
            }

            await target.Set<T>().AddRangeAsync(rows, ct).ConfigureAwait(false);
            await target.SaveChangesAsync(ct).ConfigureAwait(false);

            if (info.HasIdentityPk)
            {
#pragma warning disable EF1002
                await target.Database
                    .ExecuteSqlRawAsync($"SET IDENTITY_INSERT [{info.SqlTableName}] OFF", ct)
                    .ConfigureAwait(false);
#pragma warning restore EF1002
            }

            await tx.CommitAsync(ct).ConfigureAwait(false);
        }
        finally
        {
            target.ChangeTracker.Clear();
            target.ChangeTracker.AutoDetectChangesEnabled = previousAutoDetect;
        }
    }

    private static async Task<(bool ok, string? error)> PingAsync(string connectionString, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            return (false, "Connection string is empty.");

        try
        {
            await using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync(ct);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT 1";
            cmd.CommandTimeout = 5;
            _ = await cmd.ExecuteScalarAsync(ct);
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }
}
