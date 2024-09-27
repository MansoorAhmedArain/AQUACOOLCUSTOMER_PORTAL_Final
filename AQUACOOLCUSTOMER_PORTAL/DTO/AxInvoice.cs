namespace AQUACOOLCUSTOMER_PORTAL.DTO
{
    public class AxInvoice
    {
        public string Id { get; set; }
        public double Amount { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime DueDate { get; set; }
        public string ContractId { get; set; }
        public string Description { get; internal set; }
    }
}
