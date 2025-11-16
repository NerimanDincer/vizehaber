//News.cs  haber modeli, kategori ve yazar ile ilişkili

namespace vizehaber.Models
{
    public class News : BaseEntity
    {
        public string Title { get; set; }              // Haber başlığı
        public string Content { get; set; }            // Haber içeriği
        public DateTime PublishedDate { get; set; }    // Yayınlanma tarihi

        // ✅ Kategori ilişkisi
        public int CategoryId { get; set; }
        public Category Category { get; set; }

        // ✅ Yazar ilişkisi
        public int AuthorId { get; set; }
        public Author Author { get; set; }

        // ✅ Opsiyonel alanlar - geliştirilebilir
        public string? ImagePath { get; set; }         // Haber fotoğrafı (wwwroot/newsPhotos)
        public bool IsApproved { get; set; } = false;  // Admin onaylı mı?
    }
}
