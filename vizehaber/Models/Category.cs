//Category.cs haber kategorilerini tutuyor, News ile ilişkilendi

using System.Reflection.Emit;

namespace vizehaber.Models
{
    public class Category : BaseEntity
    {
        public string Name { get; set; } // Kategori adı
        public ICollection<News> NewsList { get; set; } // Bu kategoriye ait haberler
    }
}