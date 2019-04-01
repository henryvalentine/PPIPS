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
    
    public partial class StorageTank
    {
        public StorageTank()
        {
            this.DischargeParameterAfters = new HashSet<DischargeParameterAfter>();
            this.DischargeParameterBefores = new HashSet<DischargeParameterBefore>();
            this.NotificationDischageDatas = new HashSet<NotificationDischageData>();
        }
    
        public long Id { get; set; }
        public int DepotId { get; set; }
        public string TankNo { get; set; }
        public long ProductId { get; set; }
        public Nullable<double> Capacity { get; set; }
        public int UoMId { get; set; }
    
        public virtual Depot Depot { get; set; }
        public virtual ICollection<DischargeParameterAfter> DischargeParameterAfters { get; set; }
        public virtual ICollection<DischargeParameterBefore> DischargeParameterBefores { get; set; }
        public virtual ICollection<NotificationDischageData> NotificationDischageDatas { get; set; }
        public virtual Product Product { get; set; }
        public virtual UnitOfMeasurement UnitOfMeasurement { get; set; }
    }
}
