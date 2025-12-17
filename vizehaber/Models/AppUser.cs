using Microsoft.AspNetCore.Identity;
using System.Collections.Generic; // Listeler için gerekli
using System;

namespace vizehaber.Models
{
    public class AppUser : IdentityUser
    {
        public string? FullName { get; set; }
        public string? PhotoUrl { get; set; }
        public string? Biography { get; set; }
        public string? Specialization { get; set; }
        public bool IsActive { get; set; } = true;


        public DateTime CreatedDate { get; set; } = DateTime.Now;


        public virtual ICollection<News> News { get; set; }


        public virtual ICollection<Comment> Comments { get; set; }
    }
}