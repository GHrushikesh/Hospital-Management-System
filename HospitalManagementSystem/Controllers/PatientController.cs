using HospitalManagementSystem.Data.Repositories;
using HospitalManagementSystem.Filters;
using HospitalManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace HospitalManagementSystem.Controllers
{
    [AdminAuthorize]
    public class PatientController : Controller
    {
        private readonly PatientRepository _patientRepository;

        public PatientController(PatientRepository patientRepository)
        {
            _patientRepository = patientRepository;
        }

        public async Task<IActionResult> Index(string? searchTerm)
        {
            IReadOnlyList<Patient> patients;

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                patients = await _patientRepository.SearchAsync(searchTerm);
                ViewBag.SearchTerm = searchTerm.Trim();
            }
            else
            {
                patients = await _patientRepository.GetAllAsync();
            }

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
            TempData["SuccessMessage"] = "Patient registered successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var patient = await _patientRepository.GetByIdAsync(id);
            if (patient == null)
            {
                TempData["ErrorMessage"] = "Patient not found.";
                return RedirectToAction(nameof(Index));
            }

            return View(patient);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Patient model)
        {
            if (id != model.PatientId)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var updated = await _patientRepository.UpdateAsync(model);
            if (!updated)
            {
                ModelState.AddModelError(string.Empty, "Unable to save changes. The patient record may no longer exist.");
                return View(model);
            }

            TempData["SuccessMessage"] = "Patient details updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var patient = await _patientRepository.GetByIdAsync(id);
                if (patient == null)
                {
                    TempData["ErrorMessage"] = "Patient not found.";
                    return RedirectToAction(nameof(Index));
                }

                var deleted = await _patientRepository.DeleteAsync(id);
                if (deleted)
                {
                    TempData["SuccessMessage"] = $"Patient '{patient.FirstName} {patient.LastName}' deleted successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete patient. The record might have already been removed.";
                }
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                TempData["ErrorMessage"] = "Cannot delete this patient because they have related appointments in the system. Please cancel or remove their appointments first.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An unexpected error occurred while deleting the patient: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
