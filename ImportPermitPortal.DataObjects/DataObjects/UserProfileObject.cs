
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;

    public partial class UserProfileObject
    {
        public long Id { get; set; }
        public bool IsActive { get; set; }
        public long PersonId { get; set; }
        public bool IsAdmin { get; set; }
        public bool? IsFirstLogin { get; set; }

        public virtual ICollection<AspNetUserObject> AspNetUserObjects { get; set; }
        public virtual ICollection<DocumentIssueObject> DocumentIssueObjects { get; set; }
        public virtual ICollection<EmployeeDeskObject> EmployeeDeskObjects { get; set; }
        public virtual ICollection<MessageObject> MessageObjects { get; set; }
        public virtual ICollection<NotificationBankerObject> NotificationBankerObjects { get; set; }
        public virtual ICollection<PageObject> PageObjects { get; set; }
        public virtual PersonObject PersonObject { get; set; }
    }
}
