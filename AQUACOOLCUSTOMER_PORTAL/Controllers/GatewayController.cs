using AQUACOOLCUSTOMER_PORTAL.DTO;
using AQUACOOLCUSTOMER_PORTAL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DotNet.Scaffolding.Shared.CodeModifier.CodeChange;
using Newtonsoft.Json;
using RestSharp;
using ServiceReference1;
using System.Collections;
using System.Globalization;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace AQUACOOLCUSTOMER_PORTAL.Controllers
{
    public class GatewayController : Controller
    {
        string debugData = "";
        string SECURE_SECRET = "F01A2A60F2FBC43716945B7F1FDA0E6E";
        SortedList transactionData = new SortedList(new VPCStringComparer());
        private readonly ILogger<AdminController> _logger;
        private ServiceReference1.Service1SoapClient _service;
        //private static readonly HttpClient client = new HttpClient();
        private static  IConfiguration _configurations;
        public GatewayController(ILogger<AdminController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _service = new Service1SoapClient(Service1SoapClient.EndpointConfiguration.Service1Soap);
            _configurations = configuration;
        }
        #region Disclaimer

        public ActionResult Index()
        {
            //string strToken = getAccessToken();
            //TempData["token"] = strToken;
            var id = TempData["TicketId"];
            var amount = TempData["Amount"];
            var contract = TempData["ContractId"];
            if (_service.EnableCCPayOptionAsync(Convert.ToString(contract)).Result.ToString().Trim().ToLower().Equals("yes"))
            {
                ViewBag.Id = id;
                ViewBag.Contract = contract;
                ViewBag.Amount = TempData["Amount"];
                string token = GetAccessTokenAsync().Result;
                //ViewBag.Token = token;
                return View("Disclaimer");
            }
            else
            {
                return RedirectToAction("UserProfile", "Admin");
            }
        }

        [HttpPost]
        public ActionResult Disclaimer(PaymentRequestModel form, string submit)
        {
            if (submit == "Denied")
            {
                return RedirectToAction("UserProfile", "User2");
            }

            //TempData["Form"] = form;
            return RedirectToAction("VPC_DO",form);
        }
        #endregion

        #region Ngenius Functions
        
        public async Task<string> GetAccessTokenAsync()
        {
            //sandbox
            // var client = new RestClient("https://api-gateway.sandbox.ngenius-payments.com/identity/auth/access-token");
            //request.AddHeader("Authorization", "Basic OWI5NTk4NzUtN2UzZi00NTFiLTlkOTEtOTExY2I4MTk2MWE3OmRlYzFmZDAwLTQ4NWMtNGM0OS1iZTlmLWIyYzNmODNkZmFlMA==");
            //request.AddParameter("application/vnd.ni-identity.v1+json", "{\"realmName\":\"ni\"}", "application/vnd.ni-identity.v1+json", ParameterType.RequestBody);
           //var  client = new HttpClient();

           // //string url = "https://api-gateway.ngenius-payments.com/identity/auth/access-token"; // live
             string accessToken = string.Empty;

           // // Request body
           // var jsonBody = "{\"realmName\":\"networkinternational\"}";
           // var content = new StringContent(jsonBody, Encoding.UTF8, "application/vnd.ni-identity.v1+json");
            
            try
            {
                var client = new HttpClient();
#if DEBUG
                var request = new HttpRequestMessage(HttpMethod.Post, "https://api-gateway.sandbox.ngenius-payments.com/identity/auth/access-token");
                request.Headers.Add("Authorization", "Basic OWI5NTk4NzUtN2UzZi00NTFiLTlkOTEtOTExY2I4MTk2MWE3OmRlYzFmZDAwLTQ4NWMtNGM0OS1iZTlmLWIyYzNmODNkZmFlMA==");
#else
                var request = new HttpRequestMessage(HttpMethod.Post, "https://api-gateway.ngenius-payments.com/identity/auth/access-token");
                request.Headers.Add("Authorization", "Basic YzJjMDdhMGMtOGI5ZS00OGU0LWI0NjktYWM5M2E2MDkyZGRiOmFhMGVjNTI1LTI4YTctNDNiOS1hNTdiLTc0MTJmZWMyN2VjMA==");
#endif

                var content = new StringContent("", null, "application/vnd.ni-identity.v1+json");
                request.Content = content;
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                //// Send the POST request
                //var response = await client.PostAsync(url, content);

                //// Ensure the response status is OK (200)
                //response.EnsureSuccessStatusCode();

                // Read the response content
                var responseBody = await response.Content.ReadAsStringAsync();

                // Deserialize the response
                var jsonSerializer = new DataContractJsonSerializer(typeof(PayResponse));
                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(responseBody)))
                {
                    var payResponse = (PayResponse)jsonSerializer.ReadObject(stream);
                    accessToken = payResponse.access_token;
                }
            }
            catch (HttpRequestException e)
            {
                accessToken = $"Request error: {e.Message}";
            }
            catch (Exception e)
            {
                accessToken = $"General error: {e.Message}";
            }

            return accessToken;
        }


        private AxCustomer ConvertAxCustomer(getCusRegDet[] existing)
        {
            var e = existing[0];
            var customer = new AxCustomer
            {
                FirstName = HttpUtility.HtmlEncode(e.FirstName),
                LastName = HttpUtility.HtmlEncode(e.LastName),
                Address = HttpUtility.HtmlEncode(e.Street),
                EmirateName = HttpUtility.HtmlEncode(e.City == "0" ? "" : e.City),
                EmailAddress1 = HttpUtility.HtmlEncode(e.Email)
            };
            return customer;
        }
        public async Task<string> SetPaymentLinkAsync(string accessToken, int paramAmount, string refNumber, string email, string firstName, string lastName, string address, string city)
        {
            // string url = "https://api-gateway.ngenius-payments.com/transactions/outlets/e8a2bb33-73cb-4a98-b2f5-9bb6b0e9b683/orders"; // live
            //string url = "https://api-gateway.sandbox.ngenius-payments.com/transactions/outlets/4fbfb037-de3d-4956-b4ec-482418c01b57/orders";
#if DEBUG
            string url = "https://api-gateway.sandbox.ngenius-payments.com/transactions/outlets/4fbfb037-de3d-4956-b4ec-482418c01b57/orders";
#else
            string url = "https://api-gateway.ngenius-payments.com/transactions/outlets/e8a2bb33-73cb-4a98-b2f5-9bb6b0e9b683/orders"; // live
#endif

            string returnURL = "";
            string paymentLink = "";

            try
            {
                returnURL = _configurations["AppSettings:ngenius_ReturnURL"];
            }
            catch (Exception)
            {
                // Handle configuration exceptions if needed
            }

            // Construct the JSON data body
            var data = new
            {
                action = "SALE",
                amount = new
                {
                    currencyCode = "AED",
                    value = paramAmount
                },
                language = "en",
                emailAddress = email,
                merchantOrderReference = refNumber,
                merchantAttributes = new
                {
                    redirectUrl = returnURL,
                    skipConfirmationPage = true
                },
                billingAddress = new
                {
                    firstName = firstName,
                    lastName = lastName,
                    address1 = address,
                    city = city,
                    countryCode = "UAE"
                }
            };

            string jsonData = JsonConvert.SerializeObject(data);
            var client = new HttpClient();
            // Set up the HTTP request
            var requestContent = new StringContent(jsonData, Encoding.UTF8, "application/vnd.ni-payment.v2+json");
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Accept", "application/vnd.ni-payment.v2+json");
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            try
            {
                // Send the POST request
                var response = await client.PostAsync(url, requestContent);

                // Ensure the response status is OK (200)
                response.EnsureSuccessStatusCode();

                // Read the response content
                var responseBody = await response.Content.ReadAsStringAsync();
                dynamic dynamicData = JsonConvert.DeserializeObject(responseBody);

                // Extract the payment link
                paymentLink = dynamicData?._links?.payment?.href ?? "Ngenius-FailedPayment-NoLink";
            }
            catch (HttpRequestException e)
            {
                paymentLink = $"Ngenius-FailedPayment-{e.Message}";
            }
            catch (Exception e)
            {
                paymentLink = $"Ngenius-FailedPayment-{e.Message}";
            }

            return paymentLink;
        }
#endregion

        #region VPC_DO
        
        //[HttpGet]
        //public ActionResult VPC_DO(string response= "")
        //{
        //    ViewBag.Url = response;
        //    return View();
        //}
        
        public ActionResult VPC_DO(PaymentRequestModel form)
        {
            var userId = HttpContext.Session.GetString("UserId");
            //var username = "ryankakoon@gmail.com";
            var username = HttpContext.Session.GetString("UserName");
            if (string.IsNullOrWhiteSpace(username))
            {
                return RedirectToAction("index", "Admin");
            }
            // IFormCollection form = TempData["Form"] as IFormCollection;
            if (form == null)
                return RedirectToAction("UserProfile", "Admin");

            string message = "No Messages";
            string ticketId = form.Vpc_MerchTxnRef; // form["vpc_MerchTxnRef"].ToString();
            double amount = double.Parse(form.Vpc_Amount.ToString());
            //var amount = double.Parse(client.getMoveOutBalanceTicket(ticketId));
            //if (amount == 0)
            //    amount = double.Parse(client.getBalancebyInvoiceID(ticketId));

            //if (amount == 0)
            //    throw new Exception("Amount cannot be zero");
            var account = _service.getCustomerByUserIDAsync(username).Result;

            var contract = form.Contract;

            var logMessage = "Payment Started:  DateTime    ticket  amount  contract" + Environment.NewLine;
            //Log.WriteLine(logMessage);
            logMessage = $"Payment Started:  {DateTime.Now.ToString(CultureInfo.InvariantCulture)}    {ticketId}  {amount}  {contract}" + Environment.NewLine;
            //Log.WriteLine(logMessage);

            var result = _service.InsertPaymentAsync(account, contract, username,
                amount.ToString(), "", "", "", "", ""
                , "", "", "", "", DateTime.Now.ToString("dd/mm/yyyy"), "", ticketId, "Monthly Bill").Result;


            try
            {
                /*string queryString = "https://migs.mastercard.com.au/vpcpay";

                debugData += "<u>Data from Order Page</u><br/>";

                foreach (string item in form.Keys)
                {
                    debugData += item + "=" + form[item] + "<br/>";

                    if ((form[item] != "") &&
                        (item != "virtualPaymentClientURL") &&
                        (item != "SubButL") &&
                        (item != "Title") &&
                        (item != "vpc_MerchTxnRef") &&
                        item.StartsWith("vpc"))
                    {
                        transactionData.Add(item, form[item]);
                    }
                }*/

                transactionData.Add("vpc_MerchTxnRef", result);
                var pay = int.Parse((amount * 100).ToString());
                transactionData["vpc_Amount"] = pay;
                //transactionData.Add("vpc_Amount", pay);
                /* debugData += "<br/><u>Data from Transaction Sorted List</u><br/>";

                 string rawHashData = SECURE_SECRET;
                 string seperator = "?";

                 foreach (DictionaryEntry item in transactionData)
                 {
                     debugData += item.Key.ToString() + "=" + item.Value.ToString() + "<br/>";

                     queryString += seperator + HttpUtility.UrlEncode(item.Key.ToString()) + "=" +
                                    HttpUtility.UrlEncode(item.Value.ToString());
                     seperator = "&";

                     // Collect the data required for the MD5 sugnature if required
                     if (SECURE_SECRET.Length > 0)
                     {
                         rawHashData += item.Value.ToString();
                     }
                 }

                 string signature = "";
                 if (SECURE_SECRET.Length > 0)
                 {
                     signature = CreateSHA256Signature(true);
                     queryString += seperator + "vpc_SecureHash=" + signature;
                     queryString += seperator + "vpc_SecureHashType=SHA256"; // + signature;

                     debugData += "<br/><u>Hash Data Input</u>: " + rawHashData + "<br/><br/><u>Signature Created</u>: " + signature + "<br/>";
                 }*/


                //Added functions for Ngenius Payment Gateway
                string ReturnURL = "";
                string email = ""; string firstname = ""; string lastname = ""; string address = ""; string city = "";
                try
                {
                    ReturnURL = _configurations["AppSettings:ngenius_ReturnURL"];
                    var existing = _service.getCustomerRegDetailsAsync(username).Result;
                    if (existing.Length > 0 && existing[0].ERROR.ToLower() != "yes")
                    {
                        var customer = ConvertAxCustomer(existing);
                        email = customer.EmailAddress1;
                        firstname = customer.FirstName;
                        lastname = customer.LastName;
                        address = customer.Address;
                        city = customer.EmirateName;
                    }
                }
                catch (Exception ex)
                {

                }
                string AccessToken = GetAccessTokenAsync().Result;
                string PayLink = SetPaymentLinkAsync(AccessToken, pay, result, email, firstname, lastname, address, city).Result;
                //#if DEBUG
                //                debugData = "<u>Data from Transaction Sorted List</u><br/><br/>";
                //                debugData += "AccessToken:"+ AccessToken+ "<br/><br/>";
                //                debugData += "merchantOrderReference : " + result + " <br/><br/>";
                //                debugData += "action: SALE <br> amount : { currencyCode : AED, value : " + (pay/100) + "} <br> language : en <br>emailAddress : "+ email +"<br>merchantAttributes : { redirectUrl : " + ReturnURL + ",skipConfirmationPage : true}<br>";
                //                debugData += "billingAddress: {firstName: "+firstname+", lastName: "+lastname+",address1: "+address+", city: "+city+",countryCode: UAE }";
                //                ViewBag.Url = PayLink;
                //                if (PayLink.Contains("Ngenius-FailedPayment-"))
                //                {
                //                    debugData += "<br/><u>Payment Link Failed To Create</u>: " + PayLink + "<br/><br/>";
                //                }
                //                else
                //                {
                //                    debugData += "<br/><br/><a href=\'" + PayLink + "'>NGenius - Click here to proceed.</a><br/>";
                //                }

                //                //End Ngenius


                //#else
                Response.Redirect(PayLink);
                // uncomment above code in live               

                //#endif

            }
            catch (Exception ex)
            {
                message = "(51) Exception encountered. " + ex.Message;
                if (ex.StackTrace.Length > 0)
                {
                    ViewBag.Error = ex.ToString();
                }
            }
            debugData += "<br/><br/><u>Final QueryString Data String</u><br/>";

            ViewBag.DebugData = debugData;
            return View(form);
        }

        private string CreateSHA256Signature(bool useRequest)
        {
            byte[] convertedHash = new byte[SECURE_SECRET.Length / 2];
            for (int i = 0; i < SECURE_SECRET.Length / 2; i++)
            {
                convertedHash[i] = (byte)Int32.Parse(SECURE_SECRET.Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber);
            }

            StringBuilder sb = new StringBuilder();
            SortedList list = transactionData;
            foreach (DictionaryEntry kvp in list)
            {
                if (kvp.Key.ToString().StartsWith("vpc_") || kvp.Key.ToString().StartsWith("user_"))
                    sb.Append(kvp.Key + "=" + kvp.Value + "&");
            }
            if (sb.Length > 0)
                sb.Remove(sb.Length - 1, 1);

            string hexHash = "";
            using (HMACSHA256 hasher = new HMACSHA256(convertedHash))
            {
                byte[] hashValue = hasher.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()));
                foreach (byte b in hashValue)
                {
                    hexHash += b.ToString("X2");
                }
            }
            return hexHash;
        }

        #endregion

        #region VPC_DR

        public ActionResult VPC_DR()
        {
            //VPCRequest vpc = new VPCRequest();
            //string rawHashData = SECURE_SECRET;
            //try
            //{
            //    debugData = "<br/><u>Start of Debug Data</u><br/><br/>";

            //    if (Request.QueryString["vpc_SecureHash"].Length > 0)
            //    {
            //        debugData += "<u>Data from Payment Server</u><br/>";

            //        foreach (string item in Request.QueryString)
            //        {
            //            debugData += item + "=" + Request.QueryString[item] + "<br/>";

            //            if (SECURE_SECRET.Length > 0 && !item.Equals("vpc_SecureHash"))
            //            {
            //                rawHashData += Request.QueryString[item];
            //            }
            //        }
            //    }

            //    string signature = "";
            //    if (SECURE_SECRET.Length > 0)
            //    {
            //        var output = vpc.Process3PartyResponse(Request.QueryString);
            //        debugData += "<br/><u>Hash Data Input</u>: " + rawHashData + "<br/><br/><u>Signature Created</u>: " + signature + "<br/>";

            //        if (true) //output)
            //        {
            //            ViewBag.Error = "Hash Validated.";
            //            string result = "";

            //            var transactionCode = Request.QueryString["vpc_MerchTxnRef"].ToString();
            //            var amount = Request.QueryString["vpc_Amount"].ToString();
            //            var transactionNumber = Request.QueryString["vpc_TransactionNo"].ToString();
            //            var authorizeId = Request.QueryString["vpc_AuthorizeId"] ?? "Unknown";
            //            var txnResponseCode = Request.QueryString["vpc_TxnResponseCode"];
            //            var transactionDescription = getResponseDescription(txnResponseCode);
            //            var trnNo = Request.QueryString["vpc_TransactionNo"].ToString();
            //            var message = Request.QueryString["vpc_Message"].ToString();
            //            var rcptNo = Request.QueryString["vpc_ReceiptNo"] ?? "NA";
            //            var responseCode = Request.QueryString["vpc_AcqResponseCode"] ?? "NA";
            //            string cardType = Request.QueryString["vpc_Card"] ?? "";
            //            string source = Request.QueryString["vpc_BatchNo"] ?? "";


            //            var logMessage = "Payment Complete: DateTime  transactionCode    User.Identity.Name  txnResponseCode" + Environment.NewLine;
            //            Log.WriteLine(logMessage);

            //            logMessage = $"{DateTime.Now.ToString(CultureInfo.InvariantCulture)}   {transactionCode}    {User.Identity.Name}  {txnResponseCode}{Environment.NewLine}";
            //            Log.WriteLine(logMessage);

            //            if (txnResponseCode == "0")
            //            {
            //                var transactionOutput = _service.UpdatePaymentAsync
            //                (transactionCode,
            //                User.Identity.Name,
            //                txnResponseCode,
            //                source,
            //                transactionDescription,
            //                message, trnNo, responseCode, authorizeId, cardType,
            //                DateTime.Now.ToString("dd/mm/yyyy"), rcptNo, "Success").Result;

            //                result = _service.PostPaymentAsync(transactionCode).Result;
            //                TempData["Message"] = "Payment was successfull. " + result;
            //            }
            //            else
            //            {
            //                var transactionOutput = _service.UpdatePaymentAsync
            //                (transactionCode,
            //                User.Identity.Name,
            //                txnResponseCode,
            //                source,
            //                transactionDescription,
            //                message, trnNo, responseCode, authorizeId, cardType,
            //                DateTime.Now.ToString(), rcptNo, "Error").Result;

            //                // Incase of failure don't call Post Payment.
            //                //result = client.PostPayment(transactionCode);
            //                //result = getResponseDescription(txnResponseCode);
            //                ViewBag.Error = transactionDescription + ". Reference Code: " + transactionCode;
            //                TempData["Error"] = transactionDescription + ". Reference Code: " + transactionCode;
            //            }
            //        }
            //        else
            //        {
            //            //ViewBag.Error = "Hash Not Validated";
            //            //TempData["Error"] = "Hash Not Validated";
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    var message = "(51) Exception encountered. " + ex.Message;
            //    TempData["Error"] = message;
            //}

            //ViewBag.DebugData = debugData;

            return RedirectToAction("UserProfile", "User2");
            //return View();
        }


        public ActionResult VPC_Response()
        {
            return View();
        }

        #endregion

        #region Response Description 
        private string getResponseDescription(string vResponseCode)
        {
            /* 
             <summary>Maps the vpc_TxnResponseCode to a relevant description</summary>
             <param name="vResponseCode">The vpc_TxnResponseCode returned by the transaction.</param>
             <returns>The corresponding description for the vpc_TxnResponseCode.</returns>
             */
            string result = "Unknown";

            if (vResponseCode.Length > 0)
            {
                switch (vResponseCode)
                {
                    case "0": result = "Transaction Successful"; break;
                    case "1": result = "Transaction Declined"; break;
                    case "2": result = "Bank Declined Transaction"; break;
                    case "3": result = "No Reply from Bank"; break;
                    case "4": result = "Expired Card"; break;
                    case "5": result = "Insufficient Funds"; break;
                    case "6": result = "Error Communicating with Bank"; break;
                    case "7": result = "Payment Server detected an error"; break;
                    case "8": result = "Transaction Type Not Supported"; break;
                    case "9": result = "Bank declined transaction (Do not contact Bank)"; break;
                    case "A": result = "Transaction Aborted"; break;
                    case "B": result = "Transaction Declined - Contact the Bank"; break;
                    case "C": result = "Transaction Cancelled"; break;
                    case "D": result = "Deferred transaction has been received and is awaiting processing"; break;
                    case "F": result = "3-D Secure Authentication failed"; break;
                    case "I": result = "Card Security Code verification failed"; break;
                    case "L": result = "Shopping Transaction Locked (Please try the transaction again later)"; break;
                    case "N": result = "Cardholder is not enrolled in Authentication scheme"; break;
                    case "P": result = "Transaction has been received by the Payment Adaptor and is being processed"; break;
                    case "R": result = "Transaction was not processed - Reached limit of retry attempts allowed"; break;
                    case "S": result = "Duplicate SessionID"; break;
                    case "T": result = "Address Verification Failed"; break;
                    case "U": result = "Card Security Code Failed"; break;
                    case "V": result = "Address Verification and Card Security Code Failed"; break;
                    default: result = "Unable to be determined"; break;
                }
            }
            return result;
        }

        #endregion
    }
    #region VPCStringComparer

    class VPCStringComparer : IComparer
    {
        public int Compare(Object a, Object b)
        {
            /*
             <summary>Compare method using Ordinal comparison</summary>
             <param name="a">The first string in the comparison.</param>
             <param name="b">The second string in the comparison.</param>
             <returns>An int containing the result of the comparison.</returns>
             */

            // Return if we are comparing the same object or one of the 
            // objects is null, since we don't need to go any further.
            if (a == b) return 0;
            if (a == null) return -1;
            if (b == null) return 1;

            // Ensure we have string to compare
            string sa = a as string;
            string sb = b as string;

            // Get the CompareInfo object to use for comparing
            System.Globalization.CompareInfo myComparer = System.Globalization.CompareInfo.GetCompareInfo("en-US");
            if (sa != null && sb != null)
            {
                // Compare using an Ordinal Comparison.
                return myComparer.Compare(sa, sb, System.Globalization.CompareOptions.Ordinal);
            }
            throw new ArgumentException("a and b should be strings.");
        }
    }

    #endregion
}
