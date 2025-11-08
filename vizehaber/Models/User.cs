//User.cs  kullanıcı modeli (User/Author/Admin rolleri için Role alanı ekli)

namespace vizehaber.Models
{
    public class User : BaseEntity
    {
        public string FullName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Role { get; set; } // "User(kullanıcı)", "Author(yazar)", "Admin(yönetici)"
        public string PhotoUrl { get; set; }
    }
}
