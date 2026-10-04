using HospitalManagementSystem.Data.Repositories;
using HospitalManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementSystem.Controllers
{
    public class MedicalRecordController : Controller
    {
        private readonly MedicalRecordRepository _medicalRecordRepository;

        public MedicalRecordController(
            MedicalRecordRepository medicalRecordRepository)
        {
            _medicalRecordRepository = medicalRecordRepository;
        }

        public async Task<IActionResult> Index(string? searchTerm)
        {
            IReadOnlyList<MedicalRecord> records;

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                records = await _medicalRecordRepository.GetAllAsync();
            }
            else
            {
                records = await _medicalRecordRepository.SearchAsync(searchTerm);
            }

            ViewBag.SearchTerm = searchTerm;

            return View(records);
        }

        public async Task<IActionResult> Details(int id)
        {
            MedicalRecord? record =
                await _medicalRecordRepository.GetByIdAsync(id);

            if (record == null)
            {
                return NotFound();
            }

            return View(record);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new MedicalRecord
            {
                RecordDate = DateTime.Now
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MedicalRecord record)
        {
            if (!ModelState.IsValid)
            {
                return View(record);
            }

            try
            {
                int medicalRecordId =
                    await _medicalRecordRepository.AddAsync(record);

                return RedirectToAction(
                    nameof(Details),
                    new { id = medicalRecordId });
            }
            catch (Exception)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Unable to create the medical record. Please check the patient, doctor, and appointment details.");

                return View(record);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            MedicalRecord? record =
                await _medicalRecordRepository.GetByIdAsync(id);

            if (record == null)
            {
                return NotFound();
            }

            return View(record);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            MedicalRecord record)
        {
            if (id != record.MedicalRecordId)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(record);
            }

            try
            {
                bool updated =
                    await _medicalRecordRepository.UpdateAsync(record);

                if (!updated)
                {
                    return NotFound();
                }

                return RedirectToAction(
                    nameof(Details),
                    new { id = record.MedicalRecordId });
            }
            catch (Exception)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Unable to update the medical record. Please check the entered details.");

                return View(record);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            bool deleted =
                await _medicalRecordRepository.DeleteAsync(id);

            if (!deleted)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> PatientRecords(int patientId)
        {
            IReadOnlyList<MedicalRecord> records =
                await _medicalRecordRepository.GetByPatientAsync(patientId);

            return View("Index", records);
        }
    }
}