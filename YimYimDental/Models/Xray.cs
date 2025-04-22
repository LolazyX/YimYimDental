using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations.Schema;

namespace YimYimDental.Models
{
    public class Xray
    {
        [Key]
        public int Id { get; set; }

        // FK ไปยัง Customer
        [Required]
        [Display(Name = "ลูกค้า")]
        public int CustomerId { get; set; }

        [ForeignKey("CustomerId")]
        public Customer? Customer { get; set; }

        [Display(Name = "รูปเอ็กเรย์")]
        public string ImagePath { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        [Display(Name = "วันที่บันทึก")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
