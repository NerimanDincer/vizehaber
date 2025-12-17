using System.ComponentModel.DataAnnotations;

namespace vizehaber.ViewModels
{
    public class UserUpdateViewModel
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Biography { get; set; }
        public string PhotoUrl { get; set; }

        // Şifre değiştirme alanları
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
    }
}