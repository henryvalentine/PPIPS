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
    
    public partial class FormMDetail
    {
        public long Id { get; set; }
        public long NotificationId { get; set; }
        public System.DateTime DateIssued { get; set; }
        public string FormMReference { get; set; }
        public double Quantity { get; set; }
        public string LetterOfCreditNo { get; set; }
        public long AttachedDocumentId { get; set; }
        public System.DateTime DateAttached { get; set; }
        public int BankId { get; set; }
    
        public virtual Bank Bank { get; set; }
        public virtual Document Document { get; set; }
        public virtual Notification Notification { get; set; }
    }
}