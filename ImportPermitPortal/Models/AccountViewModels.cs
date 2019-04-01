using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Newtonsoft.Json;

namespace ImportPermitPortal.Models
{
    public class ExternalLoginConfirmationViewModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }
    }

    public class ManageUserViewModel
    {
       
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [System.ComponentModel.DataAnnotations.Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public long UserId { get; set; }
    }

    public class LoginViewModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }


    public class AccessModel
    {
        public string Email { get; set; }

        public string Password { get; set; }

        public bool IsExisting { get; set; }
    }


    public class ValidityResponse
    {
        [JsonProperty("response")]
        public string Response { get; set; }
    }

    public class RegisterViewModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
        public string RcCh { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public bool IsImporter { get; set; }
        public string PhoneNumber { get; set; }
        public string ImporterName { get; set; }
        public string TIN { get; set; }
        public bool IsUser { get; set; }
        public string RCNumber { get; set; }
        public int ImportEligibilityId { get; set; }
        public int StructureId { get; set; }
        public List<SelectListItem> Structures { get; set; }
        public List<SelectListItem> ImportEligibilities { get; set; }
         
    } 

    

    public class UserViewModel
    {
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
        public int BankId { get; set; }
        public int ImporterId { get; set; } 
        public long Id { get; set; }
        public long PersonId { get; set; }
        public int BranchId { get; set; }
        public int? Gender { get; set; }
        public string FirstName { get; set; }
        public string BankBranchName { get; set; }
        public string BranchCode { get; set; }
        public bool IsUser { get; set; }
        public bool IsActive { get; set; }
        public bool IsFirstPassword { get; set; }
        public string LastName { get; set; }
        public string SecurityStamp { get; set; }
        public string Email { get; set; }
        public string Nationality { get; set; }
        public string PhoneNumber { get; set; }
        public string Role { get; set; }
        public int RoleId { get; set; }
    }

    public class SupportRequestObject
    {
        public string Title { get; set; }
      
        public string MessageBody { get; set; }
    }
}

