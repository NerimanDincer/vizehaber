using System;

namespace vizehaber.Models
{
    public class Contact : BaseEntity
    {
        public string Name { get; set; }          // Gönderen adı
        public string Email { get; set; }         // Mail adresi
        public string Subject { get; set; }       // Konu
        public string Message { get; set; }       // Mesaj içeriği

        // Opsiyonel fotoğraf (kullanıcı ihbar eklerse)
        public string? PhotoPath { get; set; }

        public DateTime Date { get; set; } = DateTime.Now; // Gönderim tarihi
    }
}
