using System.ComponentModel.DataAnnotations;

namespace YimYimDental.Models
{
    public class Customer
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "HN")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "HN ต้องเป็นตัวเลข 6 หลัก")]
        public string HN { get; set; }

        [Required]
        [Display(Name = "ชื่อ-นามสกุล")]
        public string FullName { get; set; }

        [Required]
        [Display(Name = "เลขบัตรประชาชน")]
        [StringLength(13, MinimumLength = 13, ErrorMessage = "เลขบัตรประชาชนต้องมี 13 หลัก")]
        public string NationalId { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "วันเดือนปีเกิด")]
        public DateTime? DateOfBirth { get; set; }

        [Display(Name = "เพศ")]
        public string? Gender { get; set; }

        [Display(Name = "ที่อยู่")]
        public string Address { get; set; }

        [Display(Name = "เบอร์โทร")]
        [Phone(ErrorMessage = "กรุณากรอกเบอร์โทรที่ถูกต้อง")]
        public string? Phone { get; set; }

        [EmailAddress]
        [Display(Name = "อีเมล (ถ้ามี)")]
        public string? Email { get; set; }

        [Display(Name = "โรคประจำตัว")]
        public string? ChronicDiseases { get; set; }

        [Display(Name = "ประวัติการแพ้ยา/อาหาร")]
        public string? Allergies { get; set; }

        // Navigation
        public ICollection<TreatmentHistory>? TreatmentHistories { get; set; }
    }
}
