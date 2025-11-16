using System.ComponentModel.DataAnnotations;

namespace vizehaber.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Adınızı giriniz")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Kullanıcı adı zorunludur")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Email giriniz")]
        [EmailAddress(ErrorMessage = "Geçerli bir email giriniz")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Şifre zorunludur")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Şifreyi tekrar giriniz")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Şifreler eşleşmiyor")]
        public string ConfirmPassword { get; set; }

        public string Role { get; set; } = "User"; // default rol
    }
}

