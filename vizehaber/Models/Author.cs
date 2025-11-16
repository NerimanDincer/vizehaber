//Author.cs  yazar modeli, yazdığı haberlerle ilişkilendirildi

namespace vizehaber.Models
{
    public class Author : BaseEntity
    {
        public string Name { get; set; }            // Yazar adı
        public string Email { get; set; }           // Mail
        public string Password { get; set; }        // Giriş şifresi 

        // wwwroot/userPhotos ile uyumlu hale getirildi
        public string? PhotoPath { get; set; }      // Fotoğraf dosya yolu

        public string? Bio { get; set; }            // Kısa açıklama, biyografi

        //  ICollection nullable olmamalı (EF için)
        public ICollection<News> News { get; set; } = new List<News>(); // Yazarın haberleri
    }
}

