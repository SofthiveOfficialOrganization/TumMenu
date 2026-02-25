using WebAPI.DataAccess.Abstract;
using WebAPI.DataAccess.Contexts;
using WebAPI.DataAccess.Repositories;
using WebAPI.Models.Concrete;

namespace WebAPI.DataAccess.Concrete;

public class CountryRepository(BaseDbContext context)
    : EfRepositoryBase<Country, Guid, BaseDbContext>(context), ICountryRepository;