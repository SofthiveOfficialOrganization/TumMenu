using Application.Common.Base.DTOs;
using Domain.Base;
using Mapster;

namespace Application.Common.Base;

public class BaseEntityMapper
{
    public void Register(TypeAdapterConfig config)
    {
        config.ForType<BaseEntity, BaseDTO>();
    }
}
