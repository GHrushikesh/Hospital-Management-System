using HospitalManagementSystem.Data.Repositories;
using HospitalManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementSystem.Controllers
{
    public class ReceptionistController : Controller
    {
        private readonly AppointmentRepository _appointmentRepository;
        private readonly DoctorRepository _doctorRepository;
        private readonly PatientRepository _patientRepository;

        public ReceptionistController(
            AppointmentRepository appointmentRepository,
            DoctorRepository doctorRepository,
            PatientRepository patientRepository)
        {
            _appointmentRepository = appointmentRepository;
            _doctorRepository = doctorRepository;
            _patientRepository = patientRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var role = HttpContext.Session.GetString("LoggedInUserRole");
            var isAdmin = HttpContext.Session.GetString("AdminAuthenticated") == "true";

            if (!isAdmin && !string.Equals(role, "Receptionist", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Login", "Account");
            }

            var todayAppointments = await _appointmentRepository.GetTodayAppointmentsAsync();
            var doctors = (await _doctorRepository.GetAllAsync()).Where(d => d.IsActive).ToList();
            var recentPatients = await _patientRepository.GetAllAsync();

            var doctorRoster = new List<DoctorQueueStatusViewModel>();
            foreach (var doc in doctors)
            {
                var docAppts = todayAppointments.Where(a => a.DoctorId == doc.DoctorId).ToList();
                int nextToken = await _appointmentRepository.GetNextTokenNumberForDoctorAsync(doc.DoctorId, DateTime.Today);

                doctorRoster.Add(new DoctorQueueStatusViewModel
                {
                    DoctorId = doc.DoctorId,
                    FullName = doc.FullName,
                    Specialty = doc.Specialty,
                    Qualification = doc.Qualification,
                    RoomNumber = string.IsNullOrWhiteSpace(doc.RoomNumber) ? "OPD Desk" : doc.RoomNumber,
                    ConsultationFee = doc.ConsultationFee,
                    Availability = string.IsNullOrWhiteSpace(doc.Availability) ? "On Duty" : doc.Availability,
                    TotalBookedToday = docAppts.Count,
                    PatientsWaiting = docAppts.Count(a => a.Status == "Checked-In" || a.Status == "Approved"),
                    NextTokenNumber = nextToken
                });
            }

            var viewModel = new ReceptionistDashboardViewModel
            {
                FullName = HttpContext.Session.GetString("LoggedInUserName") ?? "Receptionist",
                CounterOrDesk = HttpContext.Session.GetString("StaffAssignedRoomOrWard") ?? "Main Reception Counter 01",
                ShiftTiming = HttpContext.Session.GetString("StaffShiftTiming") ?? "Day Shift (08:00 - 16:30)",
                Qualification = HttpContext.Session.GetString("StaffQualification") ?? "BBA Healthcare Administration",
                TodayTokensIssued = todayAppointments.Count(a => a.TokenNumber.HasValue),
                TodayWaitingInClinic = todayAppointments.Count(a => a.Status == "Checked-In" || a.Status == "Approved"),
                TodayCompletedVisits = todayAppointments.Count(a => a.Status == "Completed"),
                TodayFeesCollected = todayAppointments.Where(a => a.PaymentStatus.StartsWith("Paid")).Sum(a => a.PaymentAmount),
                ActiveDoctors = doctorRoster,
                TodayAppointments = todayAppointments,
                RecentPatients = recentPatients.Take(8).ToList()
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> WalkInBooking()
        {
            var role = HttpContext.Session.GetString("LoggedInUserRole");
            var isAdmin = HttpContext.Session.GetString("AdminAuthenticated") == "true";

            if (!isAdmin && !string.Equals(role, "Receptionist", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.Doctors = (await _doctorRepository.GetAllAsync()).Where(d => d.IsActive).ToList();
            return View(new WalkInBookingViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> WalkInBooking(WalkInBookingViewModel model)
        {
            var role = HttpContext.Session.GetString("LoggedInUserRole");
            var isAdmin = HttpContext.Session.GetString("AdminAuthenticated") == "true";

            if (!isAdmin && !string.Equals(role, "Receptionist", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Login", "Account");
            }

            var doctors = (await _doctorRepository.GetAllAsync()).Where(d => d.IsActive).ToList();
            var doctor = doctors.FirstOrDefault(d => d.DoctorId == model.DoctorId);
            if (doctor == null)
            {
                ModelState.AddModelError(nameof(model.DoctorId), "Please select an active doctor.");
                ViewBag.Doctors = doctors;
                return View(model);
            }

            if (string.IsNullOrWhiteSpace(model.FirstName) || string.IsNullOrWhiteSpace(model.LastName) || string.IsNullOrWhiteSpace(model.PhoneNumber))
            {
                ModelState.AddModelError(string.Empty, "Patient First Name, Last Name, and Mobile Number are required.");
                ViewBag.Doctors = doctors;
                return View(model);
            }

            // Identity resolution: link existing patient or register new
            int patientId = await _patientRepository.GetOrCreatePatientAsync(
                model.FirstName,
                model.LastName,
                string.IsNullOrWhiteSpace(model.Gender) ? "Not Specified" : model.Gender,
                model.PhoneNumber,
                model.Email);

            // Compute next sequential token for this doctor today
            int tokenNumber = await _appointmentRepository.GetNextTokenNumberForDoctorAsync(doctor.DoctorId, DateTime.Today);

            int newApptId = await _appointmentRepository.BookAsync(new Appointment
            {
                PatientId = patientId,
                DoctorId = doctor.DoctorId,
                AppointmentDateTime = DateTime.Now,
                Reason = string.IsNullOrWhiteSpace(model.Reason) ? "Walk-In Consultation" : model.Reason.Trim(),
                Status = "Checked-In",
                IsEmergency = model.IsEmergency,
                EmergencyDescription = model.IsEmergency ? model.EmergencyDescription : null,
                TokenNumber = tokenNumber,
                PaymentStatus = "Paid",
                PaymentAmount = doctor.ConsultationFee,
                PaymentMethod = model.PaymentMethod
            });

            TempData["SuccessMessage"] = $"Walk-in patient registered. Assigned TOKEN #{tokenNumber} for Dr. {doctor.FullName} in {doctor.RoomNumber}. Fee collected: {doctor.ConsultationFee:C}";
            return RedirectToAction(nameof(PrintSlip), new { id = newApptId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IssueToken(int appointmentId, decimal feeAmount, string paymentMethod)
        {
            var role = HttpContext.Session.GetString("LoggedInUserRole");
            var isAdmin = HttpContext.Session.GetString("AdminAuthenticated") == "true";

            if (!isAdmin && !string.Equals(role, "Receptionist", StringComparison.OrdinalIgnoreCase))
            {
                return Unauthorized();
            }

            var appt = await _appointmentRepository.GetByIdAsync(appointmentId);
            if (appt == null)
            {
                TempData["ErrorMessage"] = "Appointment not found.";
                return RedirectToAction(nameof(Dashboard));
            }

            int tokenNumber = await _appointmentRepository.GetNextTokenNumberForDoctorAsync(appt.DoctorId, DateTime.Today);
            await _appointmentRepository.IssueTokenAndCollectFeeAsync(appointmentId, tokenNumber, feeAmount, paymentMethod);

            TempData["SuccessMessage"] = $"Token #{tokenNumber} issued to {appt.PatientName}. Payment of {feeAmount:C} ({paymentMethod}) recorded.";
            return RedirectToAction(nameof(PrintSlip), new { id = appointmentId });
        }

        [HttpGet]
        public async Task<IActionResult> PrintSlip(int id)
        {
            var appt = await _appointmentRepository.GetByIdAsync(id);
            if (appt == null)
            {
                return NotFound();
            }

            return View(appt);
        }

        [HttpGet]
        public async Task<IActionResult> PatientLocator(string? searchTerm)
        {
            var role = HttpContext.Session.GetString("LoggedInUserRole");
            var isAdmin = HttpContext.Session.GetString("AdminAuthenticated") == "true";

            if (!isAdmin && !string.Equals(role, "Receptionist", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Login", "Account");
            }

            IReadOnlyList<Appointment> results = Array.Empty<Appointment>();
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var all = await _appointmentRepository.GetHistoryAsync();
                results = all.Where(a =>
                    (a.PatientName != null && a.PatientName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                    (a.PatientPhoneNumber != null && a.PatientPhoneNumber.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                    (a.PatientMRN != null && a.PatientMRN.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                ).Take(15).ToList();

                ViewBag.SearchTerm = searchTerm;
            }

            return View(results);
        }
    }
}
