using ServiceReference1;

namespace AQUACOOLCUSTOMER_PORTAL.DTO
{
    public class ComplaintViewModal
    {
        public ComplaintViewModal()
        {
            
        }
        public SubTypes[] SubTypes { get; set; } = new SubTypes[0];
        public Complaint Complaint { get; set; }
        public List<ModifiedComplaintHistory> ComplaintHistory { get; set; } = new List<ModifiedComplaintHistory>();
        public ComplaintTicketSteps ComplaintTicketSteps { get; set; } 
    }
}
