//News.cs  haber modeli, kategori ve yazar ile ilişkili

namespace vizehaber.Models
{
    public class News : BaseEntity
    {
        public string Title { get; set; }       // Haber başlığı
        public string Content { get; set; }     // Haber içeriği
        public DateTime PublishedDate { get; set; }  // Yayınlanma tarihi

        // Foreign key - kategori
        public int CategoryId { get; set; }
        public Category Category { get; set; }

        // Foreign key - yazar
        public int AuthorId { get; set; }
        public Author Author { get; set; }
    }
}