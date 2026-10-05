using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.Models
{
    public class Bill
    {
        public int BillId { get; set; }

        [Required]
        public int PatientId { get; set; }

        public int? AppointmentId { get; set; }

        public int? DoctorId { get; set; }

        public DateTime BillDate { get; set; }

        [Range(typeof(decimal), "0", "999999")]
        public decimal SubTotal { get; set; }

        [Range(typeof(decimal), "0", "999999")]
        public decimal Tax { get; set; }

        [Range(typeof(decimal), "0", "999999")]
        public decimal Discount { get; set; }

        [Range(typeof(decimal), "0", "999999")]
        public decimal Total { get; set; }

        [StringLength(50)]
        public string PaymentStatus { get; set; } = "Unpaid";

        [StringLength(50)]
        public string? PaymentMethod { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; }

        public string? PatientName { get; set; }

        public string? DoctorName { get; set; }

        public List<BillItem> Items { get; set; } = new List<BillItem>();
    }
}