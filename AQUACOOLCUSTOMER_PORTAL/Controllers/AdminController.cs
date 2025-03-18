using AQUACOOLCUSTOMER_PORTAL.DTO;
using AQUACOOLCUSTOMER_PORTAL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using NuGet.Protocol;
using ServiceReference1;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Text;
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
        /// <summary>
        ///  New landing page to show all of the ongoing requests 
        /// </summary>
        /// <returns></returns>
        public IActionResult RequestsSummary()
        {
            return View();
        }

        public ActionResult NewRegistration(string Id = "")
        {
            if (!string.IsNullOrEmpty(Id))
            {
                ViewBag.Id = Id;
                var docs = _service.getAttachmentsStatusAsync(Id).Result;
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
            }

            return View();
        }
        public async Task<IActionResult> MoveInRequest(string Id="")
        {
            
            var userId = HttpContext.Session.GetString("UserId");
            var username = HttpContext.Session.GetString("UserName");
            
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(userId))
            {
                return RedirectPermanent("/Home/Index");
            }
            if (!string.IsNullOrEmpty(Id))
            {
                ViewBag.Id = Id;
                var docs = await _service.getAttachmentsStatusAsync(Id);
                var file = new List<AxDocs>();
                foreach (var d in docs)
                {
                    file.Add(new AxDocs()
                    {
                        FileName = d.FileName,
                        Status = d.Verfiy
                    });
                }

                ViewBag.Status = file;
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
                            return RedirectToAction("NewRegistration", new { id = output[2] });
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
             ViewBag.tktID = id;
            //ViewBag.tktID = "REGRQ-000001541"; //"REGRQ-000001542"
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
                            if (a.Contains("Error|"))
                            {
                                TempData["Error"] = a;
                            }
                        }
                    }
                    //  }
                }

               // return Json(new { success = true, type });
                return RedirectToAction("NewRegistration", new { id = id });
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

        /// <summary>
        /// This method is calling from signature form
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="imageData"></param>
        /// <param name="isTicket"></param>
        /// <returns></returns>
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
        
        #region User Request

        public ActionResult UserRequests()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var username = HttpContext.Session.GetString("UserName");
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(userId))
            {
                return RedirectToAction("index", "Home");
            }
            var result = _service.getCustRequestsAllAsync(username).Result;
            List<AxRequests> requests = new List<AxRequests>();
            foreach (var r in result)
            {
                var a = new AxRequests
                {
                    ProjectID = HttpUtility.HtmlEncode(r.ProjectID),
                    ProjectName = HttpUtility.HtmlEncode(r.ProjectName),
                    Unit = HttpUtility.HtmlEncode(r.Unit),
                    UnitName = HttpUtility.HtmlEncode(r.UnitName),
                    MoveInType = HttpUtility.HtmlEncode(r.MoveInType),
                    CustomerId = HttpUtility.HtmlEncode(r.CustomerUserID),
                    TicketID = HttpUtility.HtmlEncode(r.TicketID),
                    Remarks = HttpUtility.HtmlEncode(r.Remarks),
                    DateTimeAccepted = HttpUtility.HtmlEncode(r.DateTimeAccepted),
                    RequestId = HttpUtility.HtmlEncode(r.ReqID),
                    Status = HttpUtility.HtmlEncode(r.WFStatus)
                };

                requests.Add(a);
            }

            return View(requests);
        }

        #endregion

        #region User Tickets
        public ActionResult AllTickets()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var username = HttpContext.Session.GetString("UserName");
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(userId))
            {
                return RedirectToAction("index", "Home");
            }
            var mitickets = GetMoveInTickets(username);
           
            return View(mitickets);
        }
        public List<AxTicketDetails> GetMoveInTickets(string username)
        {
            //return View();
            
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

            // ViewBag.InTickets = mitickets;

            return mitickets;
        }
        /// <summary>
        /// This will be list page for all move in tickets 
        /// </summary>
        /// <returns></returns>
        public ActionResult MoveInTickets()
        {
            //return View();
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
            var userId = HttpContext.Session.GetString("UserId");
            var username = HttpContext.Session.GetString("UserName");
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(userId))
            {
                return RedirectToAction("index", "Home");
            }
            var moveOutTickets = _service.getCustTicketsMoveOutAsync(username).Result;
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

        #region Invoices

        public bool CheckNotIsSwissOrNakheel(string contractId)
        {
            
            var IsSwissNakheel = false;
            if (string.IsNullOrEmpty(contractId))
                throw new Exception("Contract Id cannot be null");
            var userId = HttpContext.Session.GetString("UserId");
            var username = HttpContext.Session.GetString("UserName");
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(userId))
            {
                return false;
            }
            var u = _service.getCustomerByUserIDAsync(username).Result;
            var contracts = _service.getCustContAsync(u, true).Result.ToList();
            foreach (var c in contracts)
            {
                if ((!c.MainAgreement.Equals("MAG-000033")) && (!c.MainAgreement.Equals("MAG-000003")))
                {
                    IsSwissNakheel = true;
                }
                else
                {
                    IsSwissNakheel = false;
                    break;
                }
            }
            return IsSwissNakheel;
        }


        public ActionResult Invoices(string contractId)
        {
            // var IsSwissNakheel = false;
            if (string.IsNullOrEmpty(contractId))
                throw new Exception("Contract Id cannot be null");
            var userId = HttpContext.Session.GetString("UserId");
            var username = HttpContext.Session.GetString("UserName");
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(userId))
            {
                return RedirectToAction("index", "Home");
            }
            var u = _service.getCustomerByUserIDAsync(username).Result;
            var invoices = _service.GetInvoicesAsync(u, contractId).Result; 
            var contracts = _service.getCustContAsync(u, true).Result.ToList();
            var contractsList = new List<AxContract>();
            //foreach (var c in contracts)
            //{
            //    if ((!c.MainAgreement.Equals("MAG-000033")) && (!c.MainAgreement.Equals("MAG-000003")))
            //    {
            //        IsSwissNakheel = true;                    
            //    }
            //    else
            //    {
            //        IsSwissNakheel = false;
            //        break;
            //    }
            //}
            ViewBag.IsSwissNakheel = _service.EnableCCPayOptionAsync(contractId).Result.ToString().Trim().Equals("Yes") ? true : false;

            ViewBag.Balance = _service.getCustContractBalanceAsync(u, contractId).Result;

            TempData["ContractId"] = contractId;
            ViewData["ContractId"] = contractId;
            TempData["TicketId"] = contractId;

            var result = new List<AxInvoice>();
            foreach (var invoice in invoices)
            {
                var i = new AxInvoice();
                i.Id = invoice.Id;
                i.Description = invoice.description;
                i.Amount = double.Parse(invoice.Amount);
                i.InvoiceDate = DateTime.Parse(invoice.InvDate, CultureInfo.GetCultureInfo("en-US"));
                i.DueDate = DateTime.Parse(invoice.duedate, CultureInfo.GetCultureInfo("en-US"));
                i.ContractId = contractId;
                result.Add(i);
            }
            return View(result);
        }

        public ActionResult Payments(string contractId)
        {
            if (string.IsNullOrEmpty(contractId))
                throw new Exception("Contract Id cannot be null");
            var userId = HttpContext.Session.GetString("UserId");
            var username = HttpContext.Session.GetString("UserName");
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(userId))
            {
                return RedirectToAction("index", "Home");
            }
            TempData["ContractId"] = contractId;
            var u = _service.getCustomerByUserIDAsync(username).Result;
            var invoices = _service.getCustPaymentsAsync(u, contractId).Result;
            var result = new List<AxPayments>();
            foreach (var invoice in invoices)
            {
                var i = new AxPayments();
                i.TransactionNumber = invoice.TransNum;
                i.Amount = double.Parse(invoice.Amount);
                i.JournalNumber = invoice.JournalNo;
                i.Date = DateTime.Parse(invoice.Date, CultureInfo.GetCultureInfo("en-US"));
                i.ContractId = contractId;
                i.PayAgainst = invoice.PayAgainst;
                result.Add(i);
            }
            return View(result);
        }
        #endregion

        #region Move Out Requests
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
            ViewBag.BankNameList = Getbanks();
            
            //var cu = _service.getCustomerByUserIDAsync(username).Result;
            //ViewBag.Contract = cu;
            //ViewBag.ContractId = ContractId;
            ViewBag.OTPReady = false;
            //TempData["ContractId"] = ContractId;
            //TempData["RequestId"] = requestId;
            //TempData["CustomerType"] = customerType;
            // var units = LoadUnitSelection("test");

            return View(projects);
        }
        public ActionResult InitiateMoveOut(string ContractID, string RequestId, string CustomerType)
        {
            var userId = HttpContext.Session.GetString("UserId");
            var username = HttpContext.Session.GetString("UserName");
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(userId))
            {
                return RedirectToAction("index", "Home");
            }
            var account = _service.getCustomerByUserIDAsync(username).Result;

            string result = "";

            if (CustomerType.ToLower() == "tenant")
            {
                //if (!_client.validateCustBankAcount(User.Identity.Name, ContractID))
                if (!_service.validateCustBankAcountAsync(account, ContractID).Result)
                {
                    return RedirectToAction("UpdateBankingDetails", new { ContractId = ContractID, requestId = RequestId, customerType = CustomerType });
                }
                else
                {
                    result = _service.initiateDirectMoveOutAsync(username, ContractID, DateTime.Now.ToString()).Result;
                }
            }
            else
            {
                //if (!_client.validateCustBankAcount(User.Identity.Name, ContractID))
                if (!_service.validateCustBankAcountAsync(account, ContractID).Result)
                {
                    return RedirectToAction("UpdateBankingDetails", new { ContractId = ContractID, requestId = RequestId, customerType = CustomerType });
                }
                else
                {
                    if (!string.IsNullOrEmpty(RequestId))
                        result = _service.initiateRegMoveOutAsync(ContractID, RequestId).Result;
                }
            }

            if (string.IsNullOrEmpty(result))
            {
                TempData["Error"] = "Result is Empty. Please Contact Customer support";
                return RedirectToAction("UserProfile");
            }

            var output = result.Split('|');

            if (output[0] == "Success")
            {
                TempData["Message"] = output[1];
                return RedirectToAction("UploadNoC", new { Id = output[1] });
            }
            else
            {
                TempData["Error"] = output[1];
                return RedirectToAction("UserProfile");
            }
        }

        public ActionResult UploadNoC(string Id)
        {
            ViewBag.Id = Id;
            return View();
        }

        [HttpPost]
        //public ActionResult UploadNoC(HttpPostedFileBase files, string Id)
        //{
        //    try
        //    {
        //        if (files != null && files.ContentLength > 0)
        //        {
        //            var extension = Path.GetExtension(files.FileName);
        //            string fileName = $"noc{extension}";

        //            var byteArray = Data(files.InputStream);
        //            var data = Convert.ToBase64String(byteArray);

        //            var ticketId = _service.submitTicketMoveOutAsync(Id, fileName, data).Result;
        //            var result = _service.attachAqcFilesAsync(ticketId, "noc", data, "yes").Result;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }

        //    return RedirectToAction("MoveOutTickets");
        //}

        //public ActionResult UpdateBankingDetails(string ContractId, string requestId, string customerType)
        //{
        //    var userId = HttpContext.Session.GetString("UserId");
        //    var username = HttpContext.Session.GetString("UserName");
        //    if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(userId))
        //    {
        //        return RedirectToAction("index", "Home");
        //    }

        //    var cu = _service.getCustomerByUserIDAsync(username).Result;
        //    ViewBag.Contract = cu;
        //    ViewBag.ContractId = ContractId;
        //    ViewBag.OTPReady = false;
        //    TempData["ContractId"] = ContractId;
        //    TempData["RequestId"] = requestId;
        //    TempData["CustomerType"] = customerType;
        //    Getbanks();
        //    return View(new BankDetails()
        //    {
        //        MoveOutdate = DateTime.Now,
        //    });
        //}

        [HttpPost]
        public async Task<ActionResult> UpdateBankingDetails(IFormCollection formVal, BankDetails bankDetails, string submitButton)
        {
            var userId = HttpContext.Session.GetString("UserId");
            var username = HttpContext.Session.GetString("UserName");
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(userId))
            {
                return RedirectToAction("index", "Home");
            }
            var cu = _service.getCustomerByUserIDAsync(username).Result;
            ViewBag.Contract = cu;
            bankDetails.CustomerId = cu;
            //ViewBag.Contract = User.Identity.Name;
            //bankDetails.CustomerId = User.Identity.Name;

            if (string.IsNullOrWhiteSpace(bankDetails.ContractId))
            {
                bankDetails.ContractId = Convert.ToString(TempData["ContractId"]);
            }
            if (string.IsNullOrWhiteSpace(bankDetails.RequestId))
            {
                bankDetails.RequestId = Convert.ToString(TempData["RequestId"]);
            }
            if (string.IsNullOrWhiteSpace(bankDetails.CustomerType))
            {
                bankDetails.CustomerType = Convert.ToString(TempData["CustomerType"]);
            }

            if (submitButton.Equals("getotp") || submitButton.Equals("resendotp"))
            {
                if (ValidateBankAccount(bankDetails.IBanNo.Trim()))
                {
                    if (string.IsNullOrWhiteSpace(bankDetails.OTP))
                    {
                        var bankslist = _service.GetSDRefundBanksListAsync().Result;
                        foreach (SDRefundBanksList d in bankslist)
                        {
                            if (d.BankCode.Equals(bankDetails.ShortName))
                            {
                                bankDetails.BankName = d.BankName;
                            }
                        }
                        var returnMessage = await _service.getBankDetailsAsync(HttpUtility.HtmlEncode(bankDetails.CustomerId), HttpUtility.HtmlEncode(bankDetails.ContractId), HttpUtility.HtmlEncode(bankDetails.ShortName), HttpUtility.HtmlEncode(bankDetails.BankName), HttpUtility.HtmlEncode(bankDetails.IBanNo.Trim()), HttpUtility.HtmlEncode(bankDetails.AccountNumber.Trim()), string.Empty);

                        TempData["Message"] = returnMessage;
                        if (returnMessage.Contains("OTP"))
                        {
                            ViewBag.OTPReady = true;
                        }
                        else
                        {
                            ViewBag.OTPReady = false;
                        }
                    }
                }
                else
                {
                    ViewBag.OTPReady = false;
                    TempData["Error"] = "Invalid IBAN Number";
                }
            }
            else if (submitButton.Equals("validate"))
            {
                ViewBag.OTPReady = false;

                var returnMessage = _service.getBankDetailsAsync(bankDetails.CustomerId, bankDetails.ContractId, string.Empty, string.Empty, string.Empty, string.Empty, bankDetails.OTP.Trim()).Result;

                TempData["Message"] = returnMessage;
            }
            else
            {
                if (submitButton.Equals("next"))
                {

                    string result = "";

                    string CustomerType = Convert.ToString(bankDetails.CustomerType);

                    if (CustomerType.ToLower() == "tenant")
                    {

                        result = _service.initiateDirectMoveOutAsync(username, bankDetails.ContractId, bankDetails.MoveOutdate.ToString()).Result;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(bankDetails.RequestId))
                            result = _service.initiateRegMoveOutAsync(bankDetails.ContractId, bankDetails.RequestId).Result;
                    }

                    if (string.IsNullOrEmpty(result))
                    {
                        TempData["Error"] = "Result is Empty. Please Contact Customer support";
                        return RedirectToAction("UserProfile");
                    }

                    var output = result.Split('|');

                    if (output[0] == "Success")
                    {
                        try
                        {// commented by MA
                            if (!string.IsNullOrEmpty(formVal["CheckCheque"]) && Convert.ToBoolean(formVal["CheckCheque"]))
                            {
                                if (_service.validateCustBankAcountAsync(bankDetails.CustomerId, bankDetails.ContractId).Result)
                                {
                                    await _service.deleteCustBankAccountAsync(bankDetails.CustomerId, bankDetails.ContractId);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.WriteLine("Error occured while processing: " + ex.Message);
                        }


                        TempData["Message"] = output[1];
                        return RedirectToAction("UploadNoC", new { Id = output[1] });
                    }
                    else
                    {
                        TempData["Error"] = output[1];
                        return RedirectToAction("UserProfile");
                    }
                }
            }
            //ViewBag.ShowOTP = false;
            Getbanks();
            return View();
        }

        public static bool ValidateBankAccount(string bankAccount)
        {
            // Praveen Saraf : VALIDATION OF IBAN AND IN ORDER TO COPE WITH THE REGEX BELOW MAKING IT UPPER
            bankAccount = bankAccount.ToUpper();
            if (string.IsNullOrEmpty(bankAccount))
                return false;
            else if (System.Text.RegularExpressions.Regex.IsMatch(bankAccount, "^[A-Z0-9]"))
            {
                bankAccount = bankAccount.Replace(" ", string.Empty);
                string bank =
                bankAccount.Substring(4, bankAccount.Length - 4) + bankAccount.Substring(0, 4);
                int asciiShift = 55;
                StringBuilder sb = new StringBuilder();
                foreach (char c in bank)
                {
                    int v;
                    if (Char.IsLetter(c)) v = c - asciiShift;
                    else v = int.Parse(c.ToString());
                    sb.Append(v);
                }
                string checkSumString = sb.ToString();
                int checksum = int.Parse(checkSumString.Substring(0, 1));
                for (int i = 1; i < checkSumString.Length; i++)
                {
                    int v = int.Parse(checkSumString.Substring(i, 1));
                    checksum *= 10;
                    checksum += v;
                    checksum %= 97;
                }
                return checksum == 1;
            }
            else
                return false;
        }

        private List<SelectListItem> Getbanks()
        {
            var bankslist = _service.GetSDRefundBanksListAsync().Result;
            List<SelectListItem> newList = new List<SelectListItem>();
            newList = (from d in bankslist
                       select new SelectListItem()
                       {
                           Text = d.BankName,
                           Value = d.BankCode
                       }).ToList();
            newList.Insert(0, new SelectListItem()
            {
                Text = "Please select",
                Value = "Please Select"
            });
            //  ViewBag.Banks = newList;
            return newList;
        }

        public ActionResult OTPPOPUP()
        {

            return View();
        }

        #endregion
        #region Payments
        public ActionResult MoveInCharges(string Contract, string Ticket)
        {
            var userId = HttpContext.Session.GetString("UserId");
            var username = HttpContext.Session.GetString("UserName");
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(userId))
            {
                return RedirectToAction("index", "Home");
            }
            var charges = _service.getContractMoveInChargesAsync(Contract).Result;
            var Balance = _service.getCustContractBalanceAsync(_service.getCustomerByUserIDAsync(username).Result, Contract).Result;
            ViewBag.Balance = Balance;

            var axCharges = new List<AxCharges>();

            foreach (var c in charges)
            {
                var a = new AxCharges()
                {
                    Description = c.Description,
                    Amount = double.Parse(c.Amount).ToString("N2"),
                    FeesId = c.FeesId
                };

                axCharges.Add(a);
            }

            TempData["TicketId"] = Ticket;
            TempData["ContractId"] = Contract;

            return View(axCharges);
        }

        public ActionResult MoveOutCharges(string Contract, string Ticket)
        {
            TempData["TicketId"] = Ticket;
            TempData["ContractId"] = Contract;
            TempData["amount"] = _service.getMoveOutBalanceTicketAsync(Ticket).Result;

            return RedirectToAction("Index", "Gateway");
        }

        public ActionResult Pay(string amount)
        {
            var contractId = Convert.ToString(TempData["ContractId"].ToString());
            if (CheckNotIsSwissOrNakheel(contractId))
            {
                if (double.Parse(amount) < 100)
                {
                    TempData["Error"] = "The minimum amount to be Paid is AED 100";
                    TempData["ContractId"] = contractId;
                    return RedirectToAction("Invoices", new { contractId });
                }
                TempData["TicketId"] = TempData["TicketId"].ToString();
                TempData["ContractId"] = TempData["ContractId"].ToString();
                TempData["amount"] = amount;
                return RedirectToAction("Index", "Gateway");
            }
            else
            {
                return RedirectToAction("Invoices", new { contractId });
            }
        }
        #endregion
        #region User Profile
        public ActionResult UserProfile()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var username = HttpContext.Session.GetString("UserName");
            var cu = _service.getCustomerByUserIDAsync(username).Result;
            ViewBag.Username = username;
            ViewBag.Contract = cu;
            var contracts = Contracts_Read(username);

            return View(contracts);
        }

        public List<AxContract> Contracts_Read(string username)
        {
            var cu = _service.getCustomerByUserIDAsync(username).Result;
            var contracts = _service.getCustContAsync(cu, true).Result.ToList();

            var contractsList = new List<AxContract>();
            foreach (var c in contracts)
            {
                var a = new AxContract
                {
                    ContractID = HttpUtility.HtmlEncode(c.ContractID),
                    Customer = HttpUtility.HtmlEncode(c.Customer),
                    PropertyId = HttpUtility.HtmlEncode(c.PropertyId),
                    MainAgreement = HttpUtility.HtmlEncode(c.MainAgreement),
                    CustomerType = HttpUtility.HtmlEncode(c.CustType),
                    Unit = HttpUtility.HtmlEncode(c.UnitName),
                    Project = HttpUtility.HtmlEncode(c.ProjectName),
                    RequestId = HttpUtility.HtmlEncode(""),
                    Balance = _service.getCustContractBalanceAsync(HttpUtility.HtmlEncode(c.Customer), HttpUtility.HtmlEncode(c.ContractID)).Result
                };

                var propertyRequest = _service.getPropertyPendingRequestsAsync(HttpUtility.HtmlEncode(c.PropertyId)).Result;

                if (propertyRequest.Length > 0)
                {
                    a.RequestId = propertyRequest[0].ReqID;
                }

                contractsList.Add(a);
            }

            return contractsList;
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
