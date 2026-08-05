using HospitalManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace HospitalManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            // Security: Redirect unauthenticated users back to login page
            if (HttpContext.Session.GetString("AdminUser") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Redirect authenticated users straight to the new Dashboard Milestone
            return RedirectToAction("Index", "Dashboard");
        }

        public IActionResult Privacy()
        {
            if (HttpContext.Session.GetString("AdminUser") == null)
            {
                return RedirectToAction("Login", "Account");
            }
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
