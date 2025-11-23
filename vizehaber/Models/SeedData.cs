using Microsoft.EntityFrameworkCore;
using vizehaber.Models;

namespace vizehaber.Models
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new AppDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<AppDbContext>>()))
            {
                context.Database.EnsureCreated();

                if (context.Users.Any()) return;

                context.Users.AddRange(
                    new User
                    {
                        FullName = "Admin",
                        UserName = "admin",
                        Email = "admin@haber.com",
                        Password = "123",
                        Role = "Admin",
                        PhotoPath = "/sbadmin/img/undraw_profile.svg", // User için doğru isim bu
                        CreatedDate = DateTime.Now
                    },
                    new User
                    {
                        FullName = "Ahmet Yazar",
                        UserName = "ahmet",
                        Email = "ahmet@haber.com",
                        Password = "123",
                        Role = "Writer",
                        PhotoPath = "/sbadmin/img/undraw_profile_1.svg",
                        Biography = "Teknoloji Yazarı",
                        CreatedDate = DateTime.Now
                    }
                );
                context.SaveChanges();
            }
        }
    }
}