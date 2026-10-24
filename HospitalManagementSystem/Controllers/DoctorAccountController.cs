using HospitalManagementSystem.Data.Repositories;
using HospitalManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementSystem.Controllers
{
    public class DoctorAccountController : Controller
    {
        private readonly DoctorRepository _doctorRepository;

        public DoctorAccountController(DoctorRepository doctorRepository)
        {
            _doctorRepository = doctorRepository;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View(new DoctorLoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(DoctorLoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ReturnUrl = returnUrl;
                return View(model);
            }

            var doctor = await _doctorRepository.AuthenticateAsync(model.Username, model.Password);
            if (doctor is null)
            {
                ModelState.AddModelError(string.Empty, "Invalid doctor credentials or inactive account.");
                ViewBag.ReturnUrl = returnUrl;
                return View(model);
            }

            HttpContext.Session.Clear();
            HttpContext.Session.SetString("LoggedInUserRole", "Doctor");
            HttpContext.Session.SetString("LoggedInUserName", doctor.FullName);
            HttpContext.Session.SetString("DoctorId", doctor.DoctorId.ToString());
            HttpContext.Session.SetString("DoctorSpecialty", doctor.Specialty);
            HttpContext.Session.SetString("DoctorUsername", doctor.Username);
            return !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
                ? Redirect(returnUrl)
                : RedirectToAction(nameof(Dashboard));
        }

        [HttpGet]
        public IActionResult Dashboard()
        {
            if (HttpContext.Session.GetString("LoggedInUserRole") != "Doctor")
            {
                return RedirectToAction(nameof(Login));
            }

            return View(new DoctorDashboardViewModel
            {
                FullName = HttpContext.Session.GetString("LoggedInUserName") ?? "Doctor",
                Specialty = HttpContext.Session.GetString("DoctorSpecialty") ?? "Medical practice",
                Username = HttpContext.Session.GetString("DoctorUsername") ?? string.Empty
            });
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction(nameof(Login));
        }
    }
}
