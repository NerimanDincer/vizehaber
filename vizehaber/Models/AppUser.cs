using Microsoft.AspNetCore.Identity;

namespace vizehaber.Models
{
    public class AppUser : IdentityUser
    {
        public string FullName { get; set; }

        // Hocanın istediği:
        public string PhotoUrl { get; set; }

        // Bizim projenin çalışması için gerekenler:
        public bool IsActive { get; set; } = true; // <-- Bunu ekledik, hata gidecek
        public string? Biography { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public virtual ICollection<News> News { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
    }
}