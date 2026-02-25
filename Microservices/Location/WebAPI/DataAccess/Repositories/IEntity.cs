namespace WebAPI.DataAccess.Repositories;

public interface IEntity<T>
{
    T Id { get; set; }
}