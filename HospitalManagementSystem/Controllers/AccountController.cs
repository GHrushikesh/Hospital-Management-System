using HospitalManagementSystem.Data.Repositories;
using HospitalManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly AdminRepository _adminRepository;
        private readonly StaffRepository _staffRepository;

        public AccountController(AdminRepository adminRepository, StaffRepository staffRepository)
        {
            _adminRepository = adminRepository;
            _staffRepository = staffRepository;
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
                SetSession(staff.Role, staff.FullName);
                return RedirectToLocal(returnUrl);
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
            await _staffRepository.AddAsync(model);
            return RedirectToAction(nameof(Login));
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        private void SetSession(string role, string name)
        {
            HttpContext.Session.SetString("LoggedInUserRole", role);
            HttpContext.Session.SetString("LoggedInUserName", name);
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            return !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
                ? Redirect(returnUrl)
                : RedirectToAction("Index", "Home");
        }
    }
}
