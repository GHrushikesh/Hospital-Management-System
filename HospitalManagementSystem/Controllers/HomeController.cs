using HospitalManagementSystem.Data.Repositories;
using HospitalManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace HospitalManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly PatientRepository _patientRepository;
        private readonly DoctorRepository _doctorRepository;
        private readonly AppointmentRepository _appointmentRepository;

        public HomeController(PatientRepository patientRepository, DoctorRepository doctorRepository, AppointmentRepository appointmentRepository)
        {
            _patientRepository = patientRepository;
            _doctorRepository = doctorRepository;
            _appointmentRepository = appointmentRepository;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new DashboardViewModel();

            try
            {
                viewModel.PatientCount = (await _patientRepository.GetAllAsync()).Count;
                viewModel.DoctorCount = (await _doctorRepository.GetAllAsync()).Count;
                viewModel.AppointmentCount = (await _appointmentRepository.GetHistoryAsync()).Count;
                viewModel.TodayAppointments = await _appointmentRepository.GetTodayAppointmentsAsync();
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
            }

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
