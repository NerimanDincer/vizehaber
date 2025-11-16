using System.ComponentModel.DataAnnotations;

namespace vizehaber.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Kullanıcı adı veya email zorunludur")]
        public string UserNameOrEmail { get; set; }  // Kullanıcı adı veya email ile giriş

        [Required(ErrorMessage = "Şifre zorunludur")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}