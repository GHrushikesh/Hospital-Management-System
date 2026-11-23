namespace HospitalManagementSystem.Models
{
    public class WardOccupancyViewModel
    {
        public IReadOnlyList<HospitalBed> Beds { get; set; } = Array.Empty<HospitalBed>();
        public IReadOnlyList<Patient> Patients { get; set; } = Array.Empty<Patient>();
        public IReadOnlyList<Doctor> Doctors { get; set; } = Array.Empty<Doctor>();
        public IReadOnlyList<BedAdmission> AdmissionHistory { get; set; } = Array.Empty<BedAdmission>();

        public int TotalBeds { get; set; }
        public int OccupiedBeds { get; set; }
        public int VacantBeds { get; set; }
        public double OccupancyPercentage => TotalBeds > 0 ? Math.Round((double)OccupiedBeds / TotalBeds * 100, 1) : 0;
    }
}
