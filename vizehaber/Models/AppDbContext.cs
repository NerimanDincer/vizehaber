using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


namespace vizehaber.Models
{
    public class AppDbContext : IdentityDbContext<AppUser, AppRole, string>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<News> News { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Contact> Contacts { get; set; }

        // 🔥 SQL CYCLE (KISIRDÖNGÜ) HATASINI ÇÖZEN KISIM 🔥
        // AppDbContext.cs içindeki ilgili kısım:

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Haber - Kategori İlişkisi
            builder.Entity<News>()
                .HasOne(n => n.Category)
                .WithMany(c => c.NewsList) // 👈 DİKKAT: Burayı 'News' yerine 'NewsList' yaptık!
                .HasForeignKey(n => n.CategoryId);

            // Haber - Kullanıcı İlişkisi
            builder.Entity<News>()
                .HasOne(n => n.AppUser)
                .WithMany(u => u.News)
                .HasForeignKey(n => n.AppUserId);

            // Yorum - Kullanıcı İlişkisi (Kısırdöngü Çözümü)
            builder.Entity<Comment>()
                .HasOne(c => c.AppUser)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.AppUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}