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
    
    public partial class UnclearedPayment
    {
        public long Id { get; set; }
        public Nullable<long> PaymentLogId { get; set; }
        public string ReferenceCode { get; set; }
        public double Amount { get; set; }
        public string CompanyId { get; set; }
        public Nullable<bool> IsAssigned { get; set; }
        public string ReferenceNoAssignedTo { get; set; }
        public System.DateTime DateAssigned { get; set; }
    
        public virtual PaymentLog PaymentLog { get; set; }
    }
}
