using Infrastructure.DataMigration.Models;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.SqlServer.Metadata;

namespace Infrastructure.DataMigration;

public sealed class MigrationTableCatalog
{
    private readonly Dictionary<string, Entry> _entries;

    public MigrationTableCatalog()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer("Server=.;Database=__catalog_only__;TrustServerCertificate=True")
            .Options;

        using var probe = new ApplicationDbContext(options);

        _entries = new(StringComparer.OrdinalIgnoreCase);

        foreach (var et in probe.Model.GetEntityTypes())
        {
            var dbSetName = ResolveDbSetName(et.ClrType);
            if (dbSetName is null) continue;

            var pk = et.FindPrimaryKey();
            if (pk is null || pk.Properties.Count == 0) continue;

            var pkPropertyNames = pk.Properties.Select(p => p.Name).ToList();

            // Only SQL Server IDENTITY columns (int/bigint with auto-increment) need IDENTITY_INSERT.
            // Guid PKs are client-generated and not IDENTITY columns even though EF marks them ValueGenerated.OnAdd.
            var hasIdentity = pk.Properties.Count == 1
                && pk.Properties[0].GetValueGenerationStrategy() == SqlServerValueGenerationStrategy.IdentityColumn;

            var sqlName = et.GetTableName() ?? et.ClrType.Name;

            _entries[dbSetName] = new Entry(
                new TableInfo
                {
                    Name = dbSetName,
                    SqlTableName = sqlName,
                    HasIdentityPk = hasIdentity,
                    PrimaryKeyProperties = pkPropertyNames
                },
                et.ClrType);
        }
    }

    public IReadOnlyList<TableInfo> ListTables() =>
        _entries.Values
            .Select(e => e.Info)
            .OrderBy(t => t.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

    public bool TryGet(string tableName, out TableInfo info, out Type clrType)
    {
        if (_entries.TryGetValue(tableName, out var entry))
        {
            info = entry.Info;
            clrType = entry.ClrType;
            return true;
        }
        info = null!;
        clrType = null!;
        return false;
    }

    private static string? ResolveDbSetName(Type entityClrType)
    {
        var props = typeof(ApplicationDbContext)
            .GetProperties()
            .Where(p => p.PropertyType.IsGenericType
                && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>)
                && p.PropertyType.GenericTypeArguments[0] == entityClrType)
            .ToList();

        if (props.Count == 0) return null;
        if (props.Count == 1) return props[0].Name;

        // Multiple DbSet properties expose the same entity type
        // (e.g. ApplicationDbContext overrides ApplicationUsers/Users, ApplicationRoles/Roles, ApplicationUserRoles/UserRoles).
        // Prefer the one declared on ApplicationDbContext itself; fall back to the alphabetically first base property.
        var derived = props.FirstOrDefault(p => p.DeclaringType == typeof(ApplicationDbContext));
        return derived?.Name ?? props.OrderBy(p => p.Name, StringComparer.Ordinal).First().Name;
    }

    private sealed record Entry(TableInfo Info, Type ClrType);
}
