using Application.Ads.DTOs;
using Domain.Entities;
using Mapster;

namespace Application.Ads;

public class AdMappingProfiles : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<AdSlot, AdSlotDTO>();
        config.NewConfig<AdCreative, AdCreativeDTO>();
        config.NewConfig<AdPlacement, AdPlacementDTO>();
        config.NewConfig<AdRevenueImport, AdRevenueImportDTO>();
    }
}
