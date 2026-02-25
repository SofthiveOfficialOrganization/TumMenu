namespace WebAPI.DataAccess.Repositories;

public interface IQuery<T>
{
    IQueryable<T> Query();
}