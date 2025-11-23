using Microsoft.EntityFrameworkCore;

namespace vizehaber.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<News> News { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Contact> Contacts { get; set; }

        // O hatayı veren OnModelCreating kısmını SİLDİK.
        // Çünkü veriyi zaten Program.cs üzerinden yüklüyoruz.
    }
}