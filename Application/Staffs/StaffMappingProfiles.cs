using Application.Staffs.DTOs;
using Domain.Entities;
using Mapster;

namespace Application.Staffs
{
    public class StaffMappingProfiles
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<Staff, StaffDTO>();
        }
    }
}
