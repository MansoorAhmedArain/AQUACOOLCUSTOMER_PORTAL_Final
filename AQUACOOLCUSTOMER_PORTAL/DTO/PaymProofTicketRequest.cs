namespace AQUACOOLCUSTOMER_PORTAL.DTO
{
    public class PaymProofTicketRequest
    {
        public string EAG { get; set; }  // Use the correct type here
        public string Invoice { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentMethod { get; set; }
        public string FilePath { get; set; }
    }
}
