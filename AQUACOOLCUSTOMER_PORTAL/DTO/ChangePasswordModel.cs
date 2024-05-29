using System.ComponentModel.DataAnnotations;

namespace AQUACOOLCUSTOMER_PORTAL.DTO
{   
    public class ChangePasswordModel
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        public string OldPassword { get; set; }

        [Required]
        //[MinLength(6, ErrorMessage = "New Password must be at least 6 characters long.")]
        public string NewPassword { get; set; }
    }
}
