namespace HospitalManagementSystem.Models
{
    public class PatientPortalViewModel
    {
        public IReadOnlyList<Doctor> FeaturedDoctors { get; set; } = Array.Empty<Doctor>();
        public IReadOnlyList<string> Specialties { get; set; } = Array.Empty<string>();
        public int TotalSpecialtiesCount { get; set; }
        public int TotalDoctorsAvailable { get; set; }
    }
}
