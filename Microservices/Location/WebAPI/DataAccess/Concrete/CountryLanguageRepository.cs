using WebAPI.DataAccess.Abstract;
using WebAPI.DataAccess.Contexts;
using WebAPI.DataAccess.Repositories;
using WebAPI.Models.Concrete;

namespace WebAPI.DataAccess.Concrete;

public class CountryLanguageRepository(BaseDbContext context)
    : EfRepositoryBase<CountryLanguage, Guid, BaseDbContext>(context), ICountryLanguageRepository;