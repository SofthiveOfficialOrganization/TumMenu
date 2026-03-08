using System;

namespace Application.Ads.DTOs;

public record AdSlotDTO
{
    public Guid Id { get; init; }
    public string Key { get; init; } = null!;
    public string Description { get; init; } = "";
    public bool IsActive { get; init; }
}

public record AdCreativeDTO
{
    public Guid Id { get; init; }
    public string Type { get; init; } = "image";
    public string Content { get; init; } = null!;
    public string? ClickUrl { get; init; }
}

public record AdPlacementDTO
{
    public Guid Id { get; init; }
    public Guid AdSlotId { get; init; }
    public Guid AdCreativeId { get; init; }
    public AdSlotDTO AdSlot { get; init; } = null!;
    public AdCreativeDTO AdCreative { get; init; } = null!;
    public DateTime? StartAt { get; init; }
    public DateTime? EndAt { get; init; }
    public int? DailyCap { get; init; }
    public bool IsActive { get; init; }
}

public record AdRevenueImportDTO
{
    public Guid Id { get; init; }
    public string Network { get; init; } = "AdSense";
    public DateOnly Day { get; init; }
    public decimal Revenue { get; init; }
    public string Currency { get; init; } = "TRY";
    public string? SourceFile { get; init; }
}
