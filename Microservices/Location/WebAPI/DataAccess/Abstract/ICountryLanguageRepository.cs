using WebAPI.DataAccess.Repositories;
using WebAPI.Models.Concrete;

namespace WebAPI.DataAccess.Abstract;

public interface ICountryLanguageRepository : IAsyncRepository<CountryLanguage, Guid>, IRepository<CountryLanguage, Guid> { }