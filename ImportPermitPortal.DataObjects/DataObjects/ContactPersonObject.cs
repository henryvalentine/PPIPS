
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;
    
    public partial class ContactPersonObject
    {
        public long ContactPersonId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public long ImporterId { get; set; }

        public virtual ImporterObject ImporterObject { get; set; }
    }
}
