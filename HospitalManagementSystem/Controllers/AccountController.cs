using Microsoft.AspNetCore.Mvc;
using HospitalManagementSystem.ViewModels;
using HospitalManagementSystem.Repositories;

namespace HospitalManagementSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly AuthRepository _authRepository;

        // Injecting the repository via constructor
        public AccountController(AuthRepository authRepository)
        {
            _authRepository = authRepository;
        }

        // 1. HTTP GET: Displays the login page
        [HttpGet]
        public IActionResult Login()
        {
            // If user is already logged in, redirect them to the Home Page
            if (HttpContext.Session.GetString("AdminUser") != null)
            {
                return RedirectToAction("Index", "Dashboard");
            }
            return View();
        }

        // 2. HTTP POST: Handles the submitted login form safely
        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Verify credentials using our repository (which uses parameterised ADO.NET)
                bool isValid = _authRepository.ValidateAdminUser(model.Username, model.Password);

                if (isValid)
                {
                    // Create secure Session variable
                    HttpContext.Session.SetString("AdminUser", model.Username);

                    // Redirect to Dashboard Controller
                    return RedirectToAction("Index", "Dashboard");
                }
                else
                {
                    // Add friendly error message if database match fails
                    ModelState.AddModelError(string.Empty, "Invalid Username or Password.");
                }
            }

            // If we got this far, something failed, redisplay form with errors
            return View(model);
        }

        // 3. Logout Action
        [HttpGet]
        public IActionResult Logout()
        {
            // Clear all session data securely
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }
    }
}
