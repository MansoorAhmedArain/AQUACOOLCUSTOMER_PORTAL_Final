using AQUACOOLCUSTOMER_PORTAL.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using NuGet.Protocol;
using ServiceReference1;
using System.Web;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
        public async Task<IActionResult> MoveInRequest(string Id="")
        {
            
            var userId = HttpContext.Session.GetString("UserId");
            var username = HttpContext.Session.GetString("UserName");
            //if (!string.IsNullOrEmpty(Id))
            //{
            //    ViewBag.Id = Id;
            //    var docs = await _service.getAttachmentsStatusAsync(Id);
            //    var file = new List<AxDocs>();
            //    foreach (var d in docs)
            //    {
            //        file.Add(new AxDocs()
            //        {
            //            FileName = d.FileName,
            //            Status = d.Verfiy
            //        });
            //    }

            //    ViewBag.Status = file;
            //}
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(userId))
            {
                return RedirectPermanent("/Home/Index");
            }
            Projects[] projects = await LoadProjectSelection();
            // ViewBag.Projects = projects;
            // var units = LoadUnitSelection("test");
            ViewBag.Projects = projects;
            //ViewBag.Error = "";
            return View(projects);
        }

        /// <summary>
        /// Registration started.
        /// </summary>
        /// <param name="registration"></param>
        /// <param name="frm"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> MoveInRequest(AxRegistration registration, IFormCollection frm)
        {
            var userId = HttpContext.Session.GetString("UserId");
            var username = HttpContext.Session.GetString("UserName");
            registration.UserId = userId;
            ModelState.Remove("UserId");
            ModelState.Remove("RequestId");
            if (frm["Agreement"] == "on")
            {
                registration.Agreement = true;
                ModelState.Remove("Agreement");
            }
            else
            {
                ModelState.AddModelError("Agreement", "Please Accept the End User Agreement");
            }
            Projects[] projects = await LoadProjectSelection();
            ViewBag.Projects = projects;
            if (!ModelState.IsValid)
            {
                string error = "";
                foreach (var e in ModelState)
                {
                    foreach (ModelError valueError in e.Value.Errors)
                    {
                        error += valueError.ErrorMessage + ",";
                    }
                }
                ViewBag.Error = error;
                return View(projects);
            }
            else
            {
                var result = await _service.newRegistrationAsync(
                    username, // get name of customer from session
                    registration.ProjectId,
                    registration.MoveInAs,
                    registration.PropertyId,
                    DateTime.Now.Date.ToString("MM/dd/yyyy"),
                    DateTime.Now.Date.AddDays(365).ToString("MM/dd/yyyy"));

                if (string.IsNullOrEmpty(result))
                {
                    //TempData["Error"] = "Error Occurred";
                    ViewBag.Error = "Error Occurred";
                }
                else
                {
                    var output = result.Split('|');
                    if (output[0].ToLower() == ("success"))
                    {
                        registration.RequestId = output[2];
                        ViewBag.Id = registration.RequestId;
                        if (output[1] == "T")
                        {
                            TempData["Message"] = $"Ticket Created: Please upload documents for Ticket: {output[2]}";
                            return RedirectToAction("MoveInTickets");
                        }
                        else
                        {
                            TempData["Message"] = $"New Request Created: Request ID: {output[2]}";

                            var docs = await _service.getAttachmentsStatusAsync(registration.RequestId);
                            var files = new List<AxDocs>();
                            foreach (var d in docs)
                            {
                                files.Add(new AxDocs()
                                {
                                    FileName = d.FileName,
                                    Status = d.Verfiy
                                });
                            }

                            ViewBag.Status = files;
                            // redirect to upload documents.
                            return RedirectToAction("UpdateDocuments", new { id = output[2] });
                        }
                    }
                    else
                    {
                        ViewBag.Error = output[1];
                    }
                }
            }

            return View(projects);
        }
        [HttpGet]
        public async Task<string> LoadUnitSelection(string propertyId)
        {
            var units = await _service.getProjectDetailsAllAsync(propertyId);
           // var unit = units.Where(x=>x.PropertyID == propertyId).FirstOrDefault();
            var json = JsonConvert.SerializeObject(units);
            return json;
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
        #region Change Password

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

        #endregion

        [HttpGet]
        public IActionResult UpdateDocuments(string id = "")
        {
            // ViewBag.tktID = id;
            ViewBag.tktID = "REGRQ-000001541";
            return View();
        }

        #region Uploads

        [HttpPost]
        public async Task<IActionResult> Uploads(IFormFile file, string id, string type, string expirydate, string isTicket = "no")
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    TempData["Error"] = "Registration does not exist";
                    return RedirectToAction("UserProfile");
                }

                if (id.ToLower().StartsWith("tkt"))
                    isTicket = "yes";

                if (file != null)
                {
                    //foreach (IFormFile file in file)
                    //{
                    if (file != null && file.Length > 0)
                    {
                        var extension = Path.GetExtension(file.FileName);

                        string fileName;
                        if (!string.IsNullOrEmpty(expirydate))
                        {
                            var expDate = Convert.ToDateTime(expirydate);
                            fileName = $"{type}-{expDate:dd-MMM-yyyy}{extension}";
                        }
                        else
                        {
                            fileName = $"{type}{extension}";
                        }

                        using (var memoryStream = new MemoryStream())
                        {
                            file.CopyTo(memoryStream);
                            var byteArray = memoryStream.ToArray();
                            var data = Convert.ToBase64String(byteArray);

                            // Assuming _client.attachAqcFiles is still valid in your context
                            var a = await _service.attachAqcFilesAsync(id, fileName, data, isTicket);
                        }
                    }
                    //  }
                }

                //return Json(new { success = true, type });
                return View("UpdateDocuments", new { id = id });
            }
            catch (Exception ex)
            {

                throw;
            }
           
        }


        public byte[] Data(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        #endregion

        #region Complete Registration

        [HttpPost]
        public async Task<ActionResult> Complete(string Id, string imageData, string isTicket = "no")
        {
            if (!string.IsNullOrEmpty(imageData))
            {
                if (Id.ToLower().StartsWith("tkt"))
                    isTicket = "yes";

                var byteArray = Convert.FromBase64String(imageData);
                System.Drawing.Image image;
                using (MemoryStream ms = new MemoryStream(byteArray))
                {
                    image = System.Drawing.Image.FromStream(ms);
                }

                var result = await _service.attachAqcFilesAsync(Id, "signature.png", imageData, isTicket);
                var m = result.Split('|');

                if (m[0].ToLower().StartsWith("error"))
                    TempData["Error"] = result; // "Request not submitted succesfully";
                else
                {
                    result = await _service.submitReqTicketAsync(Id, isTicket);
                    if (result.ToLower().StartsWith("error"))
                    {
                        TempData["Error"] = result;
                    }
                    else
                    {
                        if (isTicket == "no")
                            TempData["Message"] = "Request submitted successfully, You will receive an email in few minutes.";
                        else
                            TempData["Message"] = "Request received. It is under reveiew. You will receive an email once completed.";
                    }
                }
            }
            else
            {
                TempData["Error"] = "Signature is required. Request not submitted succesfully";
            }

            if (isTicket == "yes")
                return RedirectToAction("MoveInTickets");

            return RedirectToAction("UserProfile");
        }

        #endregion

        #region User Tickets

        /// <summary>
        /// This will be list page for all move in tickets 
        /// </summary>
        /// <returns></returns>
        public ActionResult MoveInTickets()
        {
            return View();
            var userId = HttpContext.Session.GetString("UserId");
            var username = HttpContext.Session.GetString("UserName");
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(userId))
            {
                return RedirectToAction("index", "Home");
            }
            var u = _service.getCustomerByUserIDAsync(username).Result;
            var moveInTickets = _service.getCustTicketsMoveInAsync(u).Result;
            var mitickets = new List<AxTicketDetails>();
            //var ccode = _client.getCustomerByUserID(User.Identity.Name);

            foreach (var t in moveInTickets)
            {
                var a = new AxTicketDetails
                {
                    Id = HttpUtility.HtmlEncode(t.BETicketID),
                    RequestId = HttpUtility.HtmlEncode(t.MimoRequestID),
                    Status = HttpUtility.HtmlEncode(t.BETicketStatus),
                    ContractId = HttpUtility.HtmlEncode(t.EndUserAgreementID),
                    RequestType = HttpUtility.HtmlEncode(t.RequestType),
                    PropertyId = HttpUtility.HtmlEncode(t.PropertyID)
                };

                a.ContractId = _service.CheckNewMovinContractAsync(u, HttpUtility.HtmlEncode(a.PropertyId), a.Id).Result;
                a.Balance = _service.getCustContractBalanceAsync(u, HttpUtility.HtmlEncode(a.ContractId)).Result;

                mitickets.Add(a);
            }

            ViewBag.InTickets = mitickets;

            return View();
        }
        /// <summary>
        /// This will be list page for all move out tickets 
        /// </summary>
        /// <returns></returns>
        public ActionResult MoveOutTickets()
        {
            var moveOutTickets = _service.getCustTicketsMoveOutAsync(User.Identity.Name).Result;
            var motickets = new List<AxTicketDetails>();
            foreach (var t in moveOutTickets)
            {
                var a = new AxTicketDetails
                {
                    Id = t.BETicketID,
                    Status = t.BETicketStatus,
                    ContractId = t.EndUserAgreementID
                };

                motickets.Add(a);

                a.Balance = _service.getMoveOutBalanceTicketAsync(t.BETicketID).Result;
            }
            ViewBag.OutTickets = motickets;

            return View();
        }

        #endregion

        public IActionResult TransactionHistory()
        {
            return View(); 
        }
        public IActionResult AccountHistory()
        {
            return View();
        }
    }
}
