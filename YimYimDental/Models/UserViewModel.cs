using System.ComponentModel.DataAnnotations;

namespace YimYimDental.Models
{
    public class UserViewModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "ตำแหน่ง")]
        public string Position { get; set; }

        [Required]
        [Display(Name = "ชื่อ-สกุล")]
        public string FullName { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "วัน เดือน ปี เกิด")]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [Display(Name = "เบอร์โทร")]
        public string Phone { get; set; }

        [EmailAddress]
        [Display(Name = "อีเมล")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "รหัสผ่าน")]
        public string Password { get; set; }

        [Display(Name = "ที่อยู่")]
        public string Address { get; set; }

        [Required]
        [Display(Name = "สิทธิ์การใช้งาน")]
        public string Role { get; set; }
    }
}
