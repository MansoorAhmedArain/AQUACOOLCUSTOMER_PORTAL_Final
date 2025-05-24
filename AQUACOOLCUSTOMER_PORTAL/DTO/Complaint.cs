namespace AQUACOOLCUSTOMER_PORTAL.DTO
{
    /// <summary>
    /// Represents a customer complaint related to billing or payment.
    /// </summary>
    public class Complaint
    {
        /// <summary>
        /// Gets or sets the invoice number associated with a high bill complaint.
        /// </summary>
        public string HighBillInvoiceNo { get; set; }

        public string SubType { get; set; }
        /// <summary>
        /// Gets or sets the invoice number associated with a low bill complaint.
        /// </summary>
        public string LowBillInvoiceNo { get; set; }

        /// <summary>
        /// Gets or sets the customer's email address.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the date of payment related to the complaint.
        /// </summary>
        public string DateOfPayment { get; set; }

        /// <summary>
        /// Gets or sets any additional comments provided by the customer.
        /// </summary>
        public string Comments { get; set; }
    }
}

