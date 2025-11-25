namespace Domain.Entities
{
    public class ExtensionPackPlan
    {
        public Guid ExtensionPackId { get; set; }
        public ExtensionPack ExtensionPack { get; set; } = null!;
        public Guid PlanId { get; set; }
        public Plan Plan { get; set; } = null!;
    }
}
