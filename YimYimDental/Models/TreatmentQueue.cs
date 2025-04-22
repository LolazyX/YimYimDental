using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YimYimDental.Models
{
    public class TreatmentQueue
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "รหัสลูกค้า")]
        public int CustomerId { get; set; }

        [ForeignKey("CustomerId")]
        public Customer? Customer { get; set; }

        [Required]
        [Display(Name = "เวลานัดหมาย")]
        public DateTime AppointmentTime { get; set; }

        [Display(Name = "ลำดับคิว")]
        public int QueueNumber { get; set; }

        [Required]
        [Display(Name = "สถานะ")]
        public string Status { get; set; } = "รอรักษา";

        [Display(Name = "อาการเบื้องต้น")]
        public string? PreliminarySymptoms { get; set; }
    }
}