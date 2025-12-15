using System.ComponentModel.DataAnnotations;

namespace vizehaber.Models
{
    public class Comment : BaseEntity
    {
        [Required]
        public string Text { get; set; } // Yorum içeriği

        // İlişkiler
        public int NewsId { get; set; }
        public virtual News News { get; set; }

        public string AppUserId { get; set; }
        public virtual AppUser AppUser { get; set; }
    }
}