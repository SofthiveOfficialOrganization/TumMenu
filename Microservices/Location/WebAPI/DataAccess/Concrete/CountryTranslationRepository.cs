using WebAPI.DataAccess.Abstract;
using WebAPI.DataAccess.Contexts;
using WebAPI.DataAccess.Repositories;
using WebAPI.Models.Concrete;

namespace WebAPI.DataAccess.Concrete;

public class CountryTranslationRepository(BaseDbContext context)
    : EfRepositoryBase<CountryTranslation, Guid, BaseDbContext>(context), ICountryTranslationRepository;