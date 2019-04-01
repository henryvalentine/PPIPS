

namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;

    public partial class BankObject
    {
        public string TIN { get; set; }
        public string RCNumber { get; set; }
        public System.DateTime DateRegistered { get; set; }
        public int StructureId { get; set; }
        public string UserId  { get; set; }
        public long UserProfileId { get; set; }
        public string PhoneNumber  { get; set; }
        public string  FirstName  { get; set; }
        public string ProductCode { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
    }
}

