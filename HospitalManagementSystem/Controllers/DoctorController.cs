using HospitalManagementSystem.Data.Repositories;
using HospitalManagementSystem.Filters;
using HospitalManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace HospitalManagementSystem.Controllers
{
    [AdminAuthorize]
    public class DoctorController : Controller
    {
        private readonly DoctorRepository _doctorRepository;

        public DoctorController(DoctorRepository doctorRepository)
        {
            _doctorRepository = doctorRepository;
        }

        public async Task<IActionResult> Index(string? searchTerm)
        {
            IReadOnlyList<Doctor> doctors;

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                doctors = await _doctorRepository.SearchAsync(searchTerm);
                ViewBag.SearchTerm = searchTerm.Trim();
            }
            else
            {
                doctors = await _doctorRepository.GetAllAsync();
            }

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
            TempData["SuccessMessage"] = "Doctor registered successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var doctor = await _doctorRepository.GetByIdAsync(id);
            if (doctor == null)
            {
                TempData["ErrorMessage"] = "Doctor not found.";
                return RedirectToAction(nameof(Index));
            }

            return View(doctor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Doctor model)
        {
            if (id != model.DoctorId)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var updated = await _doctorRepository.UpdateAsync(model);
            if (!updated)
            {
                ModelState.AddModelError(string.Empty, "Unable to save changes. The doctor record may no longer exist.");
                return View(model);
            }

            TempData["SuccessMessage"] = "Doctor details updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var doctor = await _doctorRepository.GetByIdAsync(id);
                if (doctor == null)
                {
                    TempData["ErrorMessage"] = "Doctor not found.";
                    return RedirectToAction(nameof(Index));
                }

                var deleted = await _doctorRepository.DeleteAsync(id);
                if (deleted)
                {
                    TempData["SuccessMessage"] = $"Dr. '{doctor.FullName}' deleted successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete doctor. The record might have already been removed.";
                }
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                TempData["ErrorMessage"] = "Cannot delete this doctor because they have existing scheduled or past appointments. Please reassign or cancel their appointments first.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An unexpected error occurred while deleting the doctor: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
