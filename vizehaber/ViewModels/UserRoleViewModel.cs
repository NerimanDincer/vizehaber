using vizehaber.Models;

namespace vizehaber.ViewModels
{
    public class UserRoleViewModel
    {
        public string Id { get; set; } // Kullanıcının ID'si
        public string FullName { get; set; } // Adı Soyadı
        public string Email { get; set; } // E-postası
        public string SelectedRole { get; set; } // Seçilen Rol (Admin/Writer/User)

        // Dropdown (Açılır Kutu) için Rol Listesi
        public List<string> Roles { get; set; } = new List<string> { "Admin", "Writer", "User" };
    }
}
