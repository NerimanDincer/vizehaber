using System.ComponentModel.DataAnnotations;
using vizehaber.Models;
using vizehaber.ViewModels;


namespace vizehaber.ViewModels
{
    public class CategoryModel
    {
        
            public int Id { get; set; }

            [Required(ErrorMessage = "Kategori adı boş olamaz")]
            [StringLength(100, ErrorMessage = "Kategori adı 100 karakterden uzun olamaz")]
            public string Name { get; set; }

            public bool IsActive { get; set; }

            // Oluşturulma ve güncellenme tarihleri
            public DateTime Created { get; set; }
            public DateTime Updated { get; set; }
        

    }
}

