using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        [Required(ErrorMessage = "กรุณาระบุผลการตรวจ")]
        public string TreatmentDetails { get; set; }

        [Display(Name = "แพทย์ผู้ตรวจ")]
        [Required(ErrorMessage = "กรุณาระบุชื่อแพทย์ผู้ตรวจ")]
        public string DentistName { get; set; }

        [Display(Name = "สิทธิ์การรักษา")]
        [Required(ErrorMessage = "กรุณาเลือกสิทธิ์การรักษา")]
        public string TreatmentRights { get; set; }

        [Display(Name = "ชำระเงินแล้ว")]
        public bool IsPaid { get; set; }

        [Required]
        [Display(Name = "วันที่รักษา")]
        [DataType(DataType.DateTime)]
        public DateTime TreatmentDate { get; set; }

        // Navigation property สำหรับรายการเรียกเก็บเงิน
        public virtual ICollection<Billing> Billings { get; set; }
    }
}