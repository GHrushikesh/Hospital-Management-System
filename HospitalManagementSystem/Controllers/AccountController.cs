using HospitalManagementSystem.Data.Repositories;
using HospitalManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

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

            var admin = await _adminRepository.AuthenticateAsync(model.Username, model.Password);
            if (admin is not null)
            {
                SetSession("Admin", admin.Username);
                return RedirectToLocal(returnUrl);
            }

            var staff = await _staffRepository.AuthenticateAsync(model.Username, model.Password);
            if (staff is not null)
            {
                SetSession(staff.Role, staff.FullName, staff.Department);
                return RedirectToLocal(returnUrl, "Staff", "Dashboard");
            }

            var doctor = await _doctorRepository.AuthenticateAsync(model.Username, model.Password);
            if (doctor is not null)
            {
                HttpContext.Session.Clear();
                HttpContext.Session.SetString("LoggedInUserRole", "Doctor");
                HttpContext.Session.SetString("LoggedInUserName", doctor.FullName);
                HttpContext.Session.SetString("DoctorId", doctor.DoctorId.ToString());
                HttpContext.Session.SetString("DoctorSpecialty", doctor.Specialty);
                HttpContext.Session.SetString("DoctorUsername", doctor.Username);
                return RedirectToLocal(returnUrl, "DoctorAccount", "Dashboard");
            }

            ModelState.AddModelError(string.Empty, "Invalid username or password.");
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        [HttpGet]
        public IActionResult Signup()
        {
            return View(new StaffUser());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Signup(StaffUser model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.Role = "Staff";
            model.FullName = model.FullName.Trim();
            model.Username = model.Username.Trim();
            model.Email = model.Email.Trim();
            model.PhoneNumber = model.PhoneNumber.Trim();
            model.Department = model.Department.Trim();
            model.JobTitle = model.JobTitle.Trim();
            model.PasswordHash = model.PasswordHash.Trim();

            try
            {
                await _staffRepository.AddAsync(model);
            }
            catch (SqlException ex) when (ex.Number is 2601 or 2627)
            {
                ModelState.AddModelError(nameof(model.Username), "That username is already in use.");
                return View(model);
            }
            return RedirectToAction(nameof(Login));
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        private void SetSession(string role, string name, string? department = null)
        {
            HttpContext.Session.SetString("LoggedInUserRole", role);
            HttpContext.Session.SetString("LoggedInUserName", name);
            HttpContext.Session.SetString("StaffDepartment", department ?? string.Empty);

            if (string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                HttpContext.Session.SetString("AdminAuthenticated", "true");
                HttpContext.Session.SetString("AdminUsername", name);
            }
        }

        private IActionResult RedirectToLocal(string? returnUrl, string fallbackController = "Home", string fallbackAction = "Index")
        {
            return !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
                ? Redirect(returnUrl)
                : RedirectToAction(fallbackAction, fallbackController);
        }
    }
}
