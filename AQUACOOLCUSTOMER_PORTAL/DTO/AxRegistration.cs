namespace AQUACOOLCUSTOMER_PORTAL.DTO
{

    public class AxRegistration
    {
        public string RequestId { get; set; }
        public string UserId { get; set; }
        public string ProjectId { get; set; }
        public string MoveInAs { get; set; }
        public string PropertyId { get; set; }
        public DateTime ContractStartDate { get; set; }
        public DateTime ContractEndDate { get; set; }
        public bool Agreement { get; set; }
        //public bool Acknowledge { get; set; }
    }
}
