using System.ComponentModel.DataAnnotations;

namespace AQUACOOLCUSTOMER_PORTAL.DTO
{
    public class MoveOutRequestModel
    {
        [Required]
        public string ProjectId { get; set; }

        [Required]
        public string UnitNumber { get; set; }

        [Required]
        public string ContractId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime MoveOutDate { get; set; }

        // Refund method checkboxes
        public string Cheque { get; set; }
        public string Bank { get; set; }

        // Bank info (optional, only required if Bank = true)
        public string ShortName { get; set; }

        [MaxLength(25)]
        public string IBanNo { get; set; }

        [MaxLength(20)]
        public string AccountNumber { get; set; }

        public string SwiftNo { get; set; }

        [MaxLength(6)]
        public string OTP { get; set; }

        public string SubmitButton { get; set; }

        // File upload for NOC
        public IFormFile NOCFile { get; set; }
    }
}
