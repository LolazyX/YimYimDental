using System.ComponentModel.DataAnnotations;

namespace YimYimDental.Models
{
    public class WorkingTime
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "กรุณาเลือกทันตแพทย์")]
        [Display(Name = "ทันตแพทย์")]
        public required string DentistName { get; set; }

        [Required(ErrorMessage = "กรุณาระบุเวลาเริ่มต้น")]
        [DataType(DataType.DateTime)]
        [Display(Name = "เวลาเริ่มต้น")]
        public DateTime Start { get; set; }

        [Required(ErrorMessage = "กรุณาระบุเวลาสิ้นสุด")]
        [DataType(DataType.DateTime)]
        [Display(Name = "เวลาสิ้นสุด")]
        public DateTime End { get; set; }
    }
}
