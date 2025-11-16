//Category.cs haber kategorilerini tutuyor, News ile ilişkilendi

namespace vizehaber.Models
{
    public class Category : BaseEntity
    {
        public string Name { get; set; } // Kategori adı

        //  Boş koleksiyon tanımı eklendi
        public ICollection<News> NewsList { get; set; } = new List<News>(); // Bu kategoriye ait haberler
    }
}
