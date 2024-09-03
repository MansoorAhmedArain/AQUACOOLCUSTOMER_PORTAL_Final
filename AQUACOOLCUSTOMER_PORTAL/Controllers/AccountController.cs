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
