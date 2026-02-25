using WebAPI.DataAccess.Abstract;
using WebAPI.DataAccess.Contexts;
using WebAPI.DataAccess.Repositories;
using WebAPI.Models.Concrete;

namespace WebAPI.DataAccess.Concrete;

public class DistrictRepository(BaseDbContext context)
    : EfRepositoryBase<District, Guid, BaseDbContext>(context), IDistrictRepository;