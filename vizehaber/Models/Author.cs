//Author.cs  yazar modeli, yazdığı haberlerle ilişkilendirildi

namespace vizehaber.Models
{
    public class Author : BaseEntity
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        public ICollection<News> NewsList { get; set; }  // Yazarın yazdığı haberler
    }
}
