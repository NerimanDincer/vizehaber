using System.ComponentModel.DataAnnotations;

namespace vizehaber.Models
{
    public class Contact : BaseEntity
    {
        [Required(ErrorMessage = "Ad Soyad zorunludur")]
        [Display(Name = "Ad Soyad")]
        public string Name { get; set; }

        [Required(ErrorMessage = "E-posta zorunludur")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Konu zorunludur")]
        public string Subject { get; set; }

        [Required(ErrorMessage = "Mesaj zorunludur")]
        public string Message { get; set; }

        public string? PhotoUrl { get; set; }
    }
}