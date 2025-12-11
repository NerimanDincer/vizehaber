using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using vizehaber.Models; // 🔥 İŞTE EKSİK OLAN VE HATAYI ÇÖZEN SATIR BU!

namespace vizehaber.Repositories
{
    // IRepository<T> kısmı artık kızarmayacak
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly AppDbContext _context;
        private readonly DbSet<T> _dbSet;

        public Repository(AppDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        // Interface'de object dedik, burada da object olmalı
        public async Task DeleteAsync(object id)
        {
            var entity = await GetByIdAsync(id); // GetByIdAsync zaten object alıyor
            if (entity != null)
            {
                _dbSet.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        // int yerine object
        public async Task<T> GetByIdAsync(object id)
        {
            // FindAsync metodu object parametre kabul eder (Hem int hem string çalışır)
            return await _dbSet.FindAsync(id);
        }

        public async Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
        }
    }
}