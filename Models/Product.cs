using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BanHang.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public string? ImageUrl { get; set; }

        public string? Description { get; set; }

        public string Category { get; set; } // Laptop / LinhKien / PhuKien
        public string? Type { get; set; }
        public string? Brand { get; set; }     // ASUS, MSI, Acer, Dell...

        public int? Ram { get; set; }          // 8, 16, 32 (GB)     

        public string? AccessoryType { get; set; } // Chuot, BanPhim, TaiNghe...



    }
}