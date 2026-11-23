using HospitalManagementSystem.Data.Repositories;
using HospitalManagementSystem.Filters;
using HospitalManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementSystem.Controllers
{
    public class AdminController : Controller
    {
        private readonly AdminRepository _adminRepository;
        private readonly StaffRepository _staffRepository;
        private readonly DoctorRepository _doctorRepository;
        private readonly PatientRepository _patientRepository;
        private readonly AppointmentRepository _appointmentRepository;

        public AdminController(
            AdminRepository adminRepository,
            StaffRepository staffRepository,
            DoctorRepository doctorRepository,
            PatientRepository patientRepository,
            AppointmentRepository appointmentRepository)
        {
            _adminRepository = adminRepository;
            _staffRepository = staffRepository;
            _doctorRepository = doctorRepository;
            _patientRepository = patientRepository;
            _appointmentRepository = appointmentRepository;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("AdminAuthenticated") == "true")
            {
                return RedirectToAction(nameof(Dashboard));
            }

            return View(new AdminLoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(AdminLoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _adminRepository.AuthenticateAsync(model.Username, model.Password);
            if (user is null)
            {
                ModelState.AddModelError(string.Empty, "Invalid administrator credentials.");
                return View(model);
            }

            HttpContext.Session.Clear();
            HttpContext.Session.SetString("AdminAuthenticated", "true");
            HttpContext.Session.SetString("AdminUsername", user.Username);
            HttpContext.Session.SetString("LoggedInUserRole", "Admin");
            HttpContext.Session.SetString("LoggedInUserName", user.Username);

            return RedirectToAction(nameof(Dashboard));
        }

        [HttpGet]
        [AdminOnlyAuthorize]
        public async Task<IActionResult> Dashboard()
        {
            var viewModel = new DashboardViewModel();
            try
            {
                viewModel.PatientCount = (await _patientRepository.GetAllAsync()).Count;
                viewModel.DoctorCount = (await _doctorRepository.GetAllAsync()).Count;
                viewModel.AppointmentCount = (await _appointmentRepository.GetHistoryAsync()).Count;
                viewModel.TodayAppointments = await _appointmentRepository.GetTodayAppointmentsAsync();
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
            }

            return View(viewModel);
        }

        [HttpGet]
        [AdminOnlyAuthorize]
        public async Task<IActionResult> Staff()
        {
            var staffList = await _staffRepository.GetAllAsync();
            return View(staffList);
        }

        [HttpGet]
        [AdminOnlyAuthorize]
        public IActionResult CreateStaff()
        {
            TempData["InfoMessage"] = "Under Hospital Governance Policy, staff members can only be recruited & hired directly by Doctors / Clinical Leads from their Doctor Clinical Desk.";
            return RedirectToAction(nameof(Staff));
        }

        [HttpPost]
        [AdminOnlyAuthorize]
        [ValidateAntiForgeryToken]
        public IActionResult CreateStaff(StaffUser model)
        {
            TempData["ErrorMessage"] = "Staff recruitment authority is restricted solely to Doctors and Clinical Department Leads.";
            return RedirectToAction(nameof(Staff));
        }

        [HttpPost]
        [AdminOnlyAuthorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteStaff(int id)
        {
            var success = await _staffRepository.DeleteAsync(id);
            if (success)
            {
                TempData["SuccessMessage"] = "Staff record removed successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Could not remove staff record.";
            }

            return RedirectToAction(nameof(Staff));
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}
