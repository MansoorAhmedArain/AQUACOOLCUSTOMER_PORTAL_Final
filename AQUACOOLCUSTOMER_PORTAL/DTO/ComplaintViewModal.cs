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
        public ComplaintHistory[] ComplaintHistory { get; set; } = new ComplaintHistory[0];
    }
}
