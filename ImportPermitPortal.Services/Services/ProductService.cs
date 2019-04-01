using System;
using System.Collections.Generic;
using System.Linq;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Managers.Managers;

namespace ImportPermitPortal.Services.Services
{
	public class ProductServices
	{
        private readonly ProductManager _productManager;
        public ProductServices()
		{
            _productManager = new ProductManager();
		}

        public long AddProduct(ProductObject product)
		{
			try
			{
                return _productManager.AddProduct(product);
			}
			catch (Exception ex)
			{
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
				return 0;
			}
		}

        public long DeleteProduct(long productId)
        {
            try
            {
                return _productManager.DeleteProduct(productId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long UpdateProduct(ProductObject product, List<ProductDocumentRequirementObject> newRequirements)
        {
            try
            {
                return _productManager.UpdateProduct(product, newRequirements);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }
        
        public List<ProductObject> GetProducts()
		{
			try
			{
                var objList = _productManager.GetProducts();
                if (objList == null || !objList.Any())
			    {
                    return new List<ProductObject>();
			    }
				return objList;
			}
			catch (Exception ex)
			{
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<ProductObject>();
			}
		}

        public ProductObject GetProduct(long productId)
        {
            try
            {
                return _productManager.GetProduct(productId);
                
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new ProductObject();
            }
        }

        public int VerifyCode(LicenseRefObject verifier)
        {
            try
            {
                return _productManager.VerifyCode(verifier);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long VerifyVesselLicense(LicenseRefObject verifier)
        {
            try
            {
                return _productManager.VerifyVesselLicense(verifier);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public int VerifyDepotLicenseCode(LicenseRefObject verifier)
        {
            try
            {
                return _productManager.VerifyDepotLicenseCode(verifier);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public List<ProductObject> GetProducts(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                var objList = _productManager.GetProducts(itemsPerPage, pageNumber, out countG);
                if (objList == null || !objList.Any())
                {
                    return new List<ProductObject>();
			    }
                
                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                countG = 0;
                return new List<ProductObject>();
            }
        }

        public List<ProductObject> Search(string searchCriteria)
        {
            try
            {
                var objList = _productManager.Search(searchCriteria);
                if (objList == null || !objList.Any())
                {
                    return new List<ProductObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<ProductObject>();
            }
        }
	}

}
