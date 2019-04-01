
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;
    
    public partial class ImporterObject
    {

        public long Id { get; set; }
        public string Name { get; set; }
        public string TIN { get; set; } 
        public string RCNumber { get; set; }
        public Nullable<int> StructureId { get; set; }
        public string LogoPath { get; set; }
        public bool IsActive { get; set; }
        public string DateAdded { get; set; }
        public string ShortNme { get; set; }
        public Nullable<int> TotalStaff { get; set; }
        public Nullable<int> TotalExpatriate { get; set; }
        public Nullable<System.DateTime> BusinessCommencementDate { get; set; }

        public virtual ICollection<ApplicationObject> ApplicationObjects { get; set; }
        public virtual ICollection<BankObject> BankObjects { get; set; }
        public virtual ICollection<CompanyDocumentObject> CompanyDocumentObjects { get; set; }
        public virtual ICollection<DepotObject> DepotObjects { get; set; }
        public virtual ICollection<DocumentObject> DocumentObjects { get; set; }
        public virtual ICollection<MessageObject> MessageObjects { get; set; }
        public virtual ICollection<NotificationObject> NotificationObjects { get; set; }
        public virtual ICollection<PersonObject> PeopleObject { get; set; }
        public virtual ICollection<PermitObject> PermitObjects { get; set; }
    }
}
