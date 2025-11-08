using Microsoft.EntityFrameworkCore;
using vizehaber.Models;

namespace vizehaber.Repositories
{
    public class NewsRepository
    {
        private readonly AppDbContext _context;

        public NewsRepository(AppDbContext context)
        {
            _context = context;
        }

        // Tüm haberleri getir
        public async Task<List<News>> GetAllAsync()
        {
            return await _context.News
                                 .Include(n => n.Category) // Haber kategorisiyle birlikte
                                 .Include(n => n.Author)   // Haber yazarıyla birlikte
                                 .ToListAsync();
        }

        // Id ile haber getir
        public async Task<News> GetByIdAsync(int id)
        {
            return await _context.News
                                 .Include(n => n.Category)               // EF Core’da ilişkili veriyi de çekmek için kullanılır. Böylece haber listesinde kategori ve yazar bilgilerini kullanabiliriz.
                                 .Include(n => n.Author)                 
                                 .FirstOrDefaultAsync(n => n.Id == id);
        }

        // Yeni haber ekle
        public async Task AddAsync(News news)
        {
            await _context.News.AddAsync(news);
            await _context.SaveChangesAsync();
        }

        // Haber güncelle
        public async Task UpdateAsync(News news)
        {
            _context.News.Update(news);
            await _context.SaveChangesAsync();
        }

        // Haber sil
        public async Task DeleteAsync(int id)
        {
            var news = await _context.News.FindAsync(id);
            if (news != null)
            {
                _context.News.Remove(news);
                await _context.SaveChangesAsync();
            }
        }
    }
}
