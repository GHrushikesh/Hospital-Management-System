using HospitalManagementSystem.Data.Repositories;
using HospitalManagementSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementSystem.Controllers
{
    public class DashboardController : Controller
    {
        private readonly PatientRepository _patientRepository;
        private readonly DoctorRepository _doctorRepository;
        private readonly AppointmentRepository _appointmentRepository;

        // Constructor injection following clean architecture and dependency injection best practices
        public DashboardController(
            PatientRepository patientRepository,
            DoctorRepository doctorRepository,
            AppointmentRepository appointmentRepository)
        {
            _patientRepository = patientRepository;
            _doctorRepository = doctorRepository;
            _appointmentRepository = appointmentRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Security Check: Verify admin session exists before allowing dashboard access
            if (HttpContext.Session.GetString("AdminUser") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Keep controller thin: delegate count retrieval to ADO.NET repository layer
            var viewModel = new DashboardViewModel
            {
                TotalPatients = await _patientRepository.GetTotalCountAsync(),
                TotalDoctors = await _doctorRepository.GetTotalCountAsync(),
                TotalAppointments = await _appointmentRepository.GetTotalCountAsync(),
                TotalBills = 0 // Set to 0 since Billing module is scheduled for a future milestone
            };

            return View(viewModel);
        }
    }
}
