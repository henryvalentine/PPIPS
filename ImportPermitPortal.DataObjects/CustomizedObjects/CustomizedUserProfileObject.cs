

using System;
using System.Collections.Generic;

namespace ImportPermitPortal.DataObjects
{

    public partial class UserProfileObject
    {
       public int BankId { get; set; }
       public int DepotId { get; set; }
       public int JettyId { get; set; }
       public int BranchId { get; set; }
       public long CompanyId { get; set; }
       public string UserId { get; set; }
       public string BranchCode { get; set; }
       public string PhoneNumber { get; set; }
       public string BankBranchName { get; set; }
       public string CompanyName { get; set; }
       public DateTime? StartDate { get; set; }
       public DateTime? CompletionDate { get; set; }
       public string StartDateStr { get; set; }
       public string CompletionDateStr { get; set; }
       public string ActionPerformed { get; set; }
       public string Rrr { get; set; }
       public string Role { get; set; }
       public int RoleId { get; set; }
       public string JettyName { get; set; }  
       public long ContactPersonId { get; set; }
       public int StructureId { get; set; }
       public string StatusStr { get; set; }
       public int JobsDone { get; set; }
       public string FirstName { get; set; }
       public string LastName { get; set; }
       public string Name { get; set; }
       public string Password { get; set; }
       public string ConfirmPassword { get; set; }
       public string Email { get; set; }
       public string IssueDateStr { get; set; }
       public string ExpiryDateStr { get; set; }
       public long ImporterId { get; set; }
       public string DepotLicense { get; set; }
       public DateTime? IssueDate { get; set; }
       public DateTime? ExpiryDate { get; set; }
       public bool? Status { get; set; }
       public virtual AspNetUserObject AspNetUserObject { get; set; }
    }

    public partial class UserInfoObject
    {
        public string CompanyName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public string Rrr { get; set; }
        public string Name { get; set; }
        public string StartDateStr { get; set; }
        public string CompletionDateStr { get; set; }
        public string ActionPerformed { get; set; }
        
    }

    public partial class AppInfoObject
    {
        public int Code { get; set; }
        public string CompanyName { get; set; }
        public string Rrr { get; set; }
        public List<UserInfoObject> UserInfoObjects { get; set; }
        public List<ApplicationItemObject> ApplicationItemObjects { get; set; }
    }
}

