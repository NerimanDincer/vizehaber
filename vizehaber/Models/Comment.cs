using System.ComponentModel.DataAnnotations;

namespace vizehaber.Models
{
    public class Comment : BaseEntity
    {
        [Required]
        public string Text { get; set; } // Yorum içeriği

        // İlişkiler
        public int NewsId { get; set; }
        public News News { get; set; }

        public string AppUserId { get; set; }
        public AppUser AppUser { get; set; }
    }
}