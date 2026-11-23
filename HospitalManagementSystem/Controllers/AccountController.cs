using HospitalManagementSystem.Data.Repositories;
using HospitalManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly AdminRepository _adminRepository;
        private readonly StaffRepository _staffRepository;
        private readonly DoctorRepository _doctorRepository;

        public AccountController(AdminRepository adminRepository, StaffRepository staffRepository, DoctorRepository doctorRepository)
        {
            _adminRepository = adminRepository;
            _staffRepository = staffRepository;
            _doctorRepository = doctorRepository;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View(new AccountLoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(AccountLoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ReturnUrl = returnUrl;
                return View(model);
            }

            // 1. Check Administrator
            var admin = await _adminRepository.AuthenticateAsync(model.Username, model.Password);
            if (admin is not null)
            {
                HttpContext.Session.Clear();
                HttpContext.Session.SetString("AdminAuthenticated", "true");
                HttpContext.Session.SetString("AdminUsername", admin.Username);
                HttpContext.Session.SetString("LoggedInUserRole", "Admin");
                HttpContext.Session.SetString("LoggedInUserName", admin.Username);
                return RedirectToLocal(returnUrl, "Admin", "Dashboard");
            }

            // 2. Check Doctor
            var doctor = await _doctorRepository.AuthenticateAsync(model.Username, model.Password);
            if (doctor is not null)
            {
                HttpContext.Session.Clear();
                HttpContext.Session.SetString("LoggedInUserRole", "Doctor");
                HttpContext.Session.SetString("LoggedInUserName", doctor.FullName);
                HttpContext.Session.SetString("DoctorId", doctor.DoctorId.ToString());
                HttpContext.Session.SetString("DoctorSpecialty", doctor.Specialty);
                HttpContext.Session.SetString("DoctorQualification", doctor.Qualification);
                HttpContext.Session.SetString("DoctorRoomNumber", doctor.RoomNumber ?? string.Empty);
                HttpContext.Session.SetString("DoctorUsername", doctor.Username);
                return RedirectToLocal(returnUrl, "DoctorAccount", "Dashboard");
            }

            // 3. Check Staff (Nurse, Receptionist, etc.)
            var staff = await _staffRepository.AuthenticateAsync(model.Username, model.Password);
            if (staff is not null)
            {
                HttpContext.Session.Clear();
                HttpContext.Session.SetString("LoggedInUserRole", staff.Role);
                HttpContext.Session.SetString("LoggedInUserName", staff.FullName);
                HttpContext.Session.SetString("StaffDepartment", staff.Department);
                HttpContext.Session.SetString("StaffQualification", staff.Qualification);
                HttpContext.Session.SetString("StaffAssignedRoomOrWard", staff.AssignedRoomOrWard);
                HttpContext.Session.SetString("StaffShiftTiming", staff.ShiftTiming);
                HttpContext.Session.SetString("StaffHiredByDoctorName", staff.HiredByDoctorName ?? string.Empty);
                HttpContext.Session.SetString("StaffAuthenticated", "true");

                if (string.Equals(staff.Role, "Receptionist", StringComparison.OrdinalIgnoreCase))
                {
                    return RedirectToLocal(returnUrl, "Receptionist", "Dashboard");
                }

                return RedirectToLocal(returnUrl, "Staff", "Dashboard");
            }

            ModelState.AddModelError(string.Empty, "Invalid username or password.");
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        [HttpGet]
        public IActionResult Signup()
        {
            TempData["InfoMessage"] = "In accordance with hospital security protocols, staff and clinical accounts are provisioned exclusively by Hospital Administration.";
            return RedirectToAction(nameof(Login));
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        private IActionResult RedirectToLocal(string? returnUrl, string fallbackController = "Home", string fallbackAction = "Index")
        {
            return !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
                ? Redirect(returnUrl)
                : RedirectToAction(fallbackAction, fallbackController);
        }
    }
}
