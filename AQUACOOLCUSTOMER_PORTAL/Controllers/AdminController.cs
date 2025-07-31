using AQUACOOLCUSTOMER_PORTAL.DTO;
using AQUACOOLCUSTOMER_PORTAL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;
using NuGet.Protocol;
using ServiceReference1;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.ServiceModel;
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

        public ActionResult NewRegistration(string Id = "", string type = "")
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
                ViewBag.CustomerType = type;
            }

            return View();
        }
        public async Task<IActionResult> MoveInRequest(string Id = "")
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
                            return RedirectToAction("NewRegistration", new { id = output[2], type = registration.MoveInAs });
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
        private async Task<Projects[]> LoadProjectSelection()
        {
            var projects = await _service.GetProjectsAsync();
            return projects;
        }
        private async Task<List<ContractIDs>> LoadContractsForSelection(string id)
        {
            var contractIdsList = new List<ContractIDs>();

            var custContracts = await _service.getCustContAsync(id, true);

            foreach (var item in custContracts)
            {
                contractIdsList.Add(new ContractIDs { ID = item.ContractID, Name = item.ContractID });
            }
            return contractIdsList;
        }

        /// <summary>
        /// Get reconnection form.
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> ReconnectionRequest()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var username = HttpContext.Session.GetString("UserName");
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(userId))
            {
                return RedirectPermanent("/Home/Index");
            }
            var projects = await LoadProjectSelection();

            // var units = LoadUnitSelection("test");
            ViewBag.Projects = projects;
            var contracts = await LoadContractsForSelection(userId);
            var viewModal = new ReconnectionViewModal()
            {
                ContractIDs = contracts
            };
            return View(viewModal);

        }

        /// <summary>
        /// post the reconnection data
        /// </summary>
        /// <param name="EAG"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ReconnectionRequest(string EAG, string Balance)
        {

            var userId = HttpContext.Session.GetString("UserId");
            var username = HttpContext.Session.GetString("UserName");
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(userId))
            {
                return RedirectPermanent("/Home/Index");
            }
            var projects = await LoadProjectSelection();

            // var units = LoadUnitSelection("test");
            ViewBag.Projects = projects;
            var contracts = await LoadContractsForSelection(userId);
            var viewModal = new ReconnectionViewModal()
            {
                //Projects = projects,
                ContractIDs = contracts
            };
            if (string.IsNullOrWhiteSpace(EAG))
            {
                ViewBag.Message = "EAG number could'nt be empty.";
                return View(viewModal);
            }
            if (Balance != "0" || !Balance.Contains("-"))
            {
                ViewBag.Message = "Please settle your outstanding balance along with AED 105 reconnection fee in order to proceed with your reconnection request. ";
                return View(viewModal);
            }
            var activeContracts = _service.getCustContAsync(userId, true).Result;

            foreach (var item in activeContracts)
            {
                if (item.ContractID == EAG)
                {

                    var response = _service.CreateReconnectionTicketAsync(item.PropertyId).Result;
                    ViewBag.Message = response;
                    return View(viewModal);
                }
            }

            return View(viewModal);

        }

        /// <summary>
        /// Get disconnection ticket id according to EAG number.
        /// </summary>
        /// <param name="contractId"></param>
        /// <returns></returns>
        public string GetDisconnectionTicketId(string contractId)
        {
            var userId = HttpContext.Session.GetString("UserId");
            var username = HttpContext.Session.GetString("UserName");
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(userId))
            {
                return "Not found.";
            }

            var disconnectionID = _service.GetDisconnectionTicketAsync(contractId).Result;
            var outstandingBalance = _service.GetCustContractBalanceAsync(userId, contractId).Result;
            return disconnectionID + "," + outstandingBalance;

        }
        public IActionResult StatementofAccount()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var username = HttpContext.Session.GetString("UserName");
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(userId))
            {
                return RedirectPermanent("/Home/Index");
            }
            var contracts = LoadContractsForSelection(userId).Result;
            ViewBag.ContractsList = contracts;
            return View();
        }

        [HttpPost]
        public IActionResult StatementofAccount(StatementOfAccountModel sInfo)
        {
            var userId = HttpContext.Session.GetString("UserId");
            var username = HttpContext.Session.GetString("UserName");
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(userId))
            {
                return RedirectPermanent("/Home/Index");
            }
            if (sInfo.EAG == "")
            {
                ViewBag.Message = "Please select  EAG number.";
                return View();
            }
            var contracts = LoadContractsForSelection(userId).Result;
            ViewBag.ContractsList = contracts;
            var response = _service.getCustStatementReportAsync(sInfo.EAG, sInfo.FromDate.Date.ToString("MM/dd/yyyy"), sInfo.ToDate.Date.ToString("MM/dd/yyyy")).Result;
            ViewBag.Message = response;
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
                return RedirectToAction("index", "Home");
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
            var result = _service.getCustRequestsAllAsync(userId).Result;
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

        /// <summary>
        /// Get all request of the user.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        private List<AxRequests> GetAxRequestCustom(string userId)
        {
            var result = _service.getCustRequestsAllAsync(userId).Result;
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

            return requests;
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
        [HttpGet]
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
            ViewBag.ActiveContracts = GetActiveContracts(userId);
            //var cu = _service.getCustomerByUserIDAsync(userId).Result;
            //ViewBag.Contract = cu;
            //ViewBag.ContractId = ContractId;
            ViewBag.OTPReady = false;
            return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="modal"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> MoveOutRequest(MoveOutRequestModel modal)
        {

            var userId = HttpContext.Session.GetString("UserId");
            var username = HttpContext.Session.GetString("UserName");
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(userId))
            {
                return RedirectToAction("index", "Home");
            }
            Projects[] projects = await LoadProjectSelection();
            ViewBag.Projects = projects;
            ViewBag.BankNameList = Getbanks();
            ViewBag.ActiveContracts = GetActiveContracts(userId);
            ViewBag.OTPReady = false;
            // string ContractID = string.Empty; // should be filled.
            string CustomerType = string.Empty;
            // var axRequests = GetAxRequestCustom(userId);
            var contractRead = Contracts_Read(username);
            var moveType = contractRead.Where(x => x.ContractID == modal.ContractId).Select(xx => xx.CustomerType).FirstOrDefault();
            var requestId = contractRead.Where(x => x.ContractID == modal.ContractId).Select(xx => xx.RequestId).FirstOrDefault();
            //foreach (var item in axRequests)
            //{
            //    if (item.RequestId == modal.ContractId)
            //    {
            //        CustomerType = item.MoveInType;
            //    } 
            //}
            CustomerType = moveType ?? "";

            var account = _service.getCustomerByUserIDAsync(userId).Result;

            string result = "";

            if (CustomerType.ToLower() == "tenant")
            {
                if (!_service.validateCustBankAcountAsync(account, modal.ContractId).Result && modal.Bank == "on")
                {
                    // return RedirectToAction("UpdateBankingDetails", new { ContractId = ContractID, moveType = modal.ContractId, customerType = CustomerType });
                    var bankDetails = new BankDetails()
                    {
                        BankName = modal.ShortName,
                        AccountNumber = modal.AccountNumber,
                        ContractId = modal.ContractId,
                        CustomerType = CustomerType,
                        RequestId = modal.ContractId,
                        IBanNo = modal.IBanNo,
                        OTP = modal.OTP,
                        MoveOutdate = modal.MoveOutDate,
                        ShortName = modal.ShortName,
                        SwiftNo = modal.SwiftNo
                    };
                    result = UpdateBankingDetails(bankDetails, userId, modal.SubmitButton, username).Result;

                }
                else
                {
                    result = _service.initiateDirectMoveOutAsync(username, modal.ContractId, DateTime.Now.ToString()).Result;
                }
            }
            else
            {
                //if (!_client.validateCustBankAcount(User.Identity.Name, ContractID))
                if (!_service.validateCustBankAcountAsync(account, modal.ContractId).Result && modal.Bank == "on")
                {
                    //return RedirectToAction("UpdateBankingDetails", new { ContractId = ContractID, moveType = modal.ContractId, customerType = CustomerType });
                    var bankDetails = new BankDetails()
                    {
                        BankName = modal.ShortName,
                        AccountNumber = modal.AccountNumber,
                        ContractId = modal.ContractId,
                        CustomerType = CustomerType,
                        RequestId = modal.ContractId,
                        IBanNo = modal.IBanNo,
                        OTP = modal.OTP,
                        MoveOutdate = modal.MoveOutDate,
                        ShortName = modal.ShortName,
                        SwiftNo = modal.SwiftNo
                    };
                    result = UpdateBankingDetails(bankDetails, userId, modal.SubmitButton, username).Result;

                }
                else
                {
                    if (!string.IsNullOrEmpty(modal.ContractId))
                        result = _service.initiateRegMoveOutAsync(modal.ContractId, requestId).Result;
                }
            }

            //if (string.IsNullOrEmpty(result))
            //{
            //    TempData["Error"] = "Result is Empty. Please Contact Customer support";
            //    return View(modal);
            //}

            var output = result.Split('|');

            if (output[0] == "Success" || result.Contains("TKT"))
            {
                try
                {
                    // return RedirectToAction("UploadNoC", new { Id = output[1] });
                    if (modal.NOCFile != null && modal.NOCFile.Length > 0)
                    {
                        var extension = Path.GetExtension(modal.NOCFile.FileName);
                        string fileName = $"noc{extension}";
                        using (var memoryStream = new MemoryStream())
                        {
                            modal.NOCFile.CopyTo(memoryStream);
                            var byteArray = memoryStream.ToArray();
                            var data = Convert.ToBase64String(byteArray);
                            if (output.Length == 1 && output[0].Contains("TKT"))
                            {
                                var ticketId1 = _service.submitTicketMoveOutAsync(output[0], fileName, data).Result;
                                TempData["Message"] = _service.attachAqcFilesAsync(output[0], "noc", data, "yes").Result;
                                return View(modal);
                            }
                            var ticketId = _service.submitTicketMoveOutAsync(output[1], fileName, data).Result;
                            TempData["Message"] = _service.attachAqcFilesAsync(ticketId, "noc", data, "yes").Result;
                            return View(modal);
                        }
                    }
                    TempData["Message"] = output[1];
                }
                catch (Exception ex)
                {

                    TempData["Error"] = ex.Message;
                }
               
            }
            else
            {
                if (output.Length == 2)
                {
                    TempData["Message"] = output[1];
                    return View(modal);
                }
                TempData["Message"] = output[0];

            }
            return View(modal);
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

        
        
        // refactoring by mansoor
        //public async Task<string> UpdateBankingDetails(BankDetails bankDetails, string userId, string submitButton, string username)
        //{
        //    var cu = _service.getCustomerByUserIDAsync(userId).Result;
        //    ViewBag.Contract = cu;
        //    bankDetails.CustomerId = cu;
        //    if (submitButton.Equals("getotp") || submitButton.Equals("resendotp"))
        //    {
        //        if (string.IsNullOrWhiteSpace(bankDetails.BankName))
        //        {
        //            TempData["Error"] = "Bank Name is required.";
        //            return "Bank Name is required.";
        //        }
        //        if (string.IsNullOrWhiteSpace(bankDetails.IBanNo))
        //        {
        //            TempData["Error"] = "IBan Number  is required.";
        //            return "IBan Number  is required..";
        //        }
        //        if (string.IsNullOrWhiteSpace(bankDetails.AccountNumber))
        //        {
        //            TempData["Error"] = "Account Number is required.";
        //            return "Account Number is required.";
        //        }
        //        if (string.IsNullOrWhiteSpace(bankDetails.SwiftNo))
        //        {
        //            TempData["Error"] = "Swift Code is required.";
        //            return "Swift Code is required.";
        //        }
        //        if (ValidateBankAccount(bankDetails.IBanNo.Trim()))
        //        {
        //            if (string.IsNullOrWhiteSpace(bankDetails.OTP))
        //            {
        //                var bankslist = _service.GetSDRefundBanksListAsync().Result;
        //                foreach (SDRefundBanksList d in bankslist)
        //                {
        //                    if (d.BankCode.Equals(bankDetails.ShortName))
        //                    {
        //                        bankDetails.BankName = d.BankName;
        //                    }
        //                }
        //                var returnMessage = await _service.getBankDetailsAsync(HttpUtility.HtmlEncode(bankDetails.CustomerId), HttpUtility.HtmlEncode(bankDetails.ContractId), HttpUtility.HtmlEncode(bankDetails.ShortName), HttpUtility.HtmlEncode(bankDetails.BankName), HttpUtility.HtmlEncode(bankDetails.IBanNo.Trim()), HttpUtility.HtmlEncode(bankDetails.AccountNumber.Trim()), string.Empty);

        //                TempData["Message"] = returnMessage;
        //                if (returnMessage.Contains("OTP"))
        //                {
        //                    ViewBag.OTPReady = true;
        //                }
        //                else
        //                {
        //                    ViewBag.OTPReady = false;
        //                }
        //            }
        //        }
        //        else
        //        {
        //            ViewBag.OTPReady = false;
        //            TempData["Error"] = "Invalid IBAN Number";
        //        }
        //    }
        //    else if (submitButton.Equals("validate"))
        //    {
        //        ViewBag.OTPReady = false;

        //        var returnMessage = _service.getBankDetailsAsync(bankDetails.CustomerId, bankDetails.ContractId, string.Empty, string.Empty, string.Empty, string.Empty, bankDetails.OTP.Trim()).Result;

        //        TempData["Message"] = returnMessage;
        //    }
        //    else
        //    {
        //        if (submitButton.ToLower().Equals("next"))
        //        {

        //            string result = "";

        //            string CustomerType = Convert.ToString(bankDetails.CustomerType);

        //            if (CustomerType.ToLower() == "tenant")
        //            {

        //                result = _service.initiateDirectMoveOutAsync(username, bankDetails.ContractId, bankDetails.MoveOutdate.ToString()).Result;
        //            }
        //            else
        //            {
        //                if (!string.IsNullOrEmpty(bankDetails.RequestId))
        //                    result = _service.initiateRegMoveOutAsync(bankDetails.ContractId, bankDetails.RequestId).Result;
        //            }

        //            if (string.IsNullOrEmpty(result))
        //            {
        //                TempData["Error"] = "Result is Empty. Please Contact Customer support";
        //                //return View("UserProfile");
        //                return "Result is Empty. Please Contact Customer support";
        //            }

        //            var output = result.Split('|');

        //            if (output[0] == "Success")
        //            {
        //                try
        //                {// commented by MA

        //                    if (_service.validateCustBankAcountAsync(bankDetails.CustomerId, bankDetails.ContractId).Result)
        //                    {
        //                        await _service.deleteCustBankAccountAsync(bankDetails.CustomerId, bankDetails.ContractId);
        //                    }

        //                }
        //                catch (Exception ex)
        //                {
        //                    Log.WriteLine("Error occured while processing: " + ex.Message);
        //                }


        //                TempData["Message"] = output[1];
        //                return output[1];
        //            }
        //            else
        //            {
        //                TempData["Error"] = output[1];
        //                //return RedirectToAction("UserProfile");
        //                return output[1];
        //            }
        //        }
        //    }
        //    return string.Empty;
        //}
        public async Task<string> UpdateBankingDetails(BankDetails bankDetails, string userId, string submitButton, string username)
        {
            var customerId = _service.getCustomerByUserIDAsync(userId).Result;
            ViewBag.Contract = customerId;
            bankDetails.CustomerId = customerId;

            switch (submitButton.ToLower())
            {
                case "getotp":
                case "resendotp":
                    return await HandleOtpRequest(bankDetails);

                case "validate":
                    return HandleOtpValidation(bankDetails);

                case "next":
                    return await HandleNextStep(bankDetails, username);

                default:
                    return string.Empty;
            }
        }

        private async Task<string> HandleOtpRequest(BankDetails bankDetails)
        {
            if (!ValidateBankingFields(bankDetails, out var errorMessage))
            {
                TempData["Error"] = errorMessage;
                return errorMessage;
            }

            if (!ValidateBankAccount(bankDetails.IBanNo.Trim()))
            {
                ViewBag.OTPReady = false;
                TempData["Error"] = "Invalid IBAN Number";
                return string.Empty;
            }

            if (string.IsNullOrWhiteSpace(bankDetails.OTP))
            {
                var banksList = _service.GetSDRefundBanksListAsync().Result;
                var matchedBank = banksList.FirstOrDefault(d => d.BankCode.Equals(bankDetails.ShortName));
                if (matchedBank != null)
                {
                    bankDetails.BankName = matchedBank.BankName;
                }

                var returnMessage = await _service.getBankDetailsAsync(
                    HttpUtility.HtmlEncode(bankDetails.CustomerId),
                    HttpUtility.HtmlEncode(bankDetails.ContractId),
                    HttpUtility.HtmlEncode(bankDetails.ShortName),
                    HttpUtility.HtmlEncode(bankDetails.BankName),
                    HttpUtility.HtmlEncode(bankDetails.IBanNo.Trim()),
                    HttpUtility.HtmlEncode(bankDetails.AccountNumber.Trim()),
                    string.Empty);

                TempData["Message"] = returnMessage;
                ViewBag.OTPReady = returnMessage.Contains("OTP");

                return returnMessage;
            }

            return string.Empty;
        }

        private string HandleOtpValidation(BankDetails bankDetails)
        {
            ViewBag.OTPReady = false;

            var returnMessage = _service.getBankDetailsAsync(
                bankDetails.CustomerId,
                bankDetails.ContractId,
                string.Empty, string.Empty, string.Empty, string.Empty,
                bankDetails.OTP.Trim()).Result;

            TempData["Message"] = returnMessage;
            return returnMessage;
        }

        private async Task<string> HandleNextStep(BankDetails bankDetails, string username)
        {
            try
            {
                string result = string.Empty;
                string customerType = Convert.ToString(bankDetails.CustomerType).ToLower();

                if (customerType == "tenant")
                {
                    result = _service.initiateDirectMoveOutAsync(username, bankDetails.ContractId, bankDetails.MoveOutdate.ToString()).Result;
                }
                else if (!string.IsNullOrEmpty(bankDetails.RequestId))
                {
                    result = _service.initiateRegMoveOutAsync(bankDetails.ContractId, bankDetails.RequestId).Result;
                }

                if (string.IsNullOrEmpty(result))
                {
                    TempData["Error"] = "Result is Empty. Please Contact Customer support";
                    return "Result is Empty. Please Contact Customer support";
                }

                var output = result.Split('|');
                if (output[0] == "Success")
                {
                    try
                    {
                        if (_service.validateCustBankAcountAsync(bankDetails.CustomerId, bankDetails.ContractId).Result)
                        {
                            await _service.deleteCustBankAccountAsync(bankDetails.CustomerId, bankDetails.ContractId);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.WriteLine("Error occured while processing: " + ex.Message);
                    }

                    TempData["Message"] = output[1];
                    return output[1];
                }
                else
                {
                    TempData["Error"] = output[1];
                    return output[1];
                }
            }
            catch (TimeoutException ex)
            {
                return "Your request has been submitted.";
            }
            
        }

        private bool ValidateBankingFields(BankDetails bankDetails, out string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(bankDetails.BankName))
            {
                errorMessage = "Bank Name is required.";
                return false;
            }
            if (string.IsNullOrWhiteSpace(bankDetails.IBanNo))
            {
                errorMessage = "IBan Number is required.";
                return false;
            }
            if (string.IsNullOrWhiteSpace(bankDetails.AccountNumber))
            {
                errorMessage = "Account Number is required.";
                return false;
            }
            if (string.IsNullOrWhiteSpace(bankDetails.SwiftNo))
            {
                errorMessage = "Swift Code is required.";
                return false;
            }

            errorMessage = null;
            return true;
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

        /// <summary>
        /// Get approved ax request to initate the move out request.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        private List<SelectListItem> GetApporvedAxRequest(string userId)
        {
            var result = _service.getCustRequestsAllAsync(userId).Result;

            List<SelectListItem> newList = new List<SelectListItem>();
            newList = (from d in result.Where(x => x.WFStatus == "Approved")
                       select new SelectListItem()
                       {
                           Text = d.ReqID,
                           Value = d.ReqID
                       }).ToList();
            return newList;
        }
        /// <summary>
        /// Get all active contracts to initate the move out request.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        private List<SelectListItem> GetActiveContracts(string userId)
        {
            var result = _service.getCustContAsync(userId, true).Result;

            List<SelectListItem> newList = new List<SelectListItem>();
            newList = (from d in result
                       select new SelectListItem()
                       {
                           Text = d.ContractID,
                           Value = d.ContractID
                       }).ToList();
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

        /// <summary>
        /// Call from quick record popup dialouge.
        /// </summary>
        /// <param name="payAmount"></param>
        /// <returns></returns>
        public ActionResult Pay(string payAmount)
        {
            //var contractId = "EAG-032457";
            var contractId = Convert.ToString(TempData["ContractId"]?.ToString());
            if (CheckNotIsSwissOrNakheel(contractId))
                //{
                if (double.Parse(payAmount) < 100)
                {
                    TempData["Error"] = "The minimum amount to be Paid is AED 100";
                    TempData["ContractId"] = contractId;
                    //return RedirectToAction("Invoices", new { contractId });
                    return RedirectToAction("QuickPayment", "Home");
                }
            TempData["TicketId"] = TempData["TicketId"]?.ToString();
            TempData["ContractId"] = contractId;
            TempData["amount"] = payAmount;
            return RedirectToAction("Index", "Gateway");
            //}
            //else
            //{
            //    return RedirectToAction("Invoices", new { contractId });
            //}
        }
        #endregion
        #region User Profile
        public ActionResult UserProfile()
        {
            return RedirectToAction("AccountHistory");
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

        /// <summary>
        /// Displays the transaction history for a specific contract.
        /// </summary>
        /// <returns>
        /// Returns a view containing a list of record transactions associated with the specified contract.
        /// </returns>
        /// <remarks>
        /// This method retrieves the current user's session data, fetches record records for a hardcoded contract ID,
        /// constructs a list of transaction objects, and passes them to the view for display.
        /// </remarks>
        /// <exception cref="Exception">
        /// Thrown when the contract ID is null or empty.
        /// </exception>
        public IActionResult TransactionHistory(string contractId = "")
        {
            var userId = HttpContext.Session.GetString("UserId");
            var username = HttpContext.Session.GetString("UserName");
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(userId))
            {
                return RedirectPermanent("/Home/Index");
            }
            var complainTypes = _service.GetrequestIDAsync().Result;
            var contracts = _service.getCustContAsync(userId, true).Result;
            if (contracts.Length == 0)
            {
                ViewBag.ContractsList = new List<ContractIDs>();
                ViewBag.SelectedContractEAG = "N/A";
                ViewBag.SelectedContractDueDate = "N/A";
                ViewBag.SelectedContractAmount = "N/A";
                return View(new List<AxPayments>());
            }
            if (contractId == "")
            {
                contractId = contracts[0].ContractID; //"EAG-032457";

            }
            List<AxPayments> result = GetPaymentsAccordingToContract(userId, username, contractId);
            var contractsListData = LoadContractsForSelection(userId).Result;
            ViewBag.ContractsList = contractsListData;
            ViewBag.SelectedContractEAG = contractId;
            var latestBill = _service.getLatestInvoiceAsync(contractId).Result;
            ViewBag.SelectedContractDueDate = Convert.ToDateTime(latestBill[0].DueDate).ToShortDateString();
            ViewBag.SelectedContractAmount = latestBill[0].Amount;
            return View(result);
        }
        public IActionResult BillCurve(string contractId = "")
        {
            var userId = HttpContext.Session.GetString("UserId");
            var username = HttpContext.Session.GetString("UserName");
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(userId))
            {
                return RedirectPermanent("/Home/Index");
            }
            var contracts = _service.getCustContAsync(userId, true).Result;
            if (contracts.Length == 0)
            {
                ViewBag.ContractsList = new List<ContractIDs>();
                ViewBag.SelectedContractEAG = "N/A";
                ViewBag.SelectedContractDueDate = "N/A";
                ViewBag.SelectedContractAmount = "N/A";
                return View(new List<AxPayments>());
            }
            if (contractId == "")
            {
                contractId = contracts[0].ContractID; //"EAG-032457";

            }
            var contractsListData = LoadContractsForSelection(userId).Result;
            ViewBag.ContractsList = contractsListData;
            ViewBag.SelectedContractEAG = contractId;
            return View();
        }
        [HttpGet]
        public JsonResult GetEnergyConsumption(string contractId)
        {
            var userId = HttpContext.Session.GetString("UserId");
            var username = HttpContext.Session.GetString("UserName");
            
            var contracts = _service.getCustContAsync(userId, true).Result;
            if (string.IsNullOrWhiteSpace(contractId))
            {
                contractId = contracts[0].ContractID;

            }
            string GetMonthName(int month)
            {
                return System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(month);
            }
            //"EAG-065556"

            var fetchData = new List<ConsumptionData>();
            var energyConsumptionsData = _service.getCustEnergyConsumptionAsync(contractId).Result;

            foreach (var item in energyConsumptionsData)
            {
                fetchData.Add(new ConsumptionData
                {
                    Period = item.month,
                    Value = Convert.ToDouble(item.amount)
                });
            }

            var labels = fetchData.Select(x =>
            {
                int month = Convert.ToInt32(x.Period);
                string startMonth = GetMonthName(month);
                string endMonth = GetMonthName((month % 12) + 1);
                return $"{startMonth}-{endMonth}";
            }).ToList();

            var values = fetchData.Select(x => x.Value).ToList();

            return Json(new
            {
                labels = labels,
                data = values
            });

        }

        [HttpGet]
        public JsonResult GetBillConsumption(string contractId)
        {
            var userId = HttpContext.Session.GetString("UserId");
            var username = HttpContext.Session.GetString("UserName");

            var contracts = _service.getCustContAsync(userId, true).Result;
            if (string.IsNullOrWhiteSpace(contractId))
            {
                contractId = contracts[0].ContractID;

            }
            
            string GetMonthName(int month)
            {
                return System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(month);
            }

            var billingConsumptionsData = _service.getCustBillConsumptionAsync(contractId).Result;
            var fetchData = new List<ConsumptionData>();
            foreach (var item in billingConsumptionsData)
            {
                fetchData.Add(new ConsumptionData
                {
                    Period = item.month,
                    Value = Convert.ToDouble(item.amount)
                });
            }
            //return Json(data);

            //var labels = new[] { "Jan", "Feb", "Mar", "Apr", "May" ,"Jun", "Jul","Aug","Sept","Oct","Nov","Dec" };
            //var data = new[] { 100, 500, 300, 800, 200,300,400,600,1000,600,900,100 };

            var labels = fetchData.Select(x =>
            {
                int month = Convert.ToInt32(x.Period);
                string monthName = GetMonthName(month);
                return $"{monthName}";
            }).ToList();

            var values = fetchData.Select(x => x.Value).ToList();

            return Json(new
            {
                labels = labels,
                data = values
            });

        }

        [HttpPost]
        public JsonResult EsclateTicket(string ticketId)
        {
            var resp = _service.ticketEscalationCheckAsync(ticketId).Result;
            return Json(new { success = true, result = resp });
        }

        [HttpPost]
        public JsonResult UserDecison(string ticketId, string userInput)
        {
            var resp = _service.ticketEscalationAsync(ticketId, userInput).Result;
            return Json(new { success = true, result = resp });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult DownloadPdfBillingReceipt(string eag, string invoiceDate)
        {

            var dateCustom = Convert.ToDateTime(invoiceDate);
            string base64String = _service.saveBillingReportAsync(eag, dateCustom.Month, dateCustom.Year).Result; // Replace with your actual logic

            if (base64String == "SalesID not found")
            {
                return Json("SalesID not found");
            }
            else if (base64String.Length > 200)
            {
                byte[] fileBytes = Convert.FromBase64String(base64String);
                return File(fileBytes, "application/pdf", "document.pdf");
            }
            else
            {
                return Json("File Not Found." + base64String);
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult getpaymentReceiptPDF(string contract, string date)
        {
            // ("EAG-000258", "3/31/2018")
            //date = "31/3/2018";
            //contract = "EAG-000258";
            var dateCustom = Convert.ToDateTime(date).ToString("MM/dd/yyyy");
            string base64String = _service.savePaymentReceiptReportAsync(contract, dateCustom).Result; // Replace with your actual logic

            if (base64String == "Voucher not found" || base64String.Length < 200)
            {
                return Json("Voucher not found"+ base64String);
            }
            else
            {
                byte[] fileBytes = Convert.FromBase64String(base64String);
                return File(fileBytes, "application/pdf", "document.pdf");
            }

        }

        private List<AxPayments> GetPaymentsAccordingToContract(string userId, string username, string contractId)
        {
            if (string.IsNullOrEmpty(contractId))
                throw new Exception("Contract Id cannot be null");

            TempData["ContractId"] = contractId;
            var u = _service.getCustomerByUserIDAsync(username).Result;
            //var paymentsAndInvoices = _service.getCustPaymentsAsync(u, contractId).Result;
            var paymentsAndInvoices = _service.getCustomerTransactionAsync(contractId).Result;
            ViewBag.CustOutstandingBalance = _service.GetCustContractBalanceAsync(userId, contractId).Result;
            var result = new List<AxPayments>();
            foreach (var record in paymentsAndInvoices)
            {
                var i = new AxPayments();
                i.TransactionNumber = record.TransNum;
                i.Amount = double.Parse(record.Amount);
                i.JournalNumber = record.JournalNo;
                i.Date = DateTime.Parse(record.Date, CultureInfo.GetCultureInfo("en-US"));
                i.ContractId = contractId;
                i.PayAgainst = record.PayAgainst;
                i.TransNumberDescription = record.TransNumberDescription;
                
                result.Add(i);
            }

            return result;
        }


        /// <summary>
        /// Displays the account history view for the currently logged-in user.
        /// </summary>
        /// <returns>
        /// Returns a view containing a list of the user's contracts with details such as balance and pending requests.
        /// </returns>
        /// <remarks>
        /// This method retrieves the current user's session data, fetches the associated customer record and related contracts,
        /// then prepares a list of contract details with encoded values and any associated pending property requests.
        /// </remarks>
        public IActionResult AccountHistory()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var username = HttpContext.Session.GetString("UserName");
            var cu = _service.getCustomerByUserIDAsync(username).Result;
            ViewBag.Contract = cu;
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
                    Balance = _service.getCustContractBalanceAsync(HttpUtility.HtmlEncode(c.Customer), HttpUtility.HtmlEncode(c.ContractID)).Result,
                    Status = HttpUtility.HtmlEncode(c.WFstatus),
                    SecurityDeposit = _service.GetAllocatedSDAmtAsync(c.PropertyId).Result,
                    LastPaymentDate = _service.getLastPaymentDateAsync(c.ContractID).Result
                };

                var propertyRequest = _service.getPropertyPendingRequestsAsync(HttpUtility.HtmlEncode(c.PropertyId)).Result;

                if (propertyRequest.Length > 0)
                {
                    a.RequestId = propertyRequest[0].ReqID;
                }


                contractsList.Add(a);
            }
            return View(contractsList);
        }

        /// <summary>
        /// Displays the Complaint form page if the user session is valid.
        /// </summary>
        /// <returns>
        /// Returns the Complaint view if the user is authenticated; otherwise, redirects to the Home page.
        /// </returns>
        public IActionResult Complaint()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var username = HttpContext.Session.GetString("UserName");
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(userId))
            {
                return RedirectPermanent("/Home/Index");
            }
            var complainTypes = _service.GetrequestIDAsync().Result;
            var contracts = _service.getCustContAsync(userId, true).Result;
            if (contracts.Length > 0)
            {
                var complaintHistory = _service.GetComplainTicketHistoryAsync(contracts[0].PropertyId).Result;
                if (complaintHistory == null || complaintHistory.Count() == 0)
                {
                    complaintHistory = new ComplaintHistory[0];
                }
                ViewBag.SuccessMessage = "";
                var viewModal = new ComplaintViewModal()
                {
                    Complaint = new Complaint(),
                    SubTypes = complainTypes,
                    ComplaintHistory = complaintHistory
                };
                return View(viewModal);
            }

            var viewModal1 = new ComplaintViewModal()
            {
                Complaint = new Complaint(),
                SubTypes = complainTypes,
                ComplaintHistory = new ComplaintHistory[0]
            };

            return View(viewModal1);
        }

        /// <summary>
        /// Handles the submission of the Complaint form.
        /// </summary>
        /// <param name="complaint">The complaint data submitted by the user.</param>
        /// <returns>
        /// Returns the Complaint view with the submitted data if model validation fails; 
        /// otherwise, processes the complaint and returns the view.
        /// Redirects to Home page if the session is invalid.
        /// </returns>
        [HttpPost]
        public IActionResult Complaint(Complaint complaint)
        {
            var userId = HttpContext.Session.GetString("UserId");
            var username = HttpContext.Session.GetString("UserName");
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(userId))
            {
                return RedirectPermanent("/Home/Index");
            }
            //if (ModelState.IsValid)
            //{
            var contracts = _service.getCustContAsync(userId, true).Result;
            complaint.Comments += $"        Invoice No: {complaint.HighBillInvoiceNo} Email: {complaint.Email}  Date: {complaint.DateOfPayment}";

            // call the complaint api to post the data.
            var response = _service.CreateComplainTicketAsync(contracts[0].PropertyId, complaint.SubType, complaint.TicketType, complaint.Comments).Result;
            ViewBag.SuccessMessage = response;
            var complainTypes = _service.GetrequestIDAsync().Result;
            var complaintHistory = _service.GetComplainTicketHistoryAsync(contracts[0].PropertyId).Result;
            if (complaintHistory == null || complaintHistory.Count() == 0)
            {
                complaintHistory = new ComplaintHistory[0];
            }
            var viewModal = new ComplaintViewModal()
            {
                SubTypes = complainTypes,
                Complaint = complaint,
                ComplaintHistory = complaintHistory
            };
            // }
            return View(viewModal);
        }

        [HttpGet]
        public JsonResult GetSubTypes(string typeId)
        {
            // Replace with real data fetching logic
            var subTypes = _service.getComplainSubtypeAsync(typeId).Result;

            var items = subTypes.Select(x => new SelectListItem
            {
                Value = x.RequestID.ToString(),
                Text = x.Description
            });

            return Json(items);
        }

        [HttpGet]
        public IActionResult PaymentsReceipt()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var username = HttpContext.Session.GetString("UserName");
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(userId))
            {
                return RedirectPermanent("/Home/Index");
            }
            var contracts = LoadContractsForSelection(userId).Result;
            ViewBag.ContractsList = contracts;
            return View();
        }

        [HttpPost]
        public IActionResult PaymentsReceipt(PaymentsReceipt paymentsReceipt)
        {
            var userId = HttpContext.Session.GetString("UserId");
            var username = HttpContext.Session.GetString("UserName");
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(userId))
            {
                return RedirectPermanent("/Home/Index");
            }
            if (string.IsNullOrWhiteSpace(paymentsReceipt.EAG))
            {
                ViewBag.Message = "EAG number is required.";
                return View();
            }
            var response = _service.getBillingReportAsync(paymentsReceipt.EAG, paymentsReceipt.InvoiceDate.Month, paymentsReceipt.InvoiceDate.Year).Result;
            ViewBag.Message = response;

            var contracts = LoadContractsForSelection(userId).Result;
            ViewBag.ContractsList = contracts;
            return View();
        }

        /// <summary>
        /// Submit record proof action
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> SubmitPaymentProof()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var username = HttpContext.Session.GetString("UserName");
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(userId))
            {
                return RedirectPermanent("/Home/Index");
            }
            var projects = await LoadProjectSelection();

            // var units = LoadUnitSelection("test");
            ViewBag.Projects = projects;
            var contracts = await LoadContractsForSelection(userId);
            var viewModal = new ReconnectionViewModal()
            {
                ContractIDs = contracts
            };
            return View(viewModal);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> SubmitPaymentProof(PaymProofTicketRequest form, IFormFile file)
        {
            var userId = HttpContext.Session.GetString("UserId");
            var username = HttpContext.Session.GetString("UserName");
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(userId))
            {
                return RedirectPermanent("/Home/Index");
            }
            var projects = await LoadProjectSelection();

            // var units = LoadUnitSelection("test");
            ViewBag.Projects = projects;
            var contracts = await LoadContractsForSelection(userId);
            var viewModal = new ReconnectionViewModal()
            {
                ContractIDs = contracts
            };
            if (file != null)
            {
                if (file != null && file.Length > 0) //
                {
                    var extension = Path.GetExtension(file.FileName);

                    string fileName;
                    if (!string.IsNullOrEmpty(form.PaymentDate.ToShortDateString()))
                    {
                        fileName = $"{form.EAG}-{form.PaymentDate:dd-MMM-yyyy}{extension}";
                    }
                    else
                    {
                        fileName = $"{form.EAG}{extension}";
                    }

                    using (var memoryStream = new MemoryStream())
                    {
                        file.CopyTo(memoryStream);
                        var byteArray = memoryStream.ToArray();
                        var data = Convert.ToBase64String(byteArray);

                        var resp = _service.createPaymProofTicketAsync(form.EAG, form.Invoice, form.PaymentDate.ToString("dd-MMM-yyyy"), form.PaymentMethod, data).Result;
                        TempData["Message"] = resp;
                    }
                }
            }

            return View(viewModal);
        }

    }
}
