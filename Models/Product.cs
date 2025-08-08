using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace MyProject.Models
{
    public class Product
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = null!; // Tambahkan = null! untuk handle warning

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public int Qty { get; set; }

        public string Description { get; set; } = null!; // Tambahkan = null! untuk handle warning

        public string? ImageUrl { get; set; }

        [Required]
        public Guid CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        [JsonIgnore] // Tambahkan ini untuk mengabaikan properti navigasi saat serialisasi
        public virtual ProductCategory? Category { get; set; } // Jadikan nullable
    }
}
