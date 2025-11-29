using Application.Common.Base.DTOs;
using Mapster;

namespace Application.Common.Base.Page;

public class PaginationMappingProfiles
{
    public void Register(Mapster.TypeAdapterConfig config)
    {
        config.NewConfig(typeof(IPaginate<>), typeof(PaginatedListDTO<>))
            .Map(nameof(PaginatedListDTOBase.From), nameof(IPaginate<object>.From))
            .Map(nameof(PaginatedListDTOBase.Index), nameof(IPaginate<object>.Index))
            .Map(nameof(PaginatedListDTOBase.Size), nameof(IPaginate<object>.Size))
            .Map(nameof(PaginatedListDTOBase.Count), nameof(IPaginate<object>.Count))
            .Map(nameof(PaginatedListDTOBase.Pages), nameof(IPaginate<object>.Pages))
            .Map(nameof(PaginatedListDTOBase.HasPrevious), nameof(IPaginate<object>.HasPrevious))
            .Map(nameof(PaginatedListDTOBase.HasNext), nameof(IPaginate<object>.HasNext))
            .Map(nameof(PaginatedListDTO<object>.Items), nameof(IPaginate<object>.Items));
    }
}
