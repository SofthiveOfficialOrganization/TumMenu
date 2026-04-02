namespace Application.Abstractions;

/// <summary>
/// Bu interface'i implement eden command'lar AuditLogBehavior tarafından loglanır.
/// ActionName: Dashboard'da müşteriye gösterilecek Türkçe eylem adı.
/// </summary>
public interface IAuditableCommand
{
    string ActionName { get; }
}

/// <summary>
/// Entity ID de loglanması gereken command'lar için.
/// </summary>
public interface IEntityAuditableCommand : IAuditableCommand
{
    Guid EntityId { get; }
}
