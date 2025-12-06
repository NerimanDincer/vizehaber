using Microsoft.EntityFrameworkCore;
using vizehaber.Models;
using vizehaber.Services;

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
                        FullName = "Neriman Dincer",
                        UserName = "neriman",
                        Email = "neriman@haber.com",
                        Password = GeneralService.HashPassword("123"), // Şifren bu olacak
                        Role = "Admin", // Kilit nokta burası: Rolü Admin
                        PhotoPath = "/sbadmin/img/undraw_profile_3.svg", // Farklı bir avatar seçtim
                        CreatedDate = DateTime.Now,
                        IsActive = true
                    },
                        new User
                    {
                        FullName = "Ahmet Yazar",
                        UserName = "ahmet",
                        Email = "ahmet@haber.com",
                        Password = GeneralService.HashPassword("123"),
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