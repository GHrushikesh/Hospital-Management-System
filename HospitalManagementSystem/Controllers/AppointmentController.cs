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
        private readonly PrescriptionRepository _prescriptionRepository;

        public AppointmentController(
            AppointmentRepository appointmentRepository,
            PatientRepository patientRepository,
            DoctorRepository doctorRepository,
            PrescriptionRepository prescriptionRepository)
        {
            _appointmentRepository = appointmentRepository;
            _patientRepository = patientRepository;
            _doctorRepository = doctorRepository;
            _prescriptionRepository = prescriptionRepository;
        }

        public async Task<IActionResult> Index()
        {
            var role = HttpContext.Session.GetString("LoggedInUserRole");
            bool isAdmin = HttpContext.Session.GetString("AdminAuthenticated") == "true" 
                || string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);

            if (!isAdmin)
            {
                if (string.Equals(role, "Doctor", StringComparison.OrdinalIgnoreCase))
                {
                    return RedirectToAction("Dashboard", "DoctorAccount");
                }
                if (string.Equals(role, "Receptionist", StringComparison.OrdinalIgnoreCase))
                {
                    return RedirectToAction("Dashboard", "Receptionist");
                }

                TempData["ErrorMessage"] = "The Master Appointment Ledger is restricted exclusively to Hospital Administration. Staff members manage clinical intake from their assigned duty desks.";
                return RedirectToAction("Dashboard", "Staff");
            }

            var appointments = await _appointmentRepository.GetHistoryAsync();
            return View(appointments);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (!IsAdminOrReceptionist())
            {
                TempData["ErrorMessage"] = "Access Denied: Appointment scheduling is restricted to Administration and Front Desk.";
                return RedirectToAction("Dashboard", "Staff");
            }

            ViewBag.Patients = await _patientRepository.GetAllAsync();
            ViewBag.Doctors = (await _doctorRepository.GetAllAsync()).Where(d => d.IsActive).ToList();
            return View(new Appointment { AppointmentDateTime = DateTime.Now.AddHours(2) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Appointment model)
        {
            if (!IsAdminOrReceptionist())
            {
                TempData["ErrorMessage"] = "Access Denied: Appointment scheduling is restricted to Administration and Front Desk.";
                return RedirectToAction("Dashboard", "Staff");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Patients = await _patientRepository.GetAllAsync();
                ViewBag.Doctors = (await _doctorRepository.GetAllAsync()).Where(d => d.IsActive).ToList();
                return View(model);
            }

            await _appointmentRepository.BookAsync(model);
            TempData["SuccessMessage"] = "Appointment scheduled successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (!IsAdminOrReceptionist())
            {
                TempData["ErrorMessage"] = "Access Denied: Appointment modification is restricted to Administration and Front Desk.";
                return RedirectToAction("Dashboard", "Staff");
            }

            var appointment = await _appointmentRepository.GetByIdAsync(id);
            if (appointment == null)
            {
                TempData["ErrorMessage"] = "Appointment not found.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Patients = await _patientRepository.GetAllAsync();
            ViewBag.Doctors = (await _doctorRepository.GetAllAsync()).Where(d => d.IsActive).ToList();
            return View(appointment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Appointment model)
        {
            if (!IsAdminOrReceptionist())
            {
                TempData["ErrorMessage"] = "Access Denied: Appointment modification is restricted to Administration and Front Desk.";
                return RedirectToAction("Dashboard", "Staff");
            }

            if (id != model.AppointmentId)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Patients = await _patientRepository.GetAllAsync();
                ViewBag.Doctors = (await _doctorRepository.GetAllAsync()).Where(d => d.IsActive).ToList();
                return View(model);
            }

            await _appointmentRepository.UpdateAsync(model);
            TempData["SuccessMessage"] = "Appointment updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var role = HttpContext.Session.GetString("LoggedInUserRole");
            bool isAdmin = HttpContext.Session.GetString("AdminAuthenticated") == "true" 
                || string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);
            bool isDoctor = string.Equals(role, "Doctor", StringComparison.OrdinalIgnoreCase);

            if (!isAdmin && !isDoctor)
            {
                TempData["ErrorMessage"] = "Access Denied: Staff members have no clinical or administrative authority to cancel or delete appointments. Only the consulting physician or hospital administrator can process appointment cancellations.";
                return RedirectToAction("Dashboard", "Staff");
            }

            if (isDoctor)
            {
                int doctorId = int.TryParse(HttpContext.Session.GetString("DoctorId"), out int dId) ? dId : 0;
                var appt = await _appointmentRepository.GetByIdAsync(id);
                if (appt == null || appt.DoctorId != doctorId)
                {
                    TempData["ErrorMessage"] = "Access Denied: Doctors may only cancel consultations scheduled under their own clinical desk.";
                    return RedirectToAction("Dashboard", "DoctorAccount");
                }
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

            if (isDoctor)
            {
                return RedirectToAction("Dashboard", "DoctorAccount");
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetDoctorAvailableSlots(int doctorId, string date)
        {
            if (doctorId <= 0 || !DateTime.TryParse(date, out DateTime parsedDate))
            {
                return BadRequest();
            }

            if (parsedDate.Date < DateTime.Today)
            {
                return Json(new { success = false, message = "Please select today or an upcoming clinical date." });
            }

            var doctor = await _doctorRepository.GetByIdAsync(doctorId);
            if (doctor == null || !doctor.IsActive)
            {
                return Json(new { success = false, message = "Consulting doctor is not currently active for appointments." });
            }

            var bookedSlots = await _appointmentRepository.GetBookedSlotsForDoctorDateAsync(doctorId, parsedDate);

            // Clinical OPD Schedule: 09:00 - 17:00 (30-min consultation slots, lunch break 13:00 - 14:00)
            var standardHours = new[]
            {
                "09:00", "09:30", "10:00", "10:30", "11:00", "11:30",
                "12:00", "12:30", "14:00", "14:30", "15:00", "15:30", "16:00", "16:30"
            };

            var slots = new List<object>();
            foreach (var h in standardHours)
            {
                var parts = h.Split(':');
                var slotDateTime = parsedDate.Date.AddHours(int.Parse(parts[0])).AddMinutes(int.Parse(parts[1]));

                bool isPast = slotDateTime <= DateTime.Now.AddMinutes(15);
                bool isBooked = bookedSlots.Any(b => Math.Abs((b - slotDateTime).TotalMinutes) < 25);

                slots.Add(new
                {
                    time = h,
                    display = DateTime.Today.Add(slotDateTime.TimeOfDay).ToString("hh:mm tt"),
                    slotDateTime = slotDateTime.ToString("yyyy-MM-ddTHH:mm"),
                    isAvailable = !isPast && !isBooked,
                    reason = isPast ? "Time Passed" : (isBooked ? "Reserved" : "Available")
                });
            }

            return Json(new
            {
                success = true,
                doctorName = doctor.FullName,
                roomNumber = string.IsNullOrWhiteSpace(doctor.RoomNumber) ? "OPD Room" : doctor.RoomNumber,
                fee = doctor.ConsultationFee,
                slots
            });
        }

        [HttpGet]
        public async Task<IActionResult> GuestRequest(bool submitted = false, bool isEmergency = false, int appointmentId = 0)
        {
            ViewBag.Submitted = submitted;
            ViewBag.IsEmergency = isEmergency;
            ViewBag.AppointmentId = appointmentId;
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

            if (model.AppointmentDateTime <= DateTime.Now.AddMinutes(-5))
            {
                ModelState.AddModelError(nameof(model.AppointmentDateTime), "Please choose an upcoming appointment time.");
                ViewBag.Doctors = doctors;
                return View("Request", model);
            }

            var selectedDoctor = doctors.FirstOrDefault(d => d.DoctorId == model.DoctorId);
            if (selectedDoctor == null)
            {
                ModelState.AddModelError(nameof(model.DoctorId), "Please select a registered, active consulting doctor.");
                ViewBag.Doctors = doctors;
                return View("Request", model);
            }

            if (model.IsEmergency && string.IsNullOrWhiteSpace(model.EmergencyDescription))
            {
                ModelState.AddModelError(nameof(model.EmergencyDescription), "For critical emergencies, please provide a brief description of symptoms to alert the doctor immediately.");
                ViewBag.Doctors = doctors;
                return View("Request", model);
            }

            // Time Slot Collision Prevention: Double-booking validation
            if (!model.IsEmergency && await _appointmentRepository.IsDoctorSlotBookedAsync(model.DoctorId, model.AppointmentDateTime))
            {
                ModelState.AddModelError(nameof(model.AppointmentDateTime), $"Dr. {selectedDoctor.FullName} already has a consultation booked at this time. Please select another available consultation slot.");
                ViewBag.Doctors = doctors;
                return View("Request", model);
            }

            // Identity resolution: Find existing patient by phone number or create a new unique MRN record
            int patientId = await _patientRepository.GetOrCreatePatientAsync(
                model.FirstName,
                model.LastName,
                model.Gender,
                model.PhoneNumber,
                model.Email);

            int newAppointmentId = await _appointmentRepository.BookAsync(new Appointment
            {
                PatientId = patientId,
                DoctorId = model.DoctorId,
                AppointmentDateTime = model.AppointmentDateTime,
                Reason = model.Reason.Trim(),
                Notes = string.IsNullOrWhiteSpace(model.Notes) ? null : model.Notes.Trim(),
                IsEmergency = model.IsEmergency,
                EmergencyDescription = model.IsEmergency ? model.EmergencyDescription?.Trim() : null,
                Status = model.IsEmergency ? "Emergency" : "Requested"
            });

            return RedirectToAction(nameof(GuestRequest), new
            {
                submitted = true,
                isEmergency = model.IsEmergency,
                appointmentId = newAppointmentId
            });
        }

        // --- DEDICATED 24/7 TRAUMA & ACUTE EMERGENCY DISPATCH PORTAL ---

        [HttpGet]
        public async Task<IActionResult> Emergency()
        {
            var doctors = (await _doctorRepository.GetAllAsync())
                .Where(d => d.IsActive)
                .ToList();

            ViewBag.Doctors = doctors;
            return View(new EmergencyRequestViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Emergency(EmergencyRequestViewModel model)
        {
            var doctors = (await _doctorRepository.GetAllAsync())
                .Where(d => d.IsActive)
                .ToList();

            if (!ModelState.IsValid)
            {
                ViewBag.Doctors = doctors;
                return View(model);
            }

            // 1. Resolve Attending Specialist (Selected or Auto-assigned by Emergency Category)
            Doctor? assignedDoctor = null;
            if (model.PreferredDoctorId.HasValue && model.PreferredDoctorId.Value > 0)
            {
                assignedDoctor = doctors.FirstOrDefault(d => d.DoctorId == model.PreferredDoctorId.Value);
            }

            if (assignedDoctor == null)
            {
                // Auto-route to specialty based on emergency type
                string category = model.EmergencyCategory.ToLowerInvariant();
                if (category.Contains("cardiac") || category.Contains("chest"))
                {
                    assignedDoctor = doctors.FirstOrDefault(d => d.Specialty.Contains("Cardiology"));
                }
                else if (category.Contains("stroke") || category.Contains("neuro"))
                {
                    assignedDoctor = doctors.FirstOrDefault(d => d.Specialty.Contains("Neurology"));
                }
                else if (category.Contains("trauma") || category.Contains("accident") || category.Contains("fracture"))
                {
                    assignedDoctor = doctors.FirstOrDefault(d => d.Specialty.Contains("Orthopedics"));
                }
                else if (category.Contains("pediatric") || category.Contains("child"))
                {
                    assignedDoctor = doctors.FirstOrDefault(d => d.Specialty.Contains("Pediatrics"));
                }

                // Default fallback to General Medicine or first available specialist
                assignedDoctor ??= doctors.FirstOrDefault(d => d.Specialty.Contains("General")) ?? doctors.First();
            }

            // 2. Compute Emergency Severity Index (ESI Triage Level)
            string esiLevel;
            if (model.ConsciousnessLevel.Contains("Unconscious") || 
                model.BreathingStatus.Contains("Stopped") || 
                model.BreathingStatus.Contains("Gasping"))
            {
                esiLevel = "Level 1 - Resuscitation (Immediate Life Threat)";
            }
            else if (model.EmergencyCategory.Contains("Cardiac") || 
                     model.EmergencyCategory.Contains("Trauma") || 
                     model.ConsciousnessLevel.Contains("Drowsy") || 
                     model.BreathingStatus.Contains("Severe"))
            {
                esiLevel = "Level 2 - Emergent (High Risk / Severe Distress)";
            }
            else
            {
                esiLevel = "Level 3 - Urgent (Immediate ER Attention Required)";
            }

            // 3. Identity Resolution (Resolve patient or register with unique MRN)
            int patientId = await _patientRepository.GetOrCreatePatientAsync(
                model.FirstName,
                model.LastName,
                model.Gender,
                model.PhoneNumber);

            // 4. Compile emergency payload
            string formattedEmergencyDesc = $"{model.EmergencyDescription.Trim()} | Vitals/Status: Consciousness=[{model.ConsciousnessLevel}], Breathing=[{model.BreathingStatus}], ETA=[{model.EstimatedArrival}], ReportedBy=[{model.CallerRelationship}{(string.IsNullOrWhiteSpace(model.CallerName) ? "" : $" - {model.CallerName}")}]";

            int appointmentId = await _appointmentRepository.BookAsync(new Appointment
            {
                PatientId = patientId,
                DoctorId = assignedDoctor.DoctorId,
                AppointmentDateTime = DateTime.Now,
                Reason = $"🚨 [EMERGENCY - {model.EmergencyCategory.ToUpperInvariant()}]",
                IsEmergency = true,
                EmergencyDescription = formattedEmergencyDesc,
                ESITriageLevel = esiLevel,
                Status = "Emergency",
                VitalsSummary = $"Breathing: {model.BreathingStatus} | Consciousness: {model.ConsciousnessLevel} | ETA: {model.EstimatedArrival}"
            });

            return RedirectToAction(nameof(EmergencyConfirmation), new { id = appointmentId });
        }

        [HttpGet]
        public async Task<IActionResult> EmergencyConfirmation(int id)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(id);
            if (appointment == null)
            {
                return RedirectToAction(nameof(Emergency));
            }

            return View(appointment);
        }

        [HttpGet]
        public IActionResult CheckStatus()
        {
            return View(new List<Appointment>());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckStatus(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                ModelState.AddModelError(string.Empty, "Please enter your registered phone number.");
                return View(new List<Appointment>());
            }

            ViewBag.PhoneNumber = phoneNumber.Trim();
            var appointments = await _appointmentRepository.GetAppointmentsByPhoneNumberAsync(phoneNumber.Trim());

            // Load historical prescribed medicines for the patient
            var patient = await _patientRepository.GetByPhoneNumberAsync(phoneNumber.Trim());
            if (patient != null)
            {
                var prescriptions = await _prescriptionRepository.GetByPatientIdAsync(patient.PatientId);
                ViewBag.Prescriptions = prescriptions;
            }
            else
            {
                ViewBag.Prescriptions = Array.Empty<Prescription>();
            }

            return View(appointments);
        }

        private bool IsAdminOrReceptionist()
        {
            var role = HttpContext.Session.GetString("LoggedInUserRole");
            return HttpContext.Session.GetString("AdminAuthenticated") == "true"
                || role == "Admin"
                || role == "Receptionist";
        }

        private bool IsStaffOrAdmin()
        {
            return HttpContext.Session.GetString("AdminAuthenticated") == "true"
                || !string.IsNullOrWhiteSpace(HttpContext.Session.GetString("LoggedInUserRole"));
        }
    }
}
