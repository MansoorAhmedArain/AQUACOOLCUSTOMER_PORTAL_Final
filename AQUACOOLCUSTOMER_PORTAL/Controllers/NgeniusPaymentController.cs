using AQUACOOLCUSTOMER_PORTAL.Controllers;
using AQUACOOLCUSTOMER_PORTAL.DTO;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RestSharp;
using ServiceReference1;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web;

namespace AQUACOOLCUSTOMER_PORTAL.Controllers
{
    public class NgeniusPaymentController : Controller
    {
       
        //AxService.Service1SoapClient client = new AxService.Service1SoapClient();
        private readonly ILogger<AdminController> _logger;
        private ServiceReference1.Service1SoapClient _service;
        public NgeniusPaymentController(ILogger<AdminController> logger)
        {
            _logger = logger;
            _service = new Service1SoapClient(Service1SoapClient.EndpointConfiguration.Service1Soap);
        }
        public ActionResult Test() {
            return View();
        }
        [HttpGet]
        public ActionResult Index(string @ref)
        {
            string refNumber = string.Empty;
            if (!string.IsNullOrEmpty(@ref))
            {
                if (TempData["ResponseData"] != null)
                    TempData["ResponseData"] = null;
                refNumber = Convert.ToString(@ref);
                string AccessToken = getAccessToken();

                //Log.WriteLine("Reference Code: " + refNumber + " -- Token: " + AccessToken);

                var Response = RetrievePaymentStatus(refNumber, AccessToken);
                TempData["ResponseData"] = Response;
            }
            return RedirectToAction("NgeniusThankYou");
        }

        private string RetrievePaymentStatus(string reference, string AccessToken)
        {
            StringBuilder strResponseData = new StringBuilder();
            string br = "<br />";
            string Paylink = "";
            bool IsSuccess = true;
            string url = "";

#if DEBUG
            // Choose environment
            string baseUrl = "https://api-gateway.sandbox.ngenius-payments.com"; // Sandbox
            string requestUri = $"{baseUrl}/transactions/outlets/4fbfb037-de3d-4956-b4ec-482418c01b57/orders/{reference}";
#else
            string baseUrl = "https://api-gateway.ngenius-payments.com"; // Live
            string requestUri = $"{baseUrl}/transactions/outlets/e8a2bb33-73cb-4a98-b2f5-9bb6b0e9b683/orders/{reference}";
#endif



            // TLS & connection settings (still honored at the AppDomain level)
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.DefaultConnectionLimit = 9999;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.ni-payment.v2+json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);

                HttpResponseMessage response = client.GetAsync(requestUri).Result;
                string responseContent = response.Content.ReadAsStringAsync().Result;

                if (response.IsSuccessStatusCode)
                {
                    strResponseData.Append("Request succeeded:" + br);
                    strResponseData.Append(responseContent);
                }
                else
                {
                    strResponseData.Append("Request failed:" + br);
                    strResponseData.Append($"Status Code: {response.StatusCode}" + br);
                    strResponseData.Append(responseContent);
                }
            

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                string Content = response.Content.ReadAsStringAsync().Result;
                string merchantTransactionCode = "";
                Content = Content.Replace("3ds", "ThirdDS");
                dynamic DynamicData = JsonConvert.DeserializeObject(Content);

                try
                {

                    if (DynamicData._id != null)
                    {
                        string state = DynamicData._embedded.payment[0].state.ToString() ?? "";
                        merchantTransactionCode = DynamicData.merchantOrderReference.ToString();
                        if (state.ToLower() == "failed")
                        {
                            Paylink = "<h3><b><u>Transaction Failed</u></b></h3><br/><br/>";
                        }
                        else
                        {
                            Paylink = "<h3><u>Successful Transaction</u></h3><br/><br/>";
                        }
                        /* try
                         {
                             //strResponseData.Append("<b><u>Payment gateway Response</u></b>").Append(br);
                             //strResponseData.AppendFormat("_id : {0}", DynamicData._id).Append(br);
                             //strResponseData.AppendFormat("_links : {0}", DynamicData._links).Append(br);
                             //strResponseData.AppendFormat("type : {0}", DynamicData.type).Append(br);
                             //strResponseData.AppendFormat("merchantDefinedData : {0}", DynamicData.merchantDefinedData).Append(br);
                             //strResponseData.AppendFormat("action : {0}", DynamicData.action).Append(br);
                             //strResponseData.AppendFormat("amount : {0}", DynamicData.amount).Append(br);
                             //strResponseData.AppendFormat("language : {0}", DynamicData.language).Append(br);
                             //strResponseData.AppendFormat("merchantAttributes : {0}", DynamicData.merchantAttributes).Append(br);


                             //strResponseData.AppendFormat("emailAddress : {0}", DynamicData.emailAddress).Append(br);
                             //strResponseData.AppendFormat("reference : {0}", DynamicData.reference).Append(br);
                             //strResponseData.AppendFormat("outletId : {0}", DynamicData.outletId).Append(br);
                             //strResponseData.AppendFormat("createDateTime : {0}", DynamicData.createDateTime).Append(br);
                             //strResponseData.AppendFormat("paymentMethods : {0}", DynamicData.paymentMethods).Append(br);
                             //strResponseData.AppendFormat("referrer : {0}", DynamicData.referrer).Append(br);
                             //strResponseData.AppendFormat("formattedAmount : {0}", DynamicData.formattedAmount).Append(br);
                             //strResponseData.AppendFormat("formattedOrderSummary : {0}", DynamicData.formattedOrderSummary).Append(br);
                             //strResponseData.AppendFormat("_embedded.payment._id : {0}", DynamicData._embedded.payment[0]._id).Append(br);
                             //strResponseData.AppendFormat("_embedded.payment._links.curies : {0}", DynamicData._embedded.payment[0]._links.curies[0]).Append(br);
                             //strResponseData.AppendFormat("_embedded.payment.paymentMethod : {0}", DynamicData._embedded.payment[0].paymentMethod).Append(br);
                             //strResponseData.AppendFormat("_embedded.payment.state : {0}", DynamicData._embedded.payment[0].state).Append(br);
                             //if (state.ToLower() != "failed")
                             //{
                             //    strResponseData.AppendFormat("_embedded.payment.savedCard : {0}", DynamicData._embedded.payment[0].savedCard).Append(br);
                             //    strResponseData.AppendFormat("_embedded.payment.authResponse : {0}", DynamicData._embedded.payment[0].authResponse).Append(br);
                             //}
                             //strResponseData.AppendFormat("_embedded.payment.amount : {0}", DynamicData._embedded.payment[0].amount).Append(br);
                             //strResponseData.AppendFormat("_embedded.payment.updateDateTime : {0}", DynamicData._embedded.payment[0].updateDateTime).Append(br);
                             //strResponseData.AppendFormat("_embedded.payment.outletId : {0}", DynamicData._embedded.payment[0].outletId).Append(br);
                             //strResponseData.AppendFormat("_embedded.payment.orderReference : {0}", DynamicData._embedded.payment[0].orderReference).Append(br);
                             //strResponseData.AppendFormat("_embedded.payment.3ds : {0}", DynamicData._embedded.payment[0].ThirdDS).Append(br);
                             //if (DynamicData._embedded.payment[0].authResponse != null)
                             //{
                             //    strResponseData.AppendFormat("_embedded.payment[0].authResponse.resultCode : {0}", DynamicData._embedded.payment[0].authResponse["resultCode"].ToString()).Append(br);

                             //    strResponseData.AppendFormat("_embedded.payment[0].authResponse.success : {0}", DynamicData._embedded.payment[0].authResponse.success.ToString()).Append(br);
                             //    strResponseData.AppendFormat("_embedded.payment[0].authResponse.authorizationCode : {0}", DynamicData._embedded.payment[0].authResponse.authorizationCode.ToString() ?? "Unknown").Append(br);
                             //    strResponseData.AppendFormat("_embedded.payment[0].authResponse.resultMessage : {0}", DynamicData._embedded.payment[0].authResponse.resultMessage.ToString()).Append(br);
                             //    strResponseData.AppendFormat("_embedded.payment[0].authResponse.resultCode : {0}", DynamicData._embedded.payment[0].authResponse.resultCode.ToString()).Append(br);
                             //    strResponseData.AppendFormat("_embedded.payment[0].authResponse.rrn : {0}", DynamicData._embedded.payment[0].authResponse.rrn.ToString()).Append(br);
                             //    strResponseData.AppendFormat("_embedded.payment[0].paymentMethod.name : {0}", DynamicData._embedded.payment[0].paymentMethod.name.ToString()).Append(br);
                             //}
                             //#if DEBUG
                             //Paylink += strResponseData.ToString();
                             //#endif
                         }
                         catch (Exception ex)
                         {
                             Paylink += ex.ToString()+ "Error in strResponseData";
                         }*/

                        #region Save Data
                        try
                        {
                            ViewBag.Error = "Hash Validated.";
                            string result = "";
                            var transactionCode = DynamicData.reference.ToString();
                            var amount = DynamicData.amount.value / 100;
                            var transactionNumber = DynamicData.reference.ToString();
                            var authorizeId = "";
                            var message = "";
                            var txnResponseCode = "";
                            var responseCode = "";
                            var rcptNo = "NA";//hold
                            string source = "";//hold

                            if (DynamicData._embedded.payment[0].authResponse != null)
                            {
                                txnResponseCode = DynamicData._embedded.payment[0].authResponse.resultCode.ToString();
                                // if (DynamicData._embedded.payment[0].authResponse.success.ToString() == "true")
                                if (state.ToLower() != "failed")
                                {
                                    authorizeId = DynamicData._embedded.payment[0].authResponse.authorizationCode.ToString();
                                }
                                message = DynamicData._embedded.payment[0].authResponse.resultMessage.ToString();
                                responseCode = DynamicData._embedded.payment[0].authResponse.resultCode.ToString();
                                rcptNo = DynamicData._embedded.payment[0].authResponse.rrn.ToString();
                                source = DynamicData._embedded.payment[0].paymentMethod.name.ToString();
                            }
                            var transactionDescription = getResponseDescription(txnResponseCode);
                            var trnNo = DynamicData.reference.ToString();
                            string cardType = DynamicData._embedded.payment[0].paymentMethod.name.ToString() ?? "";
                            try
                            {

                                var logMessage = "Payment Complete: DateTime  transactionCode    User.Identity.Name  txnResponseCode" + Environment.NewLine;
                                // Log.WriteLine(logMessage);

                                logMessage = $"{DateTime.Now.ToString(CultureInfo.InvariantCulture)}   {merchantTransactionCode}    {User.Identity.Name}  {txnResponseCode}{Environment.NewLine}";
                                //Log.WriteLine(logMessage);

                                if (txnResponseCode == "00")
                                {
                                    var transactionOutput = _service.UpdatePaymentAsync
                                    (merchantTransactionCode,
                                    User.Identity.Name,
                                    txnResponseCode,
                                    source,
                                    transactionDescription,
                                    message, trnNo, responseCode, authorizeId, cardType,
                                    DateTime.Now.ToString("dd/mm/yyyy"), rcptNo, "Success").Result;

                                    result = _service.PostPaymentAsync(merchantTransactionCode).Result;
                                    TempData["Message"] = "Payment was successfull. " + result;

                                    if (TempData["Message"] != null)
                                    {
                                        Paylink += "<div class='alert alert-success'>" + TempData["Message"].ToString() + "<br/><br/>Thank you for your order. Your order reference number is : " + merchantTransactionCode + "</div>";
                                    }
                                    Paylink += "<p>Click <a href='" + url + "'>here</a> to go back to your profile.</p><br/><br/>";//+Content
                                }
                                else
                                {
                                    var transactionOutput = _service.UpdatePaymentAsync
                                    (merchantTransactionCode,
                                    User.Identity.Name,
                                    txnResponseCode,
                                    source,
                                    transactionDescription,
                                    message, trnNo, responseCode, authorizeId, cardType,
                                    DateTime.Now.ToString(), rcptNo, "Error").Result;

                                    // Incase of failure don't call Post Payment.
                                    //result = client.PostPayment(transactionCode);
                                    //result = getResponseDescription(txnResponseCode);
                                    ViewBag.Error = transactionDescription + ". Reference Code: " + merchantTransactionCode;
                                    TempData["Error"] = transactionDescription + ". Reference Code: " + merchantTransactionCode;

                                    if (TempData["Error"] != null)
                                    {
                                        Paylink += "<div class='alert alert-danger'>" + TempData["Error"].ToString() + "</div><br/><br/>";
                                    }
                                    Paylink += "<p>Click <a href='" + url + "'>here</a> to go back to your profile and try again.</p>";
                                }
                            }
                            catch (Exception ex)
                            {
                                Paylink += "<p>Something went wrong!</p>" + ex.ToString();
                            }
                        }
                        catch (Exception ex)
                        {
                            Paylink += "<p>Error in save data</p>" + Content + "<br/><br/>" + ex.ToString();
                        }
                        #endregion
                    }
                    else
                    {
                        Paylink += "<p>Something went wrong! Click <a href='" + url + "'>here</a> to go back to your profile and try again.</p>" + Content;
                    }
                }
                catch (Exception ex)
                {
                    Paylink += "<p>Something went wrong! Click <a href='" + url + "'>here</a> to go back to your profile and try again.</p>" + Content + "<br/><br/>" + ex.ToString();
                }
            }

            return Paylink; 
        }
        }


        private string getAccessToken()
        {
            string accessToken = string.Empty;

#if DEBUG
            // Choose environment
            string authUrl = "https://api-gateway.sandbox.ngenius-payments.com/identity/auth/access-token"; // Sandbox
            string authorization = "Basic OWI5NTk4NzUtN2UzZi00NTFiLTlkOTEtOTExY2I4MTk2MWE3OmRlYzFmZDAwLTQ4NWMtNGM0OS1iZTlmLWIyYzNmODNkZmFlMA==";
#else
            string authUrl = "https://api-gateway.ngenius-payments.com/identity/auth/access-token"; // Live
            string authorization = "Basic YzJjMDdhMGMtOGI5ZS00OGU0LWI0NjktYWM5M2E2MDkyZGRiOmFhMGVjNTI1LTI4YTctNDNiOS1hNTdiLTc0MTJmZWMyN2VjMA==";
#endif
            try
            {
                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Post, authUrl);
                request.Headers.Add("Authorization", authorization);
                var content = new StringContent("", null, "application/vnd.ni-identity.v1+json");
                request.Content = content;
                var response = client.SendAsync(request).Result;
                response.EnsureSuccessStatusCode();
                //// Send the POST request
                //var response = await client.PostAsync(url, content);

                //// Ensure the response status is OK (200)
                //response.EnsureSuccessStatusCode();

                // Read the response content
                var responseBody = response.Content.ReadAsStringAsync().Result;

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

        #region Response Description 
        private string getResponseDescription(string vResponseCode)
        {
            /* 
             <summary>Maps the vpc_TxnResponseCode to a relevant description</summary>
             <param name="vResponseCode">The vpc_TxnResponseCode returned by the transaction.</param>
             <returns>The corresponding description for the vpc_TxnResponseCode.</returns>
             */
            string result = "Transaction Failed";

            if (vResponseCode.Length > 0)
            {
                switch (vResponseCode)
                {
                    case "00": result = "Transaction Successful"; break;
                    case "05": result = "Transaction Declined - Do not honor"; break;
                    case "41": result = "Transaction Declined - Lost Card"; break;
                    case "43": result = "Transaction Declined - Stolen card"; break;
                    case "14": result = "Transaction Declined - Invalid account number"; break;
                    case "51": result = "Transaction Declined - Insufficient funds"; break;
                    case "55": result = "Transaction Declined - Incorrect PIN"; break;
                    case "54": result = "Transaction Declined - Expired card"; break;
                    default: result = "Transaction Failed"; break;
                }
            }
            return result;
        }

        #endregion
        public ActionResult NgeniusThankYou()
        {
            return View();
        }

    }
}