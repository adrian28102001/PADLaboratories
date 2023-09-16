namespace ApplicationManagementService.Repositories;

public interface IRepository<T> where T : class
{
    IQueryable<T> GetAllQuery();
    Task<List<T>> GetAllAsync();
    ValueTask<T?> GetByIdAsync(int id);
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
}