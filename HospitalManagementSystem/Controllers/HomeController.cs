using HospitalManagementSystem.Data.Repositories;
using HospitalManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace HospitalManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly DoctorRepository _doctorRepository;

        public HomeController(DoctorRepository doctorRepository)
        {
            _doctorRepository = doctorRepository;
        }

        public async Task<IActionResult> Index()
        {
            // If already logged in, redirect straight to their respective dedicated workspace
            var role = HttpContext.Session.GetString("LoggedInUserRole");
            if (string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase) ||
                HttpContext.Session.GetString("AdminAuthenticated") == "true")
            {
                return RedirectToAction("Dashboard", "Admin");
            }
            if (string.Equals(role, "Doctor", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Dashboard", "DoctorAccount");
            }
            if (string.Equals(role, "Receptionist", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Dashboard", "Receptionist");
            }
            if (!string.IsNullOrWhiteSpace(role))
            {
                return RedirectToAction("Dashboard", "Staff");
            }

            // Public Patient / Guest Portal
            var doctors = (await _doctorRepository.GetAllAsync())
                .Where(d => d.IsActive)
                .ToList();

            var specialties = doctors
                .Select(d => d.Specialty)
                .Distinct()
                .OrderBy(s => s)
                .ToList();

            var viewModel = new PatientPortalViewModel
            {
                FeaturedDoctors = doctors,
                Specialties = specialties,
                TotalSpecialtiesCount = specialties.Count,
                TotalDoctorsAvailable = doctors.Count
            };

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
