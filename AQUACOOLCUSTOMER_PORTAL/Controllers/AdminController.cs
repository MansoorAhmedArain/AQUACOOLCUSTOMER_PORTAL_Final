using Microsoft.AspNetCore.Mvc;
using ServiceReference1;

namespace AQUACOOLCUSTOMER_PORTAL.Controllers
{
    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> _logger;
        private ServiceReference1.Service1SoapClient _service;
        public AdminController(ILogger<AdminController> logger)
        {
            _logger = logger;
            _service = new Service1SoapClient(Service1SoapClient.EndpointConfiguration.Service1Soap);
        }
        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var username = HttpContext.Session.GetString("UserName");
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(userId))
            {
                return RedirectPermanent("/Home/Index");
            }
            return View();
        }
        public async Task<IActionResult> MoveInRequest()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var username = HttpContext.Session.GetString("UserName");
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(userId))
            {
                return RedirectPermanent("/Home/Index");
            }
            Projects[] projects = await LoadProjectSelection();
            ViewBag.Projects = projects;
           // var units = LoadUnitSelection("test");
            ViewBag.Projects = projects;
            return View(projects);
        }

        private async Task<string> LoadUnitSelection(string propertyId)
        {
            var units = await _service.GetUnitDLAsync(propertyId);
            return units;
        }

        /// <summary>
        /// Get the list of the projects from ax
        /// </summary>
        /// <returns></returns>
        private async Task<Projects[]>  LoadProjectSelection()
        {
           var projects =  await _service.GetProjectsAsync();
            return projects;
        }

        public async Task<IActionResult> MoveOutRequest()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var username = HttpContext.Session.GetString("UserName");
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(userId))
            {
                return RedirectPermanent("/Home/Index");
            }
            Projects[] projects = await LoadProjectSelection();
            ViewBag.Projects = projects;
            // var units = LoadUnitSelection("test");
            ViewBag.Projects = projects;
            return View(projects);
        }
        public async Task<IActionResult> ReconnectionRequest()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var username = HttpContext.Session.GetString("UserName");
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(userId))
            {
                return RedirectPermanent("/Home/Index");
            }
            var projects = await LoadProjectSelection();
            ViewBag.Projects = projects;
            // var units = LoadUnitSelection("test");
            ViewBag.Projects = projects;
            return View(projects);

        }
        public IActionResult StatementofAccount()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var username = HttpContext.Session.GetString("UserName");
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(userId))
            {
                return RedirectPermanent("/Home/Index");
            }
            return View();
        }

        /// <summary>
        /// Logout function to remove all the session from the browser. 
        /// </summary>
        /// <returns></returns>
        public IActionResult Logout()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var username = HttpContext.Session.GetString("UserName");
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(userId))
            {
                return RedirectToAction("index", "Home");
            }
            HttpContext.Session.Remove("UserId");
            HttpContext.Session.Remove("UserName");
            HttpContext.Session.Clear();
            return RedirectToAction("index", "Home");
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var username = HttpContext.Session.GetString("UserName");
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(userId))
            {
                return RedirectToAction("index","Home");
            }
            ViewBag.ErrorMessage = "";
            return View();
        }

        [HttpPost]
        public IActionResult ChangePassword(FormCollection form)
        {
            var userId = HttpContext.Session.GetString("UserId");
            var username = HttpContext.Session.GetString("UserName");
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(userId))
            {
                return RedirectToAction("index", "Home");
            }

            if (form == null)
            {
                ViewBag.ErrorMessage = "Unable to change your password.";
                return View();
            }
            var oldPass = form["OldPassword"].ToString();
            var newPass = form["NewPassword"].ToString();
            var confirmPass = form["ConfirmPassword"].ToString();
            if (string.IsNullOrWhiteSpace(oldPass) || string.IsNullOrWhiteSpace(newPass) || string.IsNullOrWhiteSpace(confirmPass))
            {
                ViewBag.ErrorMessage = "Please fill all the required fields.";
                return View();
            }
            if (newPass != confirmPass)
            {
                ViewBag.ErrorMessage = "Password and confirm password does not match.";
                return View();
            }
            var response = _service.ChangePasswordAsync(userId, username, newPass).Result;
            if (response == "Success")
            {
                // Password has been reset.
            }
            return View();
        }
    }
}
