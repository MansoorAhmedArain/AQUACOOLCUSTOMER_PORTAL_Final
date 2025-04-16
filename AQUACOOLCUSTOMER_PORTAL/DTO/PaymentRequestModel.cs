namespace AQUACOOLCUSTOMER_PORTAL.DTO
{
    /// <summary>
    /// Represents the payment request data for the payment gateway.
    /// </summary>
    public class PaymentRequestModel
    {
        /// <summary>
        /// Gets or sets the version of the VPC API.
        /// </summary>
        public string Vpc_Version { get; set; }

        /// <summary>
        /// Gets or sets the command to be executed (e.g., "pay").
        /// </summary>
        public string Vpc_Command { get; set; }

        /// <summary>
        /// Gets or sets the access code provided by the payment gateway.
        /// </summary>
        public string Vpc_AccessCode { get; set; }

        /// <summary>
        /// Gets or sets the merchant ID provided by the payment gateway.
        /// </summary>
        public string Vpc_Merchant { get; set; }

        /// <summary>
        /// Gets or sets the currency code (e.g., "AED").
        /// </summary>
        public string Vpc_Currency { get; set; }

        /// <summary>
        /// Gets or sets the locale of the transaction (e.g., "en_AE").
        /// </summary>
        public string Vpc_Locale { get; set; }

        /// <summary>
        /// Gets or sets the return URL to which the gateway will redirect after processing the payment.
        /// </summary>
        public string Vpc_ReturnURL { get; set; }

        /// <summary>
        /// Gets or sets the optional ticket number for the transaction.
        /// </summary>
        public string Vpc_TicketNo { get; set; }

        /// <summary>
        /// Gets or sets the transaction amount in the smallest unit (e.g., fils if AED).
        /// </summary>
        public string Vpc_Amount { get; set; }

        /// <summary>
        /// Gets or sets the merchant transaction reference number.
        /// </summary>
        public string Vpc_MerchTxnRef { get; set; }

        /// <summary>
        /// Gets or sets the contract identifier or name related to the transaction.
        /// </summary>
        public string Contract { get; set; }
    }

}
