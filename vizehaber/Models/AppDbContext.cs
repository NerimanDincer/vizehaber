//AppDbContext.cs  veritabanı bağlantısı ve DbSet’ler hazır.

using Microsoft.EntityFrameworkCore;

namespace vizehaber.Models
{
    public class AppDbContext : DbContext
    {
        public DbSet<News> News { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Author> Authors { get; set; }


        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Seed();  //modelBuilder.Seed() çağrısı ile SeedData aktif.
        }
    }
}
