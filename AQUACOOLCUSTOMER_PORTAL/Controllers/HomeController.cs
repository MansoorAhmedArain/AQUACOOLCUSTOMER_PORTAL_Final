using AQUACOOLCUSTOMER_PORTAL.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using ServiceReference1;
using AQUACOOLCUSTOMER_PORTAL.DTO;
using System.Drawing.Drawing2D;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
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
        /// and travese it to next form as hidden fields.Registration as indiviual
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
           
            ViewData["regType"] = regType;
            if (regType.ToLower() == "company")
            {

                return RedirectToAction("AccountAsCompany", "Home", new { _accountInfo = "" });
            }
            else if(regType.ToLower() == "individual")
            {
                return RedirectToAction("AccountAsIndividual", "Home", new { _accountInfo = "" });
            }
           
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
            //var response =  _service.RegistrationAsync("", "", "", accountInfo.PrimaryEmailAddress, "", "", accountInfo.FullName, "", accountInfo.FullName,
            //     accountInfo.PrimaryMobileNo, "Pakistani", "passno", "pbox", accountInfo.SecondryEmailAddress, accountInfo.SecondryMobileNo, "", "", "",
            //     "", "", "", "","").Result;

            // return RedirectToAction("AccountInfo1", "Home", new { _accountInfo = accountInfo });
            return View();
        }
        [HttpGet]
        public IActionResult AccountAsCompany()
        {

            return View();
        }
        [HttpPost]
        public IActionResult AccountAsCompany(AccountInfo info)
        {
            //var response =  _service.RegistrationAsync("", "", "", accountInfo.PrimaryEmailAddress, "", "", accountInfo.FullName, "", accountInfo.FullName,
            //     accountInfo.PrimaryMobileNo, "Pakistani", "passno", "pbox", accountInfo.SecondryEmailAddress, accountInfo.SecondryMobileNo, "", "", "",
            //     "", "", "", "","").Result;
            return View();
        }

        [HttpGet]
        public IActionResult AccountAsIndividual()
        {

            return View();
        }

        [HttpPost]
        public IActionResult AccountAsIndividual(AccountInfo info)
        {
            //var response =  _service.RegistrationAsync("", "", "", accountInfo.PrimaryEmailAddress, "", "", accountInfo.FullName, "", accountInfo.FullName,
            //     accountInfo.PrimaryMobileNo, "Pakistani", "passno", "pbox", accountInfo.SecondryEmailAddress, accountInfo.SecondryMobileNo, "", "", "",
            //     "", "", "", "","").Result;
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

        /// <summary>
        /// Get all the validation data from previously build logic.
        /// </summary>
        /// <param name="customer"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        public ActionResult Register(AxCustomer customer)
        {
            int mobile1;
            string validation = string.Empty;
            bool IsValid = false;
            var workin = int.TryParse(HttpUtility.HtmlEncode(customer.MobileNumber1), out mobile1);
            ViewBag.Countries = (from c in _service.GetCountriesAsync().Result
                                 select new SelectListItem
                                 {
                                     Text = HttpUtility.HtmlEncode(c.ShortName),
                                     Value = HttpUtility.HtmlEncode(c.CountryRegId)
                                 }).ToList();

            if (!workin)
            {
                TempData["Error"] = "Mobile Number 1 is not valid. Only digits are allowed";
                return View(customer);
            }

            if (customer.Password != customer.ConfirmPassword)
            {
                TempData["Error"] = "Password and Confirm Password should match.";
                return View(customer);
            }

            if (string.IsNullOrEmpty(customer.Account))
            {
                TempData["Error"] = "Please select the account type";
                return View(customer);
            }

            if (customer.Agreement == false)
            {
                TempData["Error"] = "Please select the Disclaimer check";
                return View(customer);
            }

            var passportExpiry = customer.PassportExpiryDate ?? "1/1/2000";
            var idExpiry = customer.IdExpiryDate ?? "1/1/2000";

            if (customer.Account.Equals("Company"))
            {
                int tldno;
                var TLD = int.TryParse((customer.TradeLicenseExpiryDate.Replace("/", "")), out tldno);
                if (!TLD)
                {
                    TempData["Error"] = "Invalid Trade License Expiry";
                    return View(customer);
                }
            }

            if (customer.Resident.Equals("Resident"))
            {
                int idno;
                var EID = int.TryParse((idExpiry.Replace("/", "")), out idno);
                if (!EID)
                {
                    TempData["Error"] = "Invalid Emirates ID Expiry";
                    return View(customer);
                }
            }

            int pdno;
            var PED = int.TryParse((passportExpiry.Replace("/", "")), out pdno);
            if (!PED)
            {
                TempData["Error"] = "Invalid Passport Expiry Date";
                return View(customer);
            }

            validation = PerformValidation(customer);
            //  validation = "Success";
            if (validation.Equals("Success"))
            {
                IsValid = true;
            }
            else
            {
                IsValid = false;
                TempData["Error"] = validation + " is not valid";
                return View(customer);
            }

            if (IsValid)
            {
                ViewBag.Customer = Newtonsoft.Json.JsonConvert.SerializeObject(customer);
                var result =  _service.RegistrationAsync(HttpUtility.HtmlEncode(customer.Account),
                      HttpUtility.HtmlEncode(customer.Emirate.ToString()),
                      HttpUtility.HtmlEncode(customer.Nationality),
                      HttpUtility.HtmlEncode(customer.EmailAddress1),
                      HttpUtility.HtmlEncode(customer.IdNumber),
                      HttpUtility.HtmlEncode(idExpiry),
                      HttpUtility.HtmlEncode(customer.FirstName),
                      HttpUtility.HtmlEncode("00000"),
                      HttpUtility.HtmlEncode(customer.LastName),
                      HttpUtility.HtmlEncode(mobile1.ToString()),
                      HttpUtility.HtmlEncode(customer.Nationality),
                      HttpUtility.HtmlEncode(passportExpiry),
                      HttpUtility.HtmlEncode(customer.PassportNumber),
                      HttpUtility.HtmlEncode(customer.PoBox),
                      HttpUtility.HtmlEncode(customer.EmailAddress2),
                      HttpUtility.HtmlEncode(customer.MobileNumber2),
                      HttpUtility.HtmlEncode(customer.Address),
                      HttpUtility.HtmlEncode(customer.EmailAddress1),
                      HttpUtility.HtmlEncode(customer.Password),
                      HttpUtility.HtmlEncode(customer.Company),
                      HttpUtility.HtmlEncode(customer.TradeLicenseNumber),
                      HttpUtility.HtmlEncode(customer.TradeLicenseExpiryDate),
                      HttpUtility.HtmlEncode(customer.TRN)).Result;

                if (string.IsNullOrEmpty(result))
                {
                    TempData["Error"] = "Some Error Occurred";
                    ViewBag.Customer = Newtonsoft.Json.JsonConvert.SerializeObject(customer);
                }
                else
                {
                    var mess = result.Split('|');
                    if (mess[0] == "Error")
                    {
                        TempData["Error"] = mess[1];
                        TempData["IsSuccess"] = false;
                        ViewBag.Customer = Newtonsoft.Json.JsonConvert.SerializeObject(customer);
                    }
                    else if (mess[0].StartsWith("Value cannot be null"))
                    {
                        TempData["Error"] = mess[0];
                        TempData["IsSuccess"] = false;
                        ViewBag.Customer = Newtonsoft.Json.JsonConvert.SerializeObject(customer);
                    }
                    else
                    {
                        TempData["IsSuccess"] = true;
                        TempData["Message"] = result;
                    }
                }
            }
            //  var result = "Success";


            return View(customer);
        }
        private string PerformValidation(AxCustomer AxCustomerObject)
        {
            string Validation = string.Empty;

            // Account Validation

            if (!AxCustomerObject.Account.Equals("Individual") && !AxCustomerObject.Account.Equals("Company"))
            {
                Validation = "Account";
                return Validation;
            }
            else
            {
                if (AxCustomerObject.Account.Equals("Company"))
                {
                    if (!Regex.Match(AxCustomerObject.Company.Trim(), @"^[a-zA-Z]+(?:[\s.]+[a-zA-Z]+)*$").Success)
                    {
                        Validation = "Company / Trade Name";
                        return Validation;
                    }
                    else
                    {
                        Validation = "Success";
                    }

                    if (!Regex.Match(AxCustomerObject.TradeLicenseNumber.Trim(), @"^[a-zA-Z0-9]*$").Success)
                    {
                        Validation = "Trade License Number";
                        return Validation;
                    }
                    else
                    {
                        Validation = "Success";
                    }

                    if (!Regex.Match(AxCustomerObject.TRN.Trim(), @"^\d+$").Success)
                    {
                        Validation = "Tax Registration Number";
                        return Validation;
                    }
                    else
                    {
                        Validation = "Success";
                    }
                }
                else
                {
                    Validation = "Success";
                }
            }

            if (!AxCustomerObject.Resident.Equals("Resident") && !AxCustomerObject.Resident.Equals("Non Resident"))
            {
                Validation = "Resident";
                return Validation;
            }
            else
            {
                if (AxCustomerObject.Resident.Equals("Resident"))
                {
                    if (!Regex.Match((AxCustomerObject.IdNumber.Contains("-") ? AxCustomerObject.IdNumber.Replace("-", "") : AxCustomerObject.IdNumber), @"^\d+$").Success)
                    {
                        Validation = "Emirates ID";
                        return Validation;
                    }
                    else
                    {
                        Validation = "Success";
                    }
                }
                else
                {
                    Validation = "Success";
                }
            }

            // First Name Validation
            if (!Regex.Match(AxCustomerObject.FirstName.Trim(), @"^[a-zA-Z]+(?:[\s.]+[a-zA-Z]+)*$").Success)
            {
                Validation = "First Name";
                return Validation;
            }
            else
            {
                Validation = "Success";
            }

            // Last Name Validation
            if (!Regex.Match(AxCustomerObject.LastName.Trim(), @"^[a-zA-Z]+(?:[\s.]+[a-zA-Z]+)*$").Success)
            {
                Validation = "Last Name";
                return Validation;
            }
            else
            {
                Validation = "Success";
            }

            if (!Regex.Match(AxCustomerObject.Nationality.Trim(), @"^[a-zA-Z]+(?:[\s.]+[a-zA-Z]+)*$").Success)
            {
                Validation = "Nationality";
                return Validation;
            }
            else
            {
                Validation = "Success";
            }


            if (!Regex.Match(Convert.ToString(AxCustomerObject.Emirate).Trim(), @"^[a-zA-Z0-9]*$").Success)
            {
                Validation = "Emirate";
                return Validation;
            }
            else
            {
                Validation = "Success";
            }

            if (!string.IsNullOrEmpty(AxCustomerObject.PassportNumber))
            {
                if (!Regex.Match(AxCustomerObject.PassportNumber.Trim(), @"^[a-zA-Z0-9]*$").Success)
                {
                    Validation = "Passport Number";
                    return Validation;
                }
                else
                {
                    Validation = "Success";
                }
            }
            else
            {
                Validation = "Success";
            }

            if (!string.IsNullOrEmpty(AxCustomerObject.PoBox))
            {
                if (!Regex.Match(AxCustomerObject.PoBox.Trim(), @"^[a-zA-Z0-9]*$").Success)
                {
                    Validation = "PO Box";
                    return Validation;
                }
                else
                {
                    Validation = "Success";
                }
            }
            else
            {
                Validation = "Success";
            }

            if (AxCustomerObject.Password.Length > 18)
            {
                Validation = "Password Length ";
                return Validation;
            }
            else
            {
                Validation = "Success";
            }

            if (!(new EmailAddressAttribute().IsValid(AxCustomerObject.EmailAddress1)))
            {
                Validation = "Email Address 1";
                return Validation;
            }
            else
            {
                Validation = "Success";
            }

            if (!string.IsNullOrEmpty(AxCustomerObject.EmailAddress2))
            {
                if (!(new EmailAddressAttribute().IsValid(AxCustomerObject.EmailAddress2)))
                {
                    Validation = "Email Address 2";
                    return Validation;
                }
                else
                {
                    Validation = "Success";
                }
            }
            else
            {
                Validation = "Success";
            }

            if (!(new PhoneAttribute().IsValid(AxCustomerObject.MobileNumber1)))
            {
                Validation = "MobileNumber 1";
                return Validation;
            }
            else
            {
                Validation = "Success";
            }

            if (!string.IsNullOrEmpty(AxCustomerObject.MobileNumber2))
            {
                if (!(new PhoneAttribute().IsValid(AxCustomerObject.MobileNumber2)))
                {
                    Validation = "MobileNumber 2";
                    return Validation;
                }
                else
                {
                    Validation = "Success";
                }
            }
            else
            {
                Validation = "Success";
            }

            return Validation;
        }
    }
}
