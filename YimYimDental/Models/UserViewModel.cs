using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YimYimDental.Models
{
    public class UserViewModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "ชื่อ-สกุล")]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "อีเมล")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "เบอร์โทร")]
        public string Phone { get; set; }

        [Required]
        [Display(Name = "รหัสผ่าน")]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).{8,}$", ErrorMessage = "รหัสผ่านต้องมีตัวอักษรและตัวเลขอย่างน้อย 8 ตัว")]
        public string Password { get; set; }

        [NotMapped]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "รหัสผ่านไม่ตรงกัน")]
        [Display(Name = "ยืนยันรหัสผ่าน")]
        public string ConfirmPassword { get; set; }

        [Required]
        [Display(Name = "สิทธิ์การใช้งาน")]
        public string Role { get; set; }

        [Display(Name = "ที่อยู่")]
        public string? Address { get; set; }
    }
}
