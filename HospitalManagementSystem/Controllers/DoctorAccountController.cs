using HospitalManagementSystem.Data.Repositories;
using HospitalManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementSystem.Controllers
{
    public class DoctorAccountController : Controller
    {
        private readonly AppointmentRepository _appointmentRepository;
        private readonly DoctorRepository _doctorRepository;
        private readonly PrescriptionRepository _prescriptionRepository;
        private readonly StaffRepository _staffRepository;

        public DoctorAccountController(
            AppointmentRepository appointmentRepository,
            DoctorRepository doctorRepository,
            PrescriptionRepository prescriptionRepository,
            StaffRepository staffRepository)
        {
            _appointmentRepository = appointmentRepository;
            _doctorRepository = doctorRepository;
            _prescriptionRepository = prescriptionRepository;
            _staffRepository = staffRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            if (HttpContext.Session.GetString("LoggedInUserRole") != "Doctor")
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action(nameof(Dashboard), "DoctorAccount") });
            }

            int doctorId = 0;
            if (int.TryParse(HttpContext.Session.GetString("DoctorId"), out int parsedId))
            {
                doctorId = parsedId;
            }

            var doctor = await _doctorRepository.GetByIdAsync(doctorId);
            var pending = await _appointmentRepository.GetPendingRequestsForDoctorAsync(doctorId);
            var emergencies = await _appointmentRepository.GetUnacknowledgedEmergenciesForDoctorAsync(doctorId);
            var allSchedule = await _appointmentRepository.GetScheduleForDoctorAsync(doctorId);
            var myStaff = await _staffRepository.GetStaffByDoctorIdAsync(doctorId);

            var todayAppointments = allSchedule
                .Where(a => a.AppointmentDateTime.Date == DateTime.Today.Date && a.Status != "Rejected" && a.Status != "Cancelled")
                .ToList();

            var viewModel = new DoctorDashboardViewModel
            {
                DoctorId = doctorId,
                FullName = doctor?.FullName ?? HttpContext.Session.GetString("LoggedInUserName") ?? "Doctor",
                Specialty = doctor?.Specialty ?? HttpContext.Session.GetString("DoctorSpecialty") ?? "Clinical Practice",
                Qualification = doctor?.Qualification ?? HttpContext.Session.GetString("DoctorQualification") ?? "MBBS, MD",
                RoomNumber = doctor?.RoomNumber ?? HttpContext.Session.GetString("DoctorRoomNumber") ?? "OPD Room",
                Username = doctor?.Username ?? HttpContext.Session.GetString("DoctorUsername") ?? string.Empty,
                HiredStaffCount = myStaff.Count,
                PendingRequests = pending,
                TodayAppointments = todayAppointments,
                EmergencyAlerts = emergencies,
                ScheduleHistory = allSchedule
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> GetEmergencyAlerts()
        {
            if (HttpContext.Session.GetString("LoggedInUserRole") != "Doctor")
            {
                return Unauthorized();
            }

            if (!int.TryParse(HttpContext.Session.GetString("DoctorId"), out int doctorId))
            {
                return BadRequest();
            }

            var emergencies = await _appointmentRepository.GetUnacknowledgedEmergenciesForDoctorAsync(doctorId);
            var result = emergencies.Select(e => new
            {
                appointmentId = e.AppointmentId,
                patientName = e.PatientName,
                phoneNumber = e.PatientPhoneNumber,
                emergencyDescription = e.EmergencyDescription,
                reason = e.Reason,
                time = e.AppointmentDateTime.ToString("HH:mm - dd MMM yyyy"),
                createdAt = e.CreatedAt.ToString("HH:mm:ss")
            });

            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetPatientPrescriptions(int patientId)
        {
            var role = HttpContext.Session.GetString("LoggedInUserRole");
            var adminAuth = HttpContext.Session.GetString("AdminAuthenticated");

            if (string.IsNullOrWhiteSpace(role) && adminAuth != "true")
            {
                return Unauthorized();
            }

            var history = await _prescriptionRepository.GetByPatientIdAsync(patientId);
            var result = history.Select(p => new
            {
                prescriptionId = p.PrescriptionId,
                medicineName = p.MedicineName,
                dosage = p.Dosage,
                frequency = p.Frequency,
                duration = p.Duration,
                instructions = string.IsNullOrWhiteSpace(p.Instructions) ? "As directed by physician" : p.Instructions,
                doctorName = p.DoctorName ?? "Attending Doctor",
                specialty = p.DoctorSpecialty ?? "Clinical Specialist",
                prescribedAt = p.PrescribedAt.ToString("dd MMM yyyy, HH:mm")
            });

            return Json(result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcknowledgeEmergency(int id)
        {
            if (HttpContext.Session.GetString("LoggedInUserRole") != "Doctor")
            {
                return RedirectToAction("Login", "Account");
            }

            int doctorId = 0;
            if (int.TryParse(HttpContext.Session.GetString("DoctorId"), out int parsedId))
            {
                doctorId = parsedId;
            }

            var appt = await _appointmentRepository.GetByIdAsync(id);
            if (appt == null || appt.DoctorId != doctorId)
            {
                TempData["ErrorMessage"] = "Unauthorized. You are only authorized to manage appointments assigned to your clinical roster.";
                return RedirectToAction(nameof(Dashboard));
            }

            await _appointmentRepository.AcknowledgeEmergencyAsync(id);
            TempData["SuccessMessage"] = $"Critical Emergency #{id} accepted and fast-tracked to consultation.";
            return RedirectToAction(nameof(Dashboard));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id, string? notes)
        {
            if (HttpContext.Session.GetString("LoggedInUserRole") != "Doctor")
            {
                return RedirectToAction("Login", "Account");
            }

            int doctorId = 0;
            if (int.TryParse(HttpContext.Session.GetString("DoctorId"), out int parsedId))
            {
                doctorId = parsedId;
            }

            var appt = await _appointmentRepository.GetByIdAsync(id);
            if (appt == null || appt.DoctorId != doctorId)
            {
                TempData["ErrorMessage"] = "Unauthorized. You are only authorized to approve appointments assigned to your clinical roster.";
                return RedirectToAction(nameof(Dashboard));
            }

            await _appointmentRepository.ApproveAppointmentAsync(id, notes);
            TempData["SuccessMessage"] = $"Appointment #{id} has been APPROVED for consultation.";
            return RedirectToAction(nameof(Dashboard));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, string? reason)
        {
            if (HttpContext.Session.GetString("LoggedInUserRole") != "Doctor")
            {
                return RedirectToAction("Login", "Account");
            }

            int doctorId = 0;
            if (int.TryParse(HttpContext.Session.GetString("DoctorId"), out int parsedId))
            {
                doctorId = parsedId;
            }

            var appt = await _appointmentRepository.GetByIdAsync(id);
            if (appt == null || appt.DoctorId != doctorId)
            {
                TempData["ErrorMessage"] = "Unauthorized. You are only authorized to manage your own appointments.";
                return RedirectToAction(nameof(Dashboard));
            }

            await _appointmentRepository.RejectAppointmentAsync(id, reason);
            TempData["ErrorMessage"] = $"Appointment #{id} has been DECLINED.";
            return RedirectToAction(nameof(Dashboard));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int id, string? doctorNotes, string? prescriptionItemsJson, string? medicineName, string? dosage, string? frequency, string? duration, string? instructions)
        {
            if (HttpContext.Session.GetString("LoggedInUserRole") != "Doctor")
            {
                return RedirectToAction("Login", "Account");
            }

            int doctorId = 0;
            if (int.TryParse(HttpContext.Session.GetString("DoctorId"), out int parsedId))
            {
                doctorId = parsedId;
            }

            var appt = await _appointmentRepository.GetByIdAsync(id);
            if (appt == null || appt.DoctorId != doctorId)
            {
                TempData["ErrorMessage"] = "Unauthorized. You cannot complete a consultation belonging to another physician.";
                return RedirectToAction(nameof(Dashboard));
            }

            await _appointmentRepository.CompleteAppointmentAsync(id, doctorNotes);

            int rxCount = 0;

            // 1. Process Multi-Medication Structured Rx Grid
            if (!string.IsNullOrWhiteSpace(prescriptionItemsJson))
            {
                try
                {
                    var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var items = System.Text.Json.JsonSerializer.Deserialize<List<PrescriptionItemInput>>(prescriptionItemsJson, options);
                    if (items != null)
                    {
                        foreach (var item in items.Where(i => !string.IsNullOrWhiteSpace(i.MedicineName)))
                        {
                            await _prescriptionRepository.AddAsync(new Prescription
                            {
                                PatientId = appt.PatientId,
                                DoctorId = doctorId,
                                AppointmentId = id,
                                MedicineName = item.MedicineName.Trim(),
                                Dosage = string.IsNullOrWhiteSpace(item.Dosage) ? "As directed" : item.Dosage.Trim(),
                                Frequency = string.IsNullOrWhiteSpace(item.Frequency) ? "1-0-1 (Twice Daily)" : item.Frequency.Trim(),
                                Duration = string.IsNullOrWhiteSpace(item.Duration) ? "5 Days" : item.Duration.Trim(),
                                Instructions = string.IsNullOrWhiteSpace(item.Instructions) ? "Take after meals" : item.Instructions.Trim()
                            });
                            rxCount++;
                        }
                    }
                }
                catch
                {
                    // Fall back to single item if JSON parse fails
                }
            }

            // 2. Legacy fallback for single medication inputs
            if (rxCount == 0 && !string.IsNullOrWhiteSpace(medicineName) && !string.IsNullOrWhiteSpace(dosage))
            {
                await _prescriptionRepository.AddAsync(new Prescription
                {
                    PatientId = appt.PatientId,
                    DoctorId = doctorId,
                    AppointmentId = id,
                    MedicineName = medicineName.Trim(),
                    Dosage = dosage.Trim(),
                    Frequency = string.IsNullOrWhiteSpace(frequency) ? "1-0-1 (Twice Daily)" : frequency.Trim(),
                    Duration = string.IsNullOrWhiteSpace(duration) ? "5 Days" : duration.Trim(),
                    Instructions = string.IsNullOrWhiteSpace(instructions) ? "Take after meals" : instructions.Trim()
                });
                rxCount++;
            }

            TempData["SuccessMessage"] = $"Consultation #{id} completed successfully. Diagnosis recorded and {rxCount} medication(s) prescribed.";
            return RedirectToAction(nameof(Dashboard));
        }

        // --- DOCTOR STAFF RECRUITMENT & MANAGEMENT ---

        [HttpGet]
        public async Task<IActionResult> HireStaff()
        {
            if (HttpContext.Session.GetString("LoggedInUserRole") != "Doctor")
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action(nameof(HireStaff), "DoctorAccount") });
            }

            int doctorId = 0;
            if (int.TryParse(HttpContext.Session.GetString("DoctorId"), out int parsedId))
            {
                doctorId = parsedId;
            }

            var doctor = await _doctorRepository.GetByIdAsync(doctorId);
            var model = new StaffUser
            {
                HiredByDoctorId = doctorId,
                HiredByDoctorName = doctor?.FullName ?? HttpContext.Session.GetString("LoggedInUserName") ?? "Attending Doctor",
                Department = doctor?.Specialty ?? "Clinical Unit",
                ShiftTiming = "Morning (07:00 - 15:00)"
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HireStaff(StaffUser model)
        {
            if (HttpContext.Session.GetString("LoggedInUserRole") != "Doctor")
            {
                return RedirectToAction("Login", "Account");
            }

            int doctorId = 0;
            if (int.TryParse(HttpContext.Session.GetString("DoctorId"), out int parsedId))
            {
                doctorId = parsedId;
            }

            var doctor = await _doctorRepository.GetByIdAsync(doctorId);
            model.HiredByDoctorId = doctorId;
            model.HiredByDoctorName = doctor?.FullName ?? HttpContext.Session.GetString("LoggedInUserName") ?? "Attending Doctor";

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _staffRepository.AddAsync(model);
                TempData["SuccessMessage"] = $"Staff member '{model.FullName}' ({model.Role}) has been successfully recruited & hired into your clinical team!";
                return RedirectToAction(nameof(MyStaff));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Could not complete staff hiring: " + ex.Message);
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> MyStaff()
        {
            if (HttpContext.Session.GetString("LoggedInUserRole") != "Doctor")
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action(nameof(MyStaff), "DoctorAccount") });
            }

            int doctorId = 0;
            if (int.TryParse(HttpContext.Session.GetString("DoctorId"), out int parsedId))
            {
                doctorId = parsedId;
            }

            var doctor = await _doctorRepository.GetByIdAsync(doctorId);
            var staffList = await _staffRepository.GetStaffByDoctorIdAsync(doctorId);

            ViewBag.DoctorName = doctor?.FullName ?? HttpContext.Session.GetString("LoggedInUserName") ?? "Attending Doctor";
            ViewBag.Specialty = doctor?.Specialty ?? HttpContext.Session.GetString("DoctorSpecialty") ?? "Clinical Department";

            return View(staffList);
        }
    }

    public class PrescriptionItemInput
    {
        public string MedicineName { get; set; } = string.Empty;
        public string Dosage { get; set; } = string.Empty;
        public string Frequency { get; set; } = string.Empty;
        public string Duration { get; set; } = string.Empty;
        public string? Instructions { get; set; }
    }
}
