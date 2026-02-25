using WebAPI.DataAccess.Repositories;
using WebAPI.Models.Concrete;

namespace WebAPI.DataAccess.Abstract;

public interface ICountryTranslationRepository : IAsyncRepository<CountryTranslation, Guid>, IRepository<CountryTranslation, Guid> { }