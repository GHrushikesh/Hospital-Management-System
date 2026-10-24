using HospitalManagementSystem.Data.Repositories;
using HospitalManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace HospitalManagementSystem.Controllers
{
    public class StaffController : Controller
    {
        private readonly StaffRepository _staffRepository;

        public StaffController(StaffRepository staffRepository)
        {
            _staffRepository = staffRepository;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View(new StaffLoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(StaffLoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _staffRepository.AuthenticateAsync(model.Username, model.Password);
            if (user is null)
            {
                ModelState.AddModelError(string.Empty, "Invalid staff credentials.");
                return View(model);
            }

            HttpContext.Session.SetString("LoggedInUserRole", user.Role);
            HttpContext.Session.SetString("LoggedInUserName", user.FullName);
            HttpContext.Session.SetString("StaffDepartment", user.Department);
            HttpContext.Session.SetString("StaffAuthenticated", "true");
            return RedirectToAction(nameof(Dashboard));
        }

        [HttpGet]
        public IActionResult Dashboard()
        {
            if (string.IsNullOrWhiteSpace(HttpContext.Session.GetString("LoggedInUserName")))
            {
                return RedirectToAction(nameof(Login));
            }

            var role = HttpContext.Session.GetString("LoggedInUserRole") ?? "Staff";
            var tasks = role.ToLowerInvariant() switch
            {
                "receptionist" => new[] { "Register and update patient details.", "Review new appointment requests.", "Confirm appointment times with patients." },
                "nurse" => new[] { "Review today's appointment queue.", "Record patient observations for the care team.", "Flag urgent cases for the doctor." },
                "doctor assistant" => new[] { "Prepare the doctor's daily schedule.", "Update appointment notes and statuses.", "Coordinate follow-up appointments." },
                _ => new[] { "Review today's hospital schedule.", "Keep patient and appointment records up to date.", "Escalate issues to the administrator." }
            };

            return View(new StaffDashboardViewModel
            {
                FullName = HttpContext.Session.GetString("LoggedInUserName") ?? "Staff member",
                Role = role,
                Department = HttpContext.Session.GetString("StaffDepartment") ?? "General",
                Tasks = tasks
            });
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View(new StaffUser());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(StaffUser model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.FullName = model.FullName.Trim();
            model.Username = model.Username.Trim();
            model.PasswordHash = model.PasswordHash.Trim();
            model.PhoneNumber = model.PhoneNumber.Trim();
            model.Email = model.Email.Trim();
            model.Department = model.Department.Trim();
            model.JobTitle = model.JobTitle.Trim();
            model.Role = string.IsNullOrWhiteSpace(model.Role) ? "Staff" : model.Role.Trim();

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
            return RedirectToAction(nameof(Login));
        }
    }
}
