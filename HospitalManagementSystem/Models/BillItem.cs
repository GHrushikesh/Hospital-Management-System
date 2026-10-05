using System;
using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.Models
{
    public class BillItem
    {
        public int BillItemId { get; set; }

        public int BillId { get; set; }

        [Required]
        [StringLength(300)]
        public string Description { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; } = 1;

        [Range(typeof(decimal), "0", "999999")]
        public decimal UnitPrice { get; set; }

        [Range(typeof(decimal), "0", "999999")]
        public decimal Amount { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}