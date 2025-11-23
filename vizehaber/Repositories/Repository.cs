using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using vizehaber.Models;

namespace vizehaber.Repositories
{
    public class Repository<T> : IRepository<T> where T : BaseEntity // BaseEntity şartı
    {
        private readonly AppDbContext _context;
        private readonly DbSet<T> _dbSet;

        public Repository(AppDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public async Task<List<T>> GetAllAsync() => await _dbSet.ToListAsync();
        public async Task<T> GetByIdAsync(int id) => await _dbSet.FindAsync(id);
        public async Task AddAsync(T entity) { await _dbSet.AddAsync(entity); await _context.SaveChangesAsync(); }
        public async Task UpdateAsync(T entity) { _dbSet.Update(entity); await _context.SaveChangesAsync(); }
        public async Task DeleteAsync(int id) { var entity = await _dbSet.FindAsync(id); if (entity != null) { _dbSet.Remove(entity); await _context.SaveChangesAsync(); } }
        public async Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate) => await _dbSet.Where(predicate).ToListAsync();
    }
}