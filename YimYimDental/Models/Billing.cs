using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YimYimDental.Models
{
    public class Billing
    {
        [Key]
        public int Id { get; set; }

        public int TreatmentHistoryId { get; set; }
        [ForeignKey("TreatmentHistoryId")]
        public TreatmentHistory TreatmentHistory { get; set; }

        [Required]
        public int ItemId { get; set; } // เปลี่ยนจาก string เป็น int

        [Required]
        public string ItemType { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        public string PaymentStatus { get; set; }
    }
}