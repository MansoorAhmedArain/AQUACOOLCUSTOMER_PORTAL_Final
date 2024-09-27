namespace AQUACOOLCUSTOMER_PORTAL.DTO
{
    public class AxPayments
    {
        public string PayAgainst { get; set; }
        public double Amount { get; set; }
        public DateTime Date { get; set; }
        public string TransactionNumber { get; set; }
        public string JournalNumber { get; set; }
        public string ContractId { get; set; }
    }
}
