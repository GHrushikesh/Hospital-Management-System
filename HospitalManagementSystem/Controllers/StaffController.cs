using HospitalManagementSystem.Data.Repositories;
using HospitalManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;

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
            HttpContext.Session.SetString("StaffAuthenticated", "true");
            return RedirectToAction("Index", "Home");
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

            model.PasswordHash = model.PasswordHash.Trim();
            model.Role = string.IsNullOrWhiteSpace(model.Role) ? "Staff" : model.Role.Trim();

            await _staffRepository.AddAsync(model);
            return RedirectToAction(nameof(Login));
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction(nameof(Login));
        }
    }
}
