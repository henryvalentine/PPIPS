//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ImportPermitPortal.EF.Model
{
    using System;
    using System.Collections.Generic;
    
    public partial class NotificationInspectionQueue
    {
        public long Id { get; set; }
        public long NotificationId { get; set; }
        public long EmployeeId { get; set; }
        public Nullable<System.DateTime> AssignedTime { get; set; }
        public Nullable<System.DateTime> InspectionDate { get; set; }
        public Nullable<int> OutComeCode { get; set; }
        public Nullable<System.DateTime> DueTime { get; set; }
        public Nullable<System.DateTime> ActualDeliveryDateTime { get; set; }
        public int StepCode { get; set; }
        public int StepId { get; set; }
        public int StatusId { get; set; }
    
        public virtual EmployeeDesk EmployeeDesk { get; set; }
        public virtual Notification Notification { get; set; }
        public virtual Step Step { get; set; }
    }
}
