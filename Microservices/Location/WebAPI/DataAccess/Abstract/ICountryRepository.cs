using WebAPI.DataAccess.Repositories;
using WebAPI.Models.Concrete;

namespace WebAPI.DataAccess.Abstract;

public interface ICountryRepository : IAsyncRepository<Country, Guid>, IRepository<Country, Guid> { }