using Application.Common.Base.DTOs;

namespace Application.Stores.DTOs
{
    public sealed record StoreLiteDTO(
        string Name,
        string Slug
    ) : BaseDTO, ISluggableDTO;
}
