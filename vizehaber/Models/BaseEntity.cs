// baseentity tüm modellerin temel sınıfıs

namespace vizehaber.Models
{
    public class BaseEntity
    {
        public int Id { get; set; }  //primary key
        public bool IsActive { get; set; } //aktif mi değil mi

        public DateTime Created { get; set; } //oluşturulma tarihi
        public DateTime Updated { get; set; } //güncellenme tarihis
    }
}
