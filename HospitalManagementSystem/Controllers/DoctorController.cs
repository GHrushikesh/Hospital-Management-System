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

        public async Task<IActionResult> Index()
        {
            var doctors = await _doctorRepository.GetAllAsync();
            return View(doctors);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new Doctor());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Doctor model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            await _doctorRepository.AddAsync(model);
            return RedirectToAction(nameof(Index));
        }
    }
}
