using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Aquacool.Entities.Models.Portal;
using Aquacool.Web.CustomerService;
using AQUACOOLCUSTOMER_PORTAL.Controllers;
using ServiceReference1;

namespace Aquacool.Web.Controllers
{
    [RoutePrefix("api/data")]
    public class DataController : ApiController
    {
        private readonly ILogger<AdminController> _logger;
        private ServiceReference1.Service1SoapClient _service;
        //private readonly CustomerServiceClient _service;

        public DataController(ILogger<AdminController> logger)
        {
            _logger = logger;
            _service = new Service1SoapClient(Service1SoapClient.EndpointConfiguration.Service1Soap);
        }

        [Route("Test")]
        [HttpGet]
        public object Test()
        {
            //Request.Headers.Add("Accept", "application/json");   
            return _service.GetProjectsAsync().Result;
        }

        [Route("Projects")]
        public object Projects()
        {
            try
            {
                return new { Projects = _service.GetProjectsAsync().Result, Buildings = _service.GetProjectsAsync().Result };
            }
            catch (Exception ex)
            {
                throw ex;
            }
            // BuildingUnits = _service.GetUnits()
        }

        [Route("BuildingUnits")]
        public object BuildingUnits(string projectId, int AccountType)
        {
            return _service.GetUnitsAsync(projectId, AccountType);
        }

        [HttpPost]
        [Route("Create")]
        public object CreateUser(CrmCustomer customer)
        {
            return null;
        }

        [HttpGet]
        [Route("GetRegistrations")]
        public List<CrmCustomer> GetRegistrations(string unitNo, string projectPrefix)
        {
            Request.Headers.Add("Accept", "application/xml");
            return _service.GetAllContracts(unitNo, projectPrefix).ToList();
        }

        [HttpGet]
        [Route("GetAllRegistrations")]
        public List<string> GetAllRegistrations(string type, int m, int y)
        {
            //Request.Headers.Add("Accept", "application/xml");
            List<string> output = new List<string>();
            
            var data = _service.GetAllRegistrations().ToList();
            foreach (var d in data)
            {
                var date = DateTime.Parse(d.RegistrationDate);

                if (date.Month == m && date.Year == y)
                {
                    //PaymentOf|Project|RegistrationCode|ReceiptNo|BaanReference|PropertyNumber|RegistrationDate|Amount|OfflinePayment
                    var Online = false;
                    if (!string.IsNullOrEmpty(d.TransactionStatus))
                    {
                        if (d.TransactionStatus == "Transaction Successful")
                            Online = true;
                        //else
                        //    continue;
                    }

                    if (type == "SDC" && string.IsNullOrEmpty(d.BaanSD))
                    {
                        if (!string.IsNullOrEmpty(d.SDReceipt) && d.SDReceipt != "0")
                        {
                            var sd = $"{type}|{d.ProjectPrefix}|{d.RegistrationCode}|{d.SDReceipt}|{d.BaanReference}|{d.PropertyNumber}|{date.ToString("ddMMyyyy")}|{d.SD}|{Online}|{d.FullName}";
                            output.Add(sd);
                        }
                    }

                    if (type == "ADM" && string.IsNullOrEmpty(d.BaanAdmin))
                    {
                        if (!string.IsNullOrEmpty(d.AdminReceipt) && d.AdminReceipt != "0")
                        {
                            var ad = $"{type}|{d.ProjectPrefix}|{d.RegistrationCode}|{d.AdminReceipt}|{d.BaanReference}|{d.PropertyNumber}|{date.ToString("ddMMyyyy")}|{d.AdminCharges}|{Online}|{d.FullName}";
                            output.Add(ad);
                        }
                    }
                }
            }

            return output;
        }

        [HttpGet]
        [Route("GetAllRegistrationsVAT")]
        public List<string> GetAllRegistrationsVAT(string type, int m = 4, int y = 2018)
        {
            List<string> output = new List<string>();

            var data = _service.GetAllRegistrations().ToList();
            foreach (var d in data)
            {
                var xDate = new DateTime(y, m, 1);
                var date = DateTime.Parse(d.RegistrationDate);
                var diff = (date - xDate).Days;

                //if (date.Month == m && date.Year == y)
                if(diff >= 0)
                {
                    //PaymentOf|Project|RegistrationCode|ReceiptNo|BaanReference|PropertyNumber|RegistrationDate|TotalAmount|VatAmount|PaidAmount|OfflinePayment
                    var Online = false;
                    if (!string.IsNullOrEmpty(d.TransactionStatus))
                    {
                        if (d.TransactionStatus == "Transaction Successful")
                            Online = true;
                        else
                            continue;
                    }

                    if (type == "SDC" && string.IsNullOrEmpty(d.BaanSD))
                    {
                        if (!string.IsNullOrEmpty(d.SDReceipt) && d.SDReceipt != "0")
                        {
                            var sd = $"{type}|{d.ProjectPrefix}|{d.RegistrationCode}|{d.SDReceipt}|{d.BaanReference}|{d.PropertyNumber}|{date.ToString("ddMMyyyy")}|{d.SD}|0|{d.SD}|{Online}|{d.FullName}";
                            output.Add(sd);
                        }
                    }

                    if (type == "ADM" && string.IsNullOrEmpty(d.BaanAdmin))
                    {
                        if (!string.IsNullOrEmpty(d.AdminReceipt) && d.AdminReceipt != "0")
                        {
                            if (Online)
                            {
                                var ad = $"{type}|{d.ProjectPrefix}|{d.RegistrationCode}|{d.AdminReceipt}|{d.BaanReference}|{d.PropertyNumber}|{date.ToString("ddMMyyyy")}|{d.AdminCharges}|{d.VATAmount}|{d.AdminPaid}|{Online}|{d.FullName}";
                                output.Add(ad);
                            }
                            else
                            {
                                var ad = $"{type}|{d.ProjectPrefix}|{d.RegistrationCode}|{d.AdminReceipt}|{d.BaanReference}|{d.PropertyNumber}|{date.ToString("ddMMyyyy")}|{d.AdminCharges}|{d.VATAmount}|{d.AdminPaid - d.SD}|{Online}|{d.FullName}";
                                output.Add(ad);
                            }

                        }
                    }
                }
            }

            return output;
        }


        [HttpGet]
        [Route("UpdateBaanSD")]
        public int UpdateBaanSD(string content)
        {
            return _service.UpdateBaanSD(content);
        }

        [HttpGet]
        [Route("GetRegistrationForBaan")]
        public string GetRegistrationForBaan(string baanReference, string projectPrefix)
        {
            //Request.Headers.Add("Accept", "application/text");
            var c = _service.GetRegistrationForBaan(baanReference, projectPrefix);
            if (c != null)
                return c.PropertyNumber;

            return "0";
        }

        [HttpGet]
        [Route("GetOneRegistration")]
        public CrmCustomer GetOneRegistration(string unitNo, string projectPrefix)
        {
            Request.Headers.Add("Accept", "application/xml");
            var output = _service.GetAllContracts(unitNo, projectPrefix).First();
            return output;
        }

        [HttpGet]
        [Route("UpdateInvoice")]
        public string UpdateInvoice(string invoiceDetails)
        {
            //string invoiceNumber, string contractNumber, string invoiceMonth, 
            //double declaredLoad, double energyConsumption, double dewaSurcharge, double serviceCharge,
            //double others, double previousBalance, double currentMeterReading,
            //double previousMeterReading, double consumptionRate, double conversionFactor,
            //double netConsumption

            return _service.InsertInvoice(invoiceDetails);
        }

        [HttpPost]
        [Route("UpdateInvoice1")]
        public string UpdateInvoice1(string invoiceDetails)
        {
            //string invoiceNumber, string contractNumber, string invoiceMonth, 
            //double declaredLoad, double energyConsumption, double dewaSurcharge, double serviceCharge,
            //double others, double previousBalance, double currentMeterReading,
            //double previousMeterReading, double consumptionRate, double conversionFactor,
            //double netConsumption

            return _service.InsertInvoice(invoiceDetails);
        }

        [HttpGet]
        [Route("GetBaanReceipts")]
        public List<string> GetBaanReceipts()
        {
            return _service.GetBaanReceipts().ToList();
        }

        [HttpGet]
        [Route("PostReceipt")]
        public int PostReceipt(string receiptNumber, string baanDocumentNumber)
        {
            return _service.PostReceipt(receiptNumber, baanDocumentNumber);
        }

        [HttpGet]
        [Route("GetBaanContracts")]
        public List<string> GetBaanContracts()
        {
            return _service.GetContractReceipts().ToList();
        }

        [HttpGet]
        [Route("PostContract")]
        public int PostContract(string contractNumber, string baanDocumentNumber)
        {
            return _service.PostContracts(contractNumber, baanDocumentNumber);
        }


        [HttpGet]
        [Route("GetUnPaidInvoices")]
        public List<string> GetUnPaidInvoices(int type = 1, string dateString = "")
        {
            var list = new List<string>();
            var data = _service.GetUnPaidInvoices(type, dateString).ToList();

            foreach (var d in data)
            {
                var s = $"{d.BaanCode}|{d.CustomerName}|{d.InvoiceNumber}|{d.InvoiceDate.Value.ToString("ddMMyyyy")}|{d.ProjectPrefix}|{d.PropertyNumber}|{d.Registration}|{d.UnitNumber}|{d.Status}|{d.ReceiptDate}";
                list.Add(s);
            }

            return list;
        }

        [HttpGet]
        [Route("GetUnPaidInvoicesFirst")]
        public string GetUnPaidInvoicesFirst()
        {
            var dateString = "";
            var d = _service.GetUnPaidInvoices(1, dateString).FirstOrDefault();
            var s = $"{d.BaanCode}|{d.CustomerName}|{d.InvoiceNumber}|{d.InvoiceDate.Value.ToString("ddMMyyyy")}|{d.ProjectPrefix}|{d.PropertyNumber}|{d.Registration}|{d.UnitNumber}";
            return s;
        }

        [HttpGet]
        [Route("UpdateInvoiceLatePayment")]
        public string UpdateInvoiceLatePayment(string invoiceDetails)
        {
            try
            {
                if (invoiceDetails == null)
                    throw new Exception("Invoice Details are Null");

                _service.UpdateInvoiceLatePayment(invoiceDetails);
                return "Success";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        [HttpGet]
        [Route("DisableLateFees")]
        public string DisableLateFees(string invoiceNumber, int type)
        {
            try
            {
                var invoices = invoiceNumber.Split('|');
                foreach (var s in invoices)
                {
                    _service.DisableLateFees(s, type);
                }
                return "Success";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}

