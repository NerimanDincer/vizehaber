namespace vizehaber.Models
{
    public class Haber : BaseEntity
    {
        public string Baslik { get; set; }
        public string Icerik { get; set; }
        public string Resim { get; set; }
        public DateTime YayimTarihi { get; set; }

        //kategori ilişkisi kısmı
        public int CategoryId { get; set; }
        public Category Category { get; set; }
    }
}
