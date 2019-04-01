namespace ImportPermitPortal.Models
{
    using System.Collections.Generic;

    public class ViewApplication
    {
        public long ApplicationId { get; set; }
        public long Id { get; set; }
        public string ReferenceCode { get; set; }
        public long DerivedTotalQUantity { get; set; }
        public double DerivedValue { get; set; }
        public int StatusCode { get; set; }
        public System.DateTime DateApplied { get; set; }
        public System.DateTime LastModified { get; set; }
        public string DateAppliedStr { get; set; }
        public string LastModifiedStr { get; set; }
        public string StatusStr { get; set; }
        public bool Rejected { get; set; }
        public string Approved { get; set; }
        public string CompanyName { get; set; }
        public long Code { get; set; }
        public int CompanyCategoryId { get; set; }
        public virtual List<ApplicationItemView> ApplicationItems { get; set; }
    }
}


public partial class ApplicationItemView
{
    public long ApplicationItemId { get; set; }
    public long ApplicationId { get; set; }
    public long ProductId { get; set; }
    public long EstimatedQuantity { get; set; }
    public double EstimatedValue { get; set; }
    public string CountryOfOriginName { get; set; }
    public string PortOfDischarge { get; set; }

    public virtual ProductView Product { get; set; }
}

public partial class ProductView
{
    public long ProductId { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public bool Availability { get; set; }

}