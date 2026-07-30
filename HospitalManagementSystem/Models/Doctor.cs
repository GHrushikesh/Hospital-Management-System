using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.Models
{
    public class Doctor
    {
        public int DoctorId { get; set; }

        [Required]
        [StringLength(150)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Specialty { get; set; } = string.Empty;

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [EmailAddress]
        [StringLength(150)]
        public string? Email { get; set; }

        [StringLength(20)]
        public string? RoomNumber { get; set; }

        [StringLength(100)]
        public string? Availability { get; set; }

        [Range(typeof(decimal), "0", "999999")]
        public decimal ConsultationFee { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; }
    }
}