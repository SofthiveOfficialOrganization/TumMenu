using WebAPI.DataAccess.Repositories;
using WebAPI.Models.Concrete;

namespace WebAPI.DataAccess.Abstract;

public interface IDistrictRepository : IAsyncRepository<District, Guid>, IRepository<District, Guid> { }