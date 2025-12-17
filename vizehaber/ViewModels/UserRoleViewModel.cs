using System.Collections.Generic;

namespace vizehaber.ViewModels
{
    public class UserRoleViewModel
    {
        // ID kısmını senin koduna uyumlu hale getirdim
        public string Id { get; set; }

        public string UserName { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string PhotoUrl { get; set; }

        // 🔥 EKSİK OLANLAR EKLENDİ 🔥
        public string Specialization { get; set; } // Uzmanlık alanı
        public string SelectedRole { get; set; }   // Seçilen Rol (User, Admin, Writer)

        public IList<string> Roles { get; set; } // Kullanıcının mevcut rolleri
    }
}