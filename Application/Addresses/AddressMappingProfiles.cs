using Application.Addresses.DTOs;
using Domain.Entities;
using Mapster;

namespace Application.Addresses;

public class AddressMappingProfiles : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Address, AddressDTO>();
        config.NewConfig<AddressDTO, Address>();
    }
}
