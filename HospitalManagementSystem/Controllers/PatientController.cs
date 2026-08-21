using HospitalManagementSystem.Data.Repositories;
using HospitalManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementSystem.Controllers
{
    public class PatientController : Controller
    {
        private readonly PatientRepository _patientRepository;

        public PatientController(PatientRepository patientRepository)
        {
            _patientRepository = patientRepository;
        }

        public async Task<IActionResult> Index()
        {
            var patients = await _patientRepository.GetAllAsync();
            return View(patients);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new Patient());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Patient model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            await _patientRepository.AddAsync(model);
            return RedirectToAction(nameof(Index));
        }
    }
}
