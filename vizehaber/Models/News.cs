using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace vizehaber.Models
{
    public class News : BaseEntity
    {
        [Display(Name = "Haber Başlığı")]
        public string Title { get; set; }

        [Display(Name = "Haber İçeriği")]
        public string Content { get; set; }

        public DateTime PublishedDate { get; set; } = DateTime.Now;

        [Display(Name = "Haber Fotoğrafı")]
        public string? ImagePath { get; set; } // Senin istediğin isim
        public int ViewCount { get; set; } = 0; // Varsayılan 0 olsun, okunma sayısı burası

        // --- İLİŞKİLER ---

        // Kategori İlişkisi
        public int CategoryId { get; set; }
        public Category Category { get; set; }

        // ✅ Yazar İlişkisi (Artık User tablosuna bağlı)
        public string AppUserId { get; set; }
        public AppUser AppUser { get; set; }

        public bool IsApproved { get; set; } = false;
        public List<Comment>? Comments { get; set; }
    }
}