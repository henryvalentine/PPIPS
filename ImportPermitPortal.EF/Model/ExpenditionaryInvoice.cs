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
    
    public partial class ExpenditionaryInvoice
    {
        public long Id { get; set; }
        public long NotificationId { get; set; }
        public long InvoiceId { get; set; }
    
        public virtual Invoice Invoice { get; set; }
        public virtual Notification Notification { get; set; }
    }
}
