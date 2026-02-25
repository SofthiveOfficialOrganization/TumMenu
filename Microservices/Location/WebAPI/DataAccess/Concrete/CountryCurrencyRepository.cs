using WebAPI.DataAccess.Abstract;
using WebAPI.DataAccess.Contexts;
using WebAPI.DataAccess.Repositories;
using WebAPI.Models.Concrete;

namespace WebAPI.DataAccess.Concrete;

public class CountryCurrencyRepository(BaseDbContext context)
    : EfRepositoryBase<CountryCurrency, Guid, BaseDbContext>(context), ICountryCurrencyRepository;