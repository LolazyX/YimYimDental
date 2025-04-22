using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace YimYimDental.Models
{
    public class Equipment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "ชื่ออุปกรณ์")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "ราคา")]
        [Range(0, double.MaxValue, ErrorMessage = "กรุณากรอกราคาที่มากกว่าหรือเท่ากับ 0")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
    }
}
