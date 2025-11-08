//Başlangıç verilerini ekler (kategori, haber, kullanıcı, yazar, admin...)
//SeedData.cs  örnek veriler ekledim (kategoriler, haberler, kullanıcılar ve yazarlar).

using Microsoft.EntityFrameworkCore;

namespace vizehaber.Models
{
    public static class SeedData
    {
        public static void Seed(this ModelBuilder modelBuilder)
        {
            // Kategoriler
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Siyaset", IsActive = true },
                new Category { Id = 2, Name = "Ekonomi", IsActive = true },
                new Category { Id = 3, Name = "Spor", IsActive = true }
            );

            // Yazarlar
            modelBuilder.Entity<Author>().HasData(
                new Author { Id = 1, Name = "Ahmet Yazar", Email = "ahmet@yazar.com", Password = "1234", IsActive = true, Created = DateTime.Now, Updated = DateTime.Now },
                new Author { Id = 2, Name = "Ayşe Yazar", Email = "ayse@yazar.com", Password = "1234", IsActive = true, Created = DateTime.Now, Updated = DateTime.Now }
            );

            // Kullanıcılar
            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, UserName = "Ali Kullanıcı", Email = "ali@user.com", Password = "1234", Role = "User", IsActive = true, Created = DateTime.Now, Updated = DateTime.Now },
                new User { Id = 2, UserName = "Admin", Email = "admin@admin.com", Password = "admin", Role = "Admin", IsActive = true, Created = DateTime.Now, Updated = DateTime.Now }
            );

            // Haberler
            modelBuilder.Entity<News>().HasData(
                new News { Id = 1, Title = "Siyaset Haberi 1", Content = "İçerik 1", PublishedDate = DateTime.Now, CategoryId = 1, AuthorId = 1, IsActive = true, Created = DateTime.Now, Updated = DateTime.Now },
                new News { Id = 2, Title = "Ekonomi Haberi 1", Content = "İçerik 2", PublishedDate = DateTime.Now, CategoryId = 2, AuthorId = 2, IsActive = true, Created = DateTime.Now, Updated = DateTime.Now }
            );
        }
    }
}

