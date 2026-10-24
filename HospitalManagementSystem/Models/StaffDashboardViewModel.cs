namespace HospitalManagementSystem.Models
{
    public class StaffDashboardViewModel
    {
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = "Staff";
        public string Department { get; set; } = string.Empty;
        public IReadOnlyList<string> Tasks { get; set; } = Array.Empty<string>();
    }
}
