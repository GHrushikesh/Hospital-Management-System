using HospitalManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementSystem.Controllers
{
    public class DoctorAccountController : Controller
    {
        [HttpGet]
        public IActionResult Dashboard()
        {
            if (HttpContext.Session.GetString("LoggedInUserRole") != "Doctor")
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action(nameof(Dashboard), "DoctorAccount") });
            }

            return View(new DoctorDashboardViewModel
            {
                FullName = HttpContext.Session.GetString("LoggedInUserName") ?? "Doctor",
                Specialty = HttpContext.Session.GetString("DoctorSpecialty") ?? "Medical practice",
                Username = HttpContext.Session.GetString("DoctorUsername") ?? string.Empty
            });
        }

    }
}
