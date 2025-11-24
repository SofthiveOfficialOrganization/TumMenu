using Application.Common.Base.DTOs;
using Mapster;

namespace Application.Common.Base.Page;

public class PaginationMappingProfiles
{
    public void Register(Mapster.TypeAdapterConfig config)
    {
        config.NewConfig(typeof(IPaginate<>), typeof(PaginatedListDTO<>))
            .Map(nameof(PaginationListDTOBase.From), nameof(IPaginate<object>.From))
            .Map(nameof(PaginationListDTOBase.Index), nameof(IPaginate<object>.Index))
            .Map(nameof(PaginationListDTOBase.Size), nameof(IPaginate<object>.Size))
            .Map(nameof(PaginationListDTOBase.Count), nameof(IPaginate<object>.Count))
            .Map(nameof(PaginationListDTOBase.Pages), nameof(IPaginate<object>.Pages))
            .Map(nameof(PaginationListDTOBase.HasPrevious), nameof(IPaginate<object>.HasPrevious))
            .Map(nameof(PaginationListDTOBase.HasNext), nameof(IPaginate<object>.HasNext))
            .Map(nameof(PaginatedListDTO<object>.Items), nameof(IPaginate<object>.Items));
    }
}
