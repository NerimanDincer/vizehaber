namespace vizehaber.Models
{
    public class User : BaseEntity
    {
        public string FullName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Role { get; set; } = "User"; // Admin, Author, User
        public string? PhotoPath { get; set; }
        public string? Biography { get; set; }

        // Bir Yazarın birden çok Haberi olabilir
        public ICollection<News> News { get; set; }
        public ICollection<Comment> Comments { get; set; } // Bir Kullanıcının birden çok Yorumu olabilir
    }
}