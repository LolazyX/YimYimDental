using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YimYimDental.Models
{
    public class Service
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "ชื่อบริการ")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "ราคา")]
        [Range(0, double.MaxValue, ErrorMessage = "กรุณากรอกราคาที่มากกว่าหรือเท่ากับ 0")]
        [Column(TypeName = "decimal(18,2)")]  // กำหนด precision และ scale ที่นี่
        public decimal Price { get; set; }
    }
}
