using Microsoft.EntityFrameworkCore;

namespace vizehaber.Models
{
    public static class SeedData
    {
        public static void Seed(this ModelBuilder modelBuilder)
        {
            // Kategoriler
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Siyaset", IsActive = true, Created = DateTime.Now, Updated = DateTime.Now },
                new Category { Id = 2, Name = "Ekonomi", IsActive = true, Created = DateTime.Now, Updated = DateTime.Now },
                new Category { Id = 3, Name = "Spor", IsActive = true, Created = DateTime.Now, Updated = DateTime.Now }
            );

            // Yazarlar
            modelBuilder.Entity<Author>().HasData(
                new Author
                {
                    Id = 1,
                    Name = "Ahmet Yazar",
                    Email = "ahmet@yazar.com",
                    Password = "1234",
                    PhotoPath = "/userPhotos/ahmet.jpg",
                    Bio = "Siyaset alanında uzman gazeteci.",
                    IsActive = true,
                    Created = DateTime.Now,
                    Updated = DateTime.Now
                },
                new Author
                {
                    Id = 2,
                    Name = "Ayşe Yazar",
                    Email = "ayse@yazar.com",
                    Password = "1234",
                    PhotoPath = "/userPhotos/ayse.jpg",
                    Bio = "Ekonomi yazılarıyla tanınan deneyimli yazar.",
                    IsActive = true,
                    Created = DateTime.Now,
                    Updated = DateTime.Now
                }
            );

            // Kullanıcılar
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    FullName = "Ali Kullanıcı",
                    UserName = "alikullanici",
                    Email = "ali@user.com",
                    Password = "1234",
                    Role = "User",
                    PhotoPath = "/userPhotos/ali.png",
                    IsActive = true,
                    Created = DateTime.Now,
                    Updated = DateTime.Now
                },
                new User
                {
                    Id = 2,
                    FullName = "Admin Kullanıcı",
                    UserName = "admin",
                    Email = "admin@admin.com",
                    Password = "admin",
                    Role = "Admin",
                    PhotoPath = "/userPhotos/admin.png",
                    IsActive = true,
                    Created = DateTime.Now,
                    Updated = DateTime.Now
                }
            );

            // Haberler
            modelBuilder.Entity<News>().HasData(
                new News
                {
                    Id = 1,
                    Title = "Siyaset Haberi 1",
                    Content = "Siyaset gündemine dair haber 1.",
                    PublishedDate = DateTime.Now,
                    CategoryId = 1,
                    AuthorId = 1,
                    IsActive = true,
                    Created = DateTime.Now,
                    Updated = DateTime.Now
                },
                new News
                {
                    Id = 2,
                    Title = "Ekonomi Haberi 1",
                    Content = "Ekonomiyle ilgili haber 1.",
                    PublishedDate = DateTime.Now,
                    CategoryId = 2,
                    AuthorId = 2,
                    IsActive = true,
                    Created = DateTime.Now,
                    Updated = DateTime.Now
                }
            );
        }
    }
}
