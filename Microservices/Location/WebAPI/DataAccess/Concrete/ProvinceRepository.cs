using WebAPI.DataAccess.Abstract;
using WebAPI.DataAccess.Contexts;
using WebAPI.DataAccess.Repositories;
using WebAPI.Models.Concrete;

namespace WebAPI.DataAccess.Concrete;

public class ProvinceRepository(BaseDbContext context)
    : EfRepositoryBase<Province, Guid, BaseDbContext>(context), IProvinceRepository;