using Application.Owners.DTOs;
using Domain.Entities;
using Mapster;

namespace Application.Owners;

public class OwnerMapping
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Owner, OwnerDTO>();
    }
}