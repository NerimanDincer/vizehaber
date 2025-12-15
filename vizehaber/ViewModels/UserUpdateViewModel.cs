using System.ComponentModel.DataAnnotations;

namespace vizehaber.ViewModels
{
    public class UserUpdateViewModel
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Biography { get; set; }
        public string? PhotoUrl { get; set; }

        // Şifre değiştirmek isterse diye (Zorunlu değil)
        [DataType(DataType.Password)]
        public string? CurrentPassword { get; set; }

        [DataType(DataType.Password)]
        public string? NewPassword { get; set; }
    }
}