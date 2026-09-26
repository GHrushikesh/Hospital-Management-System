using HospitalManagementSystem.Data.Repositories;
using HospitalManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementSystem.Controllers
{
    public class DoctorController : Controller
    {
        private readonly DoctorRepository _doctorRepository;

        public DoctorController(DoctorRepository doctorRepository)
        {
            _doctorRepository = doctorRepository;
        }

        public async Task<IActionResult> Index(string? search)
        {
            var doctors = string.IsNullOrWhiteSpace(search)
                ? await _doctorRepository.GetAllAsync()
                : await _doctorRepository.SearchAsync(search);

            ViewBag.SearchTerm = search;
            return View(doctors);
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (!IsAdmin())
            {
                return Forbid();
            }

            return View(new Doctor());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Doctor model)
        {
            if (!IsAdmin())
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            await _doctorRepository.AddAsync(model);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            var doctor = await _doctorRepository.GetByIdAsync(id);
            if (doctor is null)
            {
                return NotFound();
            }

            return View(doctor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Doctor model)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            await _doctorRepository.UpdateAsync(model);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            await _doctorRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        private bool IsAuthenticated() => !string.IsNullOrWhiteSpace(HttpContext.Session.GetString("LoggedInUserRole"));

        private bool IsAdmin() => string.Equals(HttpContext.Session.GetString("LoggedInUserRole"), "Admin", StringComparison.OrdinalIgnoreCase);
    }
}
