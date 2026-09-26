using HospitalManagementSystem.Data.Repositories;
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
            return View(new Appointment());
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
            if (appointment is null)
            {
                return NotFound();
            }

            ViewBag.Patients = await _patientRepository.GetAllAsync();
            ViewBag.Doctors = await _doctorRepository.GetAllAsync();
            return View(appointment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Appointment model)
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

            await _appointmentRepository.UpdateAsync(model);
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

            await _appointmentRepository.CancelAsync(id);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GuestRequest(bool submitted = false)
        {
            ViewBag.Submitted = submitted;
            ViewBag.Doctors = (await _doctorRepository.GetAllAsync()).Where(doctor => doctor.IsActive).ToList();
            return View("Request", new GuestAppointmentViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuestRequest(GuestAppointmentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Doctors = (await _doctorRepository.GetAllAsync()).Where(doctor => doctor.IsActive).ToList();
                return View("Request", model);
            }

            var patientId = await _patientRepository.AddAsync(new Patient
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Gender = model.Gender,
                PhoneNumber = model.PhoneNumber,
                Email = model.Email
            });

            await _appointmentRepository.BookAsync(new Appointment
            {
                PatientId = patientId,
                DoctorId = model.DoctorId,
                AppointmentDateTime = model.AppointmentDateTime,
                Reason = model.Reason,
                Notes = model.Notes,
                Status = "Requested"
            });

            return RedirectToAction(nameof(GuestRequest), new { submitted = true });
        }

        private bool IsAuthenticated() => !string.IsNullOrWhiteSpace(HttpContext.Session.GetString("LoggedInUserRole"));
    }
}
