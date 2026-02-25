using WebAPI.DataAccess.Repositories;
using WebAPI.Models.Concrete;

namespace WebAPI.DataAccess.Abstract;

public interface ICountryCurrencyRepository : IAsyncRepository<CountryCurrency, Guid>, IRepository<CountryCurrency, Guid> { }