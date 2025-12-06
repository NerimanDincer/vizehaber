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
        public DbSet<Comment> Comments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Yorum ile User arasındaki ilişkiyi "NoAction" yapıyoruz (Döngü hatasını çözer)
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany(u => u.Comments) // User modelinde Comments listesi var demiştik
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.NoAction); // KİLİT NOKTA BURASI!

            base.OnModelCreating(modelBuilder);
        }

    }
}