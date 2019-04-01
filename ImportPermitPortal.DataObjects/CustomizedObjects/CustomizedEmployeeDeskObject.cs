
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;

    public partial class EmployeeDeskObject
    {


        public string EmployeeName { get; set; }
        public string GroupName { get; set; }
        public string ZoneName { get; set; }
        public string ActivityTypeName { get; set; }

        public string Email { get; set; }
        public string Phone { get; set; }
        public string FirstName { get; set; }

        public string LastName { get; set; }
        public string JobCountStr { get; set; }
        public string StepDescription { get; set; }
    }
}

