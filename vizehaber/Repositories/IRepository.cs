using System.Linq.Expressions;

namespace vizehaber.Repositories
{
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();

        // DİKKAT: int değil object yapıyoruz
        Task<T> GetByIdAsync(object id);

        Task AddAsync(T entity);
        Task UpdateAsync(T entity);

        // DİKKAT: int değil object yapıyoruz
        Task DeleteAsync(object id);

        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    }
}