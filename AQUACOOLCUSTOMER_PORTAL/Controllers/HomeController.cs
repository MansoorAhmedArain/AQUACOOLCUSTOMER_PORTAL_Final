using AQUACOOLCUSTOMER_PORTAL.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using ServiceReference1;
using AQUACOOLCUSTOMER_PORTAL.DTO;
using System.Drawing.Drawing2D;
namespace AQUACOOLCUSTOMER_PORTAL.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private ServiceReference1.Service1SoapClient _service;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
            _service = new Service1SoapClient(Service1SoapClient.EndpointConfiguration.Service1Soap);
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Login()
        {
            ViewBag.ErrorMessage = "";
            return View();
        }

        /// <summary>
        /// This function will be utilize for posting of the login
        /// form to the server.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    ViewBag.ErroMessage = "username or password can not be empty.";
                    return View();
                }
                var response = _service.LoginAsync("test", "test").Result;  //Success|C003776|test
                if (!String.IsNullOrWhiteSpace(response) || response.Contains("Success"))
                {
                    var resp = response.Split("|");
                    HttpContext.Session.SetString("UserId", resp[1]);
                    HttpContext.Session.SetString("UserName", username);
                    return RedirectToAction("Index", "Admin");
                }
                ViewBag.ErrorMessage = "Not Authorized. Please check your Credentials";
            }
            catch (Exception ex)
            {

                ViewBag.ErroMessage = "Internal Server error unable to fetch your details.";
            }
            return View();
        }
        public IActionResult Registration()
        {
            return View();
        }

        /// <summary>
        /// Get the registration type parameter from the first form
        /// and travese it to next form as hidden fields.
        /// </summary>
        /// <param name="regType"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult AccountInfo(string regType = "")
        {
            //if (regType == "")
            //{
            //    return RedirectToAction("Registration");
            //}
            if (regType.ToLower() == "company")
            {
                return RedirectToAction("AccountInfo1", "Home", new { _regType = "Company" });
            }
            ViewData["regType"] = regType;
            return View();
        }

        /// <summary>
        /// Get the form collection and trverse it to the third step of the
        /// form name as account info1 with hidden fields.
        /// </summary>
        /// <param name="frm"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult AccountInfo(AccountInfo accountInfo)
        {
            if (accountInfo == null)
            {
                ViewData["regType"] = "";
                return View();
            }
            return RedirectToAction("AccountInfo1", "Home", new { _accountInfo = accountInfo });
        }
        public IActionResult AccountInfo1(AccountInfo _accountInfo)
        {
            return View();
        }
        public IActionResult QuickPayment()
        {
            return View();
        }

        /// <summary>
        /// This function will be utilize for posting of the QuickPayment
        /// form to the server.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult QuickPayment(string username, string password)
        {
            return View();
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

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            ViewBag.ErrorMessage = "";
            return View();
        }

        [HttpPost]
        public IActionResult ForgotPassword(FormCollection form)
        {
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
          //  var response = _service.forgotPasswordAsync(userId, username, newPass).Result;
            //if (response == "Success")
            //{
            //    // Password has been reset.
            //}
            return View();
        }
    }
}
