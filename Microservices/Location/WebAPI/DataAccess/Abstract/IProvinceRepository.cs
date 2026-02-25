using WebAPI.DataAccess.Repositories;
using WebAPI.Models.Concrete;

namespace WebAPI.DataAccess.Abstract;

public interface IProvinceRepository : IAsyncRepository<Province, Guid>, IRepository<Province, Guid> { }