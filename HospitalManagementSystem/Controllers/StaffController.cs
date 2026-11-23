using HospitalManagementSystem.Data.Repositories;
using HospitalManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementSystem.Controllers
{
    public class StaffController : Controller
    {
        private readonly StaffRepository _staffRepository;
        private readonly AppointmentRepository _appointmentRepository;
        private readonly PatientRepository _patientRepository;
        private readonly BedRepository _bedRepository;
        private readonly DoctorRepository _doctorRepository;
        private readonly PrescriptionRepository _prescriptionRepository;

        public StaffController(
            StaffRepository staffRepository,
            AppointmentRepository appointmentRepository,
            PatientRepository patientRepository,
            BedRepository bedRepository,
            DoctorRepository doctorRepository,
            PrescriptionRepository prescriptionRepository)
        {
            _staffRepository = staffRepository;
            _appointmentRepository = appointmentRepository;
            _patientRepository = patientRepository;
            _bedRepository = bedRepository;
            _doctorRepository = doctorRepository;
            _prescriptionRepository = prescriptionRepository;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (!string.IsNullOrWhiteSpace(HttpContext.Session.GetString("LoggedInUserRole")))
            {
                return RedirectToAction(nameof(Dashboard));
            }

            return View(new StaffLoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(StaffLoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _staffRepository.AuthenticateAsync(model.Username, model.Password);
            if (user is null)
            {
                ModelState.AddModelError(string.Empty, "Invalid staff credentials.");
                return View(model);
            }

            HttpContext.Session.Clear();
            HttpContext.Session.SetString("LoggedInUserRole", user.Role);
            HttpContext.Session.SetString("LoggedInUserName", user.FullName);
            HttpContext.Session.SetString("StaffDepartment", user.Department);
            HttpContext.Session.SetString("StaffQualification", user.Qualification);
            HttpContext.Session.SetString("StaffAssignedRoomOrWard", user.AssignedRoomOrWard);
            HttpContext.Session.SetString("StaffShiftTiming", user.ShiftTiming);
            HttpContext.Session.SetString("StaffHiredByDoctorName", user.HiredByDoctorName ?? string.Empty);
            HttpContext.Session.SetString("StaffAuthenticated", "true");

            return RedirectToAction(nameof(Dashboard));
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var role = HttpContext.Session.GetString("LoggedInUserRole");
            if (string.IsNullOrWhiteSpace(role) || role == "Doctor")
            {
                return RedirectToAction(nameof(Login));
            }

            var tasks = role.ToLowerInvariant() switch
            {
                "receptionist" => new[] { "Verify returning patients by Phone or MRN to prevent duplicates.", "Review new patient appointment requests & guide walk-ins.", "Confirm appointment check-ins and print OPD arrival slips." },
                "nurse" => new[] { "Record triage vitals (Blood Pressure, Pulse, Temp, SpO2) upon patient arrival.", "Monitor assigned room and ward beds.", "Assist physicians and flag critical emergency cases." },
                "doctor assistant" => new[] { "Prepare daily clinic queue for attending doctor.", "Coordinate clinical diagnostics and follow-up schedules.", "Ensure patient files are loaded before doctor consultation." },
                _ => new[] { "Review assigned room/ward roster and shift timings.", "Maintain accurate clinical operations log.", "Escalate immediate issues to the hospital administrator." }
            };

            var todayAppointments = await _appointmentRepository.GetTodayAppointmentsAsync();
            var recentPatients = await _patientRepository.GetAllAsync();
            var bedStats = await _bedRepository.GetOccupancySummaryAsync();
            var allBeds = await _bedRepository.GetAllAsync();
            var occupiedBeds = allBeds.Where(b => b.Status == "Occupied").ToList();
            var recentRx = await _prescriptionRepository.GetRecentPrescriptionsAsync(8);

            return View(new StaffDashboardViewModel
            {
                FullName = HttpContext.Session.GetString("LoggedInUserName") ?? "Staff Member",
                Role = role,
                Department = HttpContext.Session.GetString("StaffDepartment") ?? "Clinical Operations",
                Qualification = HttpContext.Session.GetString("StaffQualification") ?? "Healthcare Professional",
                AssignedRoomOrWard = HttpContext.Session.GetString("StaffAssignedRoomOrWard") ?? "General Outpatient Desk",
                ShiftTiming = HttpContext.Session.GetString("StaffShiftTiming") ?? "Scheduled Duty Hours",
                HiredByDoctorName = HttpContext.Session.GetString("StaffHiredByDoctorName"),
                TotalBeds = bedStats.Total,
                OccupiedBeds = bedStats.Occupied,
                VacantBeds = bedStats.Vacant,
                TodayAppointments = todayAppointments,
                RecentPatients = recentPatients.Take(10).ToList(),
                OccupiedBedsList = occupiedBeds,
                ActivePrescriptionOrders = recentRx,
                Tasks = tasks
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckInWithVitals(int appointmentId, string bp, string pulse, string temp, string? spo2, string? respRate)
        {
            var role = HttpContext.Session.GetString("LoggedInUserRole");
            if (string.IsNullOrWhiteSpace(role))
            {
                return RedirectToAction(nameof(Login));
            }

            string vitals = $"BP: {bp} mmHg | Pulse: {pulse} bpm | Temp: {temp}°F" 
                + (string.IsNullOrWhiteSpace(spo2) ? "" : $" | SpO2: {spo2}%")
                + (string.IsNullOrWhiteSpace(respRate) ? "" : $" | RR: {respRate}/min");

            await _appointmentRepository.UpdateVitalsAndCheckInAsync(appointmentId, vitals);

            TempData["SuccessMessage"] = $"Patient checked in successfully. Triage vitals recorded: [{vitals}]";
            return RedirectToAction(nameof(Dashboard));
        }

        // --- WARD & BED OCCUPANCY MANAGEMENT ---

        [HttpGet]
        public async Task<IActionResult> WardOccupancy()
        {
            var role = HttpContext.Session.GetString("LoggedInUserRole");
            var adminAuth = HttpContext.Session.GetString("AdminAuthenticated");

            if (string.IsNullOrWhiteSpace(role) && adminAuth != "true")
            {
                return RedirectToAction(nameof(Login));
            }

            var beds = await _bedRepository.GetAllAsync();
            var patients = await _patientRepository.GetAllAsync();
            var doctors = (await _doctorRepository.GetAllAsync()).Where(d => d.IsActive).ToList();
            var summary = await _bedRepository.GetOccupancySummaryAsync();
            var history = await _bedRepository.GetAdmissionHistoryAsync(30);

            var viewModel = new WardOccupancyViewModel
            {
                Beds = beds,
                Patients = patients,
                Doctors = doctors,
                AdmissionHistory = history,
                TotalBeds = summary.Total,
                OccupiedBeds = summary.Occupied,
                VacantBeds = summary.Vacant
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdmitPatient(int bedId, int patientId, int doctorId, string? admissionNotes)
        {
            var role = HttpContext.Session.GetString("LoggedInUserRole");
            var adminAuth = HttpContext.Session.GetString("AdminAuthenticated");

            if (string.IsNullOrWhiteSpace(role) && adminAuth != "true")
            {
                return RedirectToAction(nameof(Login));
            }

            // Concurrency & Medical Safety Check: Is this patient already admitted to another bed?
            var activeBed = await _bedRepository.GetActiveBedByPatientIdAsync(patientId);
            if (activeBed != null)
            {
                TempData["ErrorMessage"] = $"Admission Denied: This patient is already admitted to Bed {activeBed.BedNumber} ({activeBed.WardName}). A patient cannot occupy multiple beds simultaneously.";
                return RedirectToAction(nameof(WardOccupancy));
            }

            var success = await _bedRepository.AdmitPatientAsync(bedId, patientId, doctorId, admissionNotes ?? "Inpatient observation");
            if (success)
            {
                TempData["SuccessMessage"] = "Patient admitted to bed successfully. Real-time ward occupancy & permanent admission log updated.";
            }
            else
            {
                TempData["ErrorMessage"] = "Could not admit patient. Bed may already be occupied or unavailable.";
            }

            return RedirectToAction(nameof(WardOccupancy));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DischargePatient(int bedId, string? dischargeSummary)
        {
            var role = HttpContext.Session.GetString("LoggedInUserRole");
            var adminAuth = HttpContext.Session.GetString("AdminAuthenticated");

            if (string.IsNullOrWhiteSpace(role) && adminAuth != "true")
            {
                return RedirectToAction(nameof(Login));
            }

            string staffName = HttpContext.Session.GetString("LoggedInUserName") ?? "Ward Charge Nurse";
            var success = await _bedRepository.DischargePatientAsync(bedId, dischargeSummary, staffName);
            if (success)
            {
                TempData["SuccessMessage"] = $"Patient discharged. Inpatient admission archived and bed returned to Vacant inventory.";
            }
            else
            {
                TempData["ErrorMessage"] = "Could not process bed discharge.";
            }

            return RedirectToAction(nameof(WardOccupancy));
        }

        // --- CENTRAL EMERGENCY ROOM & TRIAGE WING COCKPIT ---

        [HttpGet]
        public async Task<IActionResult> EmergencyQueue()
        {
            var role = HttpContext.Session.GetString("LoggedInUserRole");
            var adminAuth = HttpContext.Session.GetString("AdminAuthenticated");

            if (string.IsNullOrWhiteSpace(role) && adminAuth != "true")
            {
                return RedirectToAction(nameof(Login));
            }

            var emergencies = await _appointmentRepository.GetAllActiveEmergenciesAsync();
            return View(emergencies);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TriageEmergency(int appointmentId, string esiLevel, string? triageNotes, string? vitals)
        {
            var role = HttpContext.Session.GetString("LoggedInUserRole");
            var adminAuth = HttpContext.Session.GetString("AdminAuthenticated");

            if (string.IsNullOrWhiteSpace(role) && adminAuth != "true")
            {
                return RedirectToAction(nameof(Login));
            }

            await _appointmentRepository.TriageEmergencyAsync(appointmentId, esiLevel, triageNotes, vitals);
            TempData["SuccessMessage"] = $"Emergency #{appointmentId} triaged to '{esiLevel}'. Clinical alert updated across duty desks.";
            return RedirectToAction(nameof(EmergencyQueue));
        }

        [HttpGet]
        public async Task<IActionResult> GetCentralEmergencies()
        {
            var emergencies = await _appointmentRepository.GetAllActiveEmergenciesAsync();
            var result = emergencies.Select(e => new
            {
                appointmentId = e.AppointmentId,
                patientName = e.PatientName,
                phoneNumber = e.PatientPhoneNumber,
                emergencyDescription = e.EmergencyDescription,
                reason = e.Reason,
                esiLevel = e.ESITriageLevel ?? "Level 2 - Emergent",
                doctorName = e.DoctorName,
                specialty = e.DoctorSpecialty,
                time = e.AppointmentDateTime.ToString("HH:mm - dd MMM yyyy"),
                createdAt = e.CreatedAt.ToString("HH:mm:ss")
            });

            return Json(result);
        }

        [HttpGet]
        public IActionResult Register()
        {
            TempData["InfoMessage"] = "Per Hospital Policy, staff members are hired directly by Doctors and Clinical Leads from their Doctor Clinical Desk.";
            return RedirectToAction(nameof(Login));
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}
