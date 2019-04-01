using System;
using System.Collections.Generic;
using System.Linq;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Managers.Managers;

namespace ImportPermitPortal.Services.Services
{
	public class ProductRequirementServices
	{
        private readonly ProductDocumentRequirementManager _productRequirementManager;
        public ProductRequirementServices()
		{
            _productRequirementManager = new ProductDocumentRequirementManager();
		}

        public long AddProductRequirement(ProductDocumentRequirementObject productRequirement)
		{
			try
			{
                return _productRequirementManager.AddProductDocumentRequirement(productRequirement);
			}
			catch (Exception ex)
			{
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
				return 0;
			}
		}

        public long DeleteProductRequirement(long productRequirementId)
        {
            try
            {
                return _productRequirementManager.DeleteProductDocumentRequirement(productRequirementId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long UpdateProductRequirement(ProductDocumentRequirementObject productRequirement)
        {
            try
            {
                return _productRequirementManager.UpdateProductDocumentRequirement(productRequirement);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public List<ProductDocumentRequirementObject> GetProductRequirementsByDocumentType(int documentTypeId)
		{
			try
			{
                var objList = _productRequirementManager.GetProductDocumentRequirementsByDocumentType(documentTypeId);
                if (objList == null || !objList.Any())
			    {
                    return new List<ProductDocumentRequirementObject>();
			    }
				return objList;
			}
			catch (Exception ex)
			{
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<ProductDocumentRequirementObject>();
			}
		}

        public List<ProductDocumentRequirementObject> GetProductRequirementsByProduct(long productId)
        {
            try
            {
                var objList = _productRequirementManager.GetProductDocumentRequirementsByProduct(productId);
                if (objList == null || !objList.Any())
                {
                    return new List<ProductDocumentRequirementObject>();
                }
                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<ProductDocumentRequirementObject>();
            }
        }

        public List<ProductDocumentRequirementObject> GetProductRequirementsByProducts(List<ApplicationItemObject> importItems)
        {
            try
            {
                var objList = _productRequirementManager.GetProductDocumentRequirementsByProducts(importItems);
                if (objList == null || !objList.Any())
                {
                    return new List<ProductDocumentRequirementObject>();
                }
                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<ProductDocumentRequirementObject>();
            }
        }

        public ProductDocumentRequirementObject GetProductRequirement(long productRequirementId)
        {
            try
            {
                return _productRequirementManager.GetProductDocumentRequirement(productRequirementId);
                
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new ProductDocumentRequirementObject();
            }
        }

        public List<ProductDocumentRequirementObject> GetProductRequirements(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                var objList = _productRequirementManager.GetProductDocumentRequirements(itemsPerPage, pageNumber, out countG);
                if (objList == null || !objList.Any())
                {
                    return new List<ProductDocumentRequirementObject>();
			    }
                
                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                countG = 0;
                return new List<ProductDocumentRequirementObject>();
            }
        }

        public List<ProductDocumentRequirementObject> Search(string searchCriteria)
        {
            try
            {
                var objList = _productRequirementManager.Search(searchCriteria);
                if (objList == null || !objList.Any())
                {
                    return new List<ProductDocumentRequirementObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<ProductDocumentRequirementObject>();
            }
        }
        
	}

}
