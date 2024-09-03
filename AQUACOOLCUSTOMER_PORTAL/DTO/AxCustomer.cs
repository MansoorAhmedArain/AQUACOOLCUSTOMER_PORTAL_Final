using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace AQUACOOLCUSTOMER_PORTAL.DTO
{
    public class AxCustomer
    {
        public string Id { get; set; }

            [DisplayName("First Name"), Required]
            public string FirstName { get; set; }

            [DisplayName("Last Name"), Required]
            public string LastName { get; set; }

            [DisplayName("Customer Name")]
            public string FullName { get; set; }


            [DisplayName("Account"), Required]
            public string Account { get; set; }

            [DisplayName("Id Type")]
            public string IdType { get; set; }

            [DisplayName("Create your Password (This will be used to log in to your account)  (Required)")]
            [Required(ErrorMessage = "Password is Required.")]
            public string Password { get; set; }

            [DisplayName("Confirm your Password (Required)")]
            [Required(ErrorMessage = "Confirm Password is Required")]
            public string ConfirmPassword { get; set; }

            [DisplayName("Company")]
            public string Company { get; set; }

            [DisplayName("TRN")]
            public string TRN { get; set; }

            [DisplayName("Trade License Number")]
            public string TradeLicenseNumber { get; set; }

            [DisplayName("Trade License Expiry Date"), DefaultValue(""), UIHint("Date")]
            public string TradeLicenseExpiryDate { get; set; }

            [DisplayName("Trade License Contract End Date"), DefaultValue(""), UIHint("Date")]
            public string TenancyContractEndDate { get; set; }

            [DisplayName("Trade License Contract Start Date"), DefaultValue(""), UIHint("Date")]
            public string TenancyContractStartDate { get; set; }

            [DisplayName("Landline"), DefaultValue("")]
            public string LandLine { get; set; }

            [DisplayName("P.O. Box"), DefaultValue("")]
            public string PoBox { get; set; }
            [DisplayName("Emirate"), DefaultValue(""), Required]
            public int Emirate { get; set; }

            public bool RegisteredAs { get; set; }


            [DisplayName("Nationality"), DefaultValue(""), Required]
            public string Nationality { get; set; }


            [DisplayName("Address"), DefaultValue("")]
            public string Address { get; set; }

            [DisplayName("Email Address Primary  (Required)"), DefaultValue(""), UIHint("EmailAddress"), Required(ErrorMessage = "Primary Email is Required")]
            public string EmailAddress1 { get; set; }

            [DisplayName("Email Address Secondary"), DefaultValue(""), UIHint("EmailAddress")]
            public string EmailAddress2 { get; set; }


            [DisplayName("Primary Phone Number (Required)"), DefaultValue(""), Required(ErrorMessage = "Primary Mobile Number is Required")]
            public string MobileNumber1 { get; set; }

            [DisplayName("Secondary Phone Number"), DefaultValue("")]
            public string MobileNumber2 { get; set; }

            [DisplayName("Emirates Id Number"), DefaultValue("")]
            public string IdNumber { get; set; }
            [DisplayName("Emirates Id Expiry Date"), DefaultValue(""), UIHint("Date")]
            public string IdExpiryDate { get; set; }

            [Required]
            public string ProjectId { get; set; }

            [Required]
            public string BuildingId { get; set; }

            [Required]
            public string UnitId { get; set; }

            public string BuildingNumber { get; set; }
            public string Phase { get; set; }

            public string OwnerName { get; set; }
            public string ProjectName { get; set; }
            //        public string IdTypeValue { get; set; }

            [DisplayName("Account Number")]
            public string AccountNumber { get; set; }

            [DisplayName("Residency Status")]
            public string Resident { get; set; }


            [DisplayName("Security Deposit")]
            public string SecurityDeposit { get; set; }

            [DisplayName("Admin Charges")]
            public string AdminCharges { get; set; }
            public string UnitNo { get; set; }
            public string BuildingName { get; set; }
            public string UnitType { get; set; }
            public string EmirateName { get; set; }
            public string DeclaredLoad { get; set; }

            [DisplayName("Receipt Number")]
            public string ReceiptNumber { get; set; }

            [DisplayName("Transaction Status")]
            public string TransactionStatus { get; set; }

            [DisplayName("Passport Number")]
            [Required(ErrorMessage = "Passport Number is Required")]
            public string PassportNumber { get; set; }

            [DisplayName("Passport Expiry Date"), UIHint("Date"),
                Required(ErrorMessage = "Passport Expiry Date is Required")]
            public string PassportExpiryDate { get; set; }

            public bool EmailVerified { get; set; }
            public bool OfflinePayment { get; set; }
            public string OfflineDetails { get; set; }
            public string EnergyConsumptionRate { get; set; }
            public string DeclaredLoadUnit { get; set; }
            public string DeclaredLoadAnnual { get; set; }
            public string CustomerStatus { get; set; }
            public string RegistrationId { get; set; }
            public string RegistrationCode { get; set; }
            public string ProjectRegistrationForm { get; set; }
            public string ProjectEUA { get; set; }
            public string ProjectPrefix { get; set; }
            public string BaanReference { get; set; }
            public string PropertyNumber { get; set; }
            public decimal SD { get; set; }
            public decimal AdminFees { get; set; }
            public string SDReceipt { get; set; }
            public string AdminReceipt { get; set; }
            public string BaanSD { get; set; }
            public string BaanAdmin { get; set; }
            public string RegistrationDate { get; set; }
            public decimal VATAmount { get; set; }
            public decimal AdminPaid { get; set; }
            public string AdminChargesWithoutTax { get; set; }
            public string UserID { get; set; }

            public bool Agreement { get; set; }
        }
    }

