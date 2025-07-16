using AQUACOOLCUSTOMER_PORTAL.DTO;
using Microsoft.AspNetCore.Mvc;
using ServiceReference1;

namespace AQUACOOLCUSTOMER_PORTAL.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        private ServiceReference1.Service1SoapClient _service;
        public AccountController(ILogger<AccountController> logger)
        {
            _logger = logger;
            _service = new Service1SoapClient(Service1SoapClient.EndpointConfiguration.Service1Soap);
        }
        [HttpGet]
        public ActionResult Index(string email = "", string code = "")
        {
            //email = "mansoor.ahmed@bemea.com";
            // code = "E6C1B895-9B4F-4BA9-9EE4-01ECF318674B";
            email = "mansoorahmedarain@gmail.com";
            code = "5D6BDC55-C045-43FA-B71B-BC2A933BE6C8";
            if (!string.IsNullOrEmpty(email))
            {
                var result = _service.verifyNewEmailAsync(email, code).Result;
                if (result.StartsWith("Error"))
                {
                    TempData["Error"] = result;
                }
                else
                {
                    TempData["Message"] = result;
                    return RedirectToAction("VerifyEmail");
                }
            }

            return View();
        }

        [HttpGet]
        public ActionResult VerifyEmail()
        {
            return View();
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ChangePasswordRequest()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var username = HttpContext.Session.GetString("UserName");
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(userId))
            {
                return RedirectPermanent("/Home/Index");
            }
            ViewBag.ErrorMessage = "";
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ChangePasswordRequest(ChangePasswordModel model)
        {
            var userId = HttpContext.Session.GetString("UserId");
            var username = HttpContext.Session.GetString("UserName");
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(userId))
            {
                return RedirectPermanent("/Home/Index");
            }
            ViewBag.ErrorMessage = "";
            if (ModelState.IsValid)
            {
                var response = await _service.ChangePasswordAsync(model.UserName,model.OldPassword,model.NewPassword);
                var response1 = response.Split("|");
                if (response1[0] != "Success")
                {
                    ViewBag.ErrorMessage = response;
                    return View(model);
                }
                // Successful changed
                return RedirectToAction("Index","Admin");
            }
            ViewBag.ErrorMessage = "Please fill all the required fields.";
            return View(model);
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            ViewBag.ErrorMessage = "";
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ChangePasswordModel model)
        {
            
            ViewBag.ErrorMessage = "";
            if (ModelState.IsValid)
            {
                var response = await _service.forgotPasswordAsync(model.UserName,"", model.OldPassword, model.NewPassword);
                var response1 = response.Split("|");
                if (response1[0] != "Success")
                {
                    ViewBag.ErrorMessage = response;
                    return View(model);
                }
                // Successful changed
                return RedirectToAction("Index", "Admin");
            }
            ViewBag.ErrorMessage = "Please fill all the required fields.";
            return View(model);
        }
    }
}
