using Microsoft.Build.Evaluation;

namespace AQUACOOLCUSTOMER_PORTAL.DTO
{
    public class ReconnectionViewModal
    {
        public List<Project> Projects { get; set; } = new ();
        public List<ContractIDs> ContractIDs { get; set; } = new ();
    }
}
