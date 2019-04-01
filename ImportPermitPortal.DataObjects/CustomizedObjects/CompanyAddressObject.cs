
namespace ImportPermitPortal.DataObjects
{
    public class CompanyAddressObject
    {
        public long AddressId { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string LastUpdated { get; set; }
        public long CityId { get; set; }
        public long CompanyAddressId { get; set; }
        public int AddressTypeId { get; set; }
        public long CompanyId { get; set; }
        public bool? IsRegisteredSameAsOperational { get; set; }
    
    }
}
