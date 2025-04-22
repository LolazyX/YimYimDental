using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace YimYimDental.Models
{
    public class TreatmentHistory
    {
        [Key]
        public int Id { get; set; }

        public int CustomerId { get; set; }

        [ForeignKey("CustomerId")]
        public Customer Customer { get; set; }

        [Display(Name = "ผลการตรวจ")]
        public string TreatmentDetails { get; set; }

        [Display(Name = "อุปกรณ์เพิ่มเติม")]
        public string? EquipmentDetails { get; set; }

        [Display(Name = "แพทย์ผู้ตรวจ")]
        public string DentistName { get; set; }

        [Display(Name = "สิทธิ์การรักษา")]
        public string TreatmentRights { get; set; }

        [Required]
        [Display(Name = "วันที่รักษา")]
        [DataType(DataType.Date)]
        public DateTime TreatmentDate { get; set; }
    }
}
