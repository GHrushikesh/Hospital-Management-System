using HospitalManagementSystem.Data.Repositories;
using HospitalManagementSystem.Filters;
using HospitalManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementSystem.Controllers
{
    [AdminAuthorize]
    public class AppointmentController : Controller
    {
        private readonly AppointmentRepository _appointmentRepository;
        private readonly PatientRepository _patientRepository;
        private readonly DoctorRepository _doctorRepository;

        public AppointmentController(AppointmentRepository appointmentRepository, PatientRepository patientRepository, DoctorRepository doctorRepository)
        {
            _appointmentRepository = appointmentRepository;
            _patientRepository = patientRepository;
            _doctorRepository = doctorRepository;
        }

        public async Task<IActionResult> Index()
        {
            var appointments = await _appointmentRepository.GetHistoryAsync();
            return View(appointments);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Patients = await _patientRepository.GetAllAsync();
            ViewBag.Doctors = await _doctorRepository.GetAllAsync();
            return View(new Appointment { AppointmentDateTime = DateTime.Now.AddHours(1) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Appointment model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Patients = await _patientRepository.GetAllAsync();
                ViewBag.Doctors = await _doctorRepository.GetAllAsync();
                return View(model);
            }

            await _appointmentRepository.BookAsync(model);
            TempData["SuccessMessage"] = "Appointment scheduled successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(id);
            if (appointment == null)
            {
                TempData["ErrorMessage"] = "Appointment not found.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Patients = await _patientRepository.GetAllAsync();
            ViewBag.Doctors = await _doctorRepository.GetAllAsync();
            return View(appointment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Appointment model)
        {
            if (id != model.AppointmentId)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Patients = await _patientRepository.GetAllAsync();
                ViewBag.Doctors = await _doctorRepository.GetAllAsync();
                return View(model);
            }

            var updated = await _appointmentRepository.UpdateAsync(model);
            if (!updated)
            {
                ModelState.AddModelError(string.Empty, "Unable to save changes. The appointment may no longer exist.");
                ViewBag.Patients = await _patientRepository.GetAllAsync();
                ViewBag.Doctors = await _doctorRepository.GetAllAsync();
                return View(model);
            }

            TempData["SuccessMessage"] = "Appointment details updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(id);
            if (appointment == null)
            {
                TempData["ErrorMessage"] = "Appointment not found.";
                return RedirectToAction(nameof(Index));
            }

            if (string.Equals(appointment.Status, "Cancelled", StringComparison.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] = "This appointment is already cancelled.";
                return RedirectToAction(nameof(Index));
            }

            var cancelled = await _appointmentRepository.CancelAsync(id);
            if (cancelled)
            {
                TempData["SuccessMessage"] = $"Appointment #{id} has been cancelled successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to cancel the appointment.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
