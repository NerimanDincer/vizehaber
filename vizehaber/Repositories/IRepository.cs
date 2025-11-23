using System.Linq.Expressions;

namespace vizehaber.Repositories
{
    public interface IRepository<T> where T : class
    {
        Task<List<T>> GetAllAsync();
        Task<T> GetByIdAsync(int id);
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(int id);

        // Şartlı sorgular için (Örn: Sadece Spor kategorisindeki haberleri getir)
        Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate);
    }
}