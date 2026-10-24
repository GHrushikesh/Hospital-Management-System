using HospitalManagementSystem.Data.Repositories;
using HospitalManagementSystem.Filters;
using HospitalManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementSystem.Controllers
{
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
            if (!IsAuthenticated())
            {
                return RedirectToAction(nameof(GuestRequest));
            }

            var appointments = await _appointmentRepository.GetHistoryAsync();
            return View(appointments);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction(nameof(GuestRequest));
            }

            ViewBag.Patients = await _patientRepository.GetAllAsync();
            ViewBag.Doctors = await _doctorRepository.GetAllAsync();
            return View(new Appointment { AppointmentDateTime = DateTime.Now.AddHours(1) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Appointment model)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction(nameof(GuestRequest));
            }

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
            if (!IsAuthenticated())
            {
                return RedirectToAction(nameof(GuestRequest));
            }

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
            if (!IsAuthenticated())
            {
                return RedirectToAction(nameof(GuestRequest));
            }

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
            if (!IsAuthenticated())
            {
                return RedirectToAction(nameof(GuestRequest));
            }

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

        [HttpGet]
        public async Task<IActionResult> GuestRequest(bool submitted = false)
        {
            ViewBag.Submitted = submitted;
            ViewBag.Doctors = (await _doctorRepository.GetAllAsync())
                .Where(doctor => doctor.IsActive)
                .ToList();
            return View("Request", new GuestAppointmentViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuestRequest(GuestAppointmentViewModel model)
        {
            var doctors = (await _doctorRepository.GetAllAsync())
                .Where(doctor => doctor.IsActive)
                .ToList();

            if (!ModelState.IsValid)
            {
                ViewBag.Doctors = doctors;
                return View("Request", model);
            }

            if (model.AppointmentDateTime <= DateTime.Now)
            {
                ModelState.AddModelError(nameof(model.AppointmentDateTime), "Choose a future appointment time.");
                ViewBag.Doctors = doctors;
                return View("Request", model);
            }

            if (!doctors.Any(doctor => doctor.DoctorId == model.DoctorId))
            {
                ModelState.AddModelError(nameof(model.DoctorId), "Choose an available doctor.");
                ViewBag.Doctors = doctors;
                return View("Request", model);
            }

            var patientId = await _patientRepository.AddAsync(new Patient
            {
                FirstName = model.FirstName.Trim(),
                LastName = model.LastName.Trim(),
                Gender = model.Gender.Trim(),
                PhoneNumber = model.PhoneNumber.Trim(),
                Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim()
            });

            await _appointmentRepository.BookAsync(new Appointment
            {
                PatientId = patientId,
                DoctorId = model.DoctorId,
                AppointmentDateTime = model.AppointmentDateTime,
                Reason = model.Reason.Trim(),
                Notes = string.IsNullOrWhiteSpace(model.Notes) ? null : model.Notes.Trim(),
                Status = "Requested"
            });

            return RedirectToAction(nameof(GuestRequest), new { submitted = true });
        }

        private bool IsAuthenticated()
        {
            return HttpContext.Session.GetString("AdminAuthenticated") == "true"
                || !string.IsNullOrWhiteSpace(HttpContext.Session.GetString("LoggedInUserRole"));
        }
    }
}
