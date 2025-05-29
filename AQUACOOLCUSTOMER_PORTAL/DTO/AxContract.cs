namespace AQUACOOLCUSTOMER_PORTAL.DTO
{
    public class AxContract
    {
        public string MainAgreement { get; set; }
        public string PropertyId { get; set; }
        public string Customer { get; set; }
        public string ContractID { get; set; }
        public string RequestId { get; internal set; }
        public string Balance { get; internal set; }
        public string CustomerType { get; set; }
        public string Unit { get; internal set; }
        public string Project { get; internal set; }
        public string Status { get; set; }
    }
}
