using System.Collections.Generic;

namespace vizehaber.ViewModels
{
    public class UserRoleViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string PhotoUrl { get; set; }

        // Kullanıcı düzenlerken unvanı buraya gelecek
        public string Specialization { get; set; }

        // Listeleme sayfasında (Index) tek bir rol göstermek için
        public string Role { get; set; }

        // Düzenleme sayfasında (EditUser) seçilen rolü tutmak için
        public string SelectedRole { get; set; }
    }
}