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
    
    public partial class SignOffDocument
    {
        public long Id { get; set; }
        public long ApplicationId { get; set; }
        public string DocumentPath { get; set; }
        public long UploadedById { get; set; }
        public System.DateTime DateUploaded { get; set; }
        public string IPAddress { get; set; }
    
        public virtual Application Application { get; set; }
        public virtual UserProfile UserProfile { get; set; }
    }
}
