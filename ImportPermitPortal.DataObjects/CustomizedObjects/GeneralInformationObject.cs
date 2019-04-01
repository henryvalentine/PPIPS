

using System;
using System.Collections.Generic;
using System.Web;

namespace ImportPermitPortal.DataObjects
{
    public class GeneralInformationObject
    {
        public long CompanyId { get; set; }
        public long ParentId { get; set; }
        public string RCNumber { get; set; }
        public string TIN { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public int StructureId { get; set; }
        public string StructureName { get; set; }
        public string LogoPath { get; set; }
        public DateTime BusinessCommencementDate { get; set; }
        public bool IsActive { get; set; }
        public string DateAdded { get; set; }
        public string BusinessCommencementDateStr { get; set; }
        public string ShortNme { get; set; }
        public int? TotalStaff { get; set; }
        public int? TotalExpatriate { get; set; }
        public List<CompanyAddressObj> CompanyAddressObjects { get; set; }
        public ItemCountObject AppCountObject { get; set; }
    }

    public class CompanyAddressObj
    {
        public long AddressId { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string LastUpdated { get; set; }
        public long CityId { get; set; }
        public string CityName { get; set; }
        public long CompanyAddressId { get; set; }
        public int AddressTypeId { get; set; }
        public long CompanyId { get; set; }
        public bool? IsRegisteredSameAsOperational { get; set; }

    }

    public class AddressTypeObject
    {
        public int AddressTypeId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string DateAdded { get; set; }
    }

    public class CompanyDocument
    {
        public long DocumentLibraryId { get; set; }
        public int DocumentTypeId { get; set; }
        public long CompanyId { get; set; }
        public string DocumentPath { get; set; }
        public string ValidFrom { get; set; }
        public string ValidTo { get; set; }
        public string LastUpdated { get; set; }
        public string Title { get; set; }
        public string DocumentTypeName { get; set; }
        public string FileJson { get; set; }
        public HttpPostedFileBase File { get; set; }
    }

    public class EligibilityResponse
    {
        public bool IsAddressProvided { get; set; }
        public bool IsAllDocumentsProvided { get; set; }
        public string Error { get; set; }
        public int Code { get; set; }
        public List<string> UnsuppliedDocs { get; set; }
    }
   
    public class CityObject
    {
        public long CityId { get; set; }
        public string Name { get; set; }
        public long CountryId { get; set; }
        public virtual CountryObject CountryObject { get; set; }
    }

    public class DocumentTypeObj
    {
        public int DocumentTypeId { get; set; }
        public string Name { get; set; }
    }

    public class GenericList
    {
        public List<CityObject> Cities { get; set; }
        public List<AddressTypeObject> AddressTypes { get; set; }
        public List<StructureObj> Structures { get; set; }
    }

    public class StructureObj
    {
        public int StructureId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}

