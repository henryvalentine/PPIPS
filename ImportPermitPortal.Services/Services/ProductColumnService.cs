using System;
using System.Collections.Generic;
using System.Linq;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Managers.Managers;

namespace ImportPermitPortal.Services.Services
{
	public class ProductColumnServices
	{
        private readonly ProductColumnManager _productColumnManager;
        public ProductColumnServices()
		{
            _productColumnManager = new ProductColumnManager();
		}

        public long AddProductColumn(ProductColumnObject productColumn)
		{
			try
			{
                return _productColumnManager.AddProductColumn(productColumn);
			}
			catch (Exception ex)
			{
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
				return 0;
			}
		}

        public long DeleteProductColumn(long productColumnId)
        {
            try
            {
                return _productColumnManager.DeleteProductColumn(productColumnId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long UpdateProductColumn(ProductColumnObject productColumn)
        {
            try
            {
                return _productColumnManager.UpdateProductColumn(productColumn);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

         
       public List<ProductColumnObject> GetProductColumnsByProduct(int productId)
		{
			try
			{
                var objList = _productColumnManager.GetProductColumnsByProduct(productId);
                if (objList == null || !objList.Any())
			    {
                    return new List<ProductColumnObject>();
			    }
				return objList;
			}
			catch (Exception ex)
			{
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<ProductColumnObject>();
			}
		}

       public List<ProductColumnObject> GetProductColumnsByCustomCode(int customCodeId)
        {
            try
            {
                var objList = _productColumnManager.GetProductColumnsByCustomCode(customCodeId);
                if (objList == null || !objList.Any())
                {
                    return new List<ProductColumnObject>();
                }
                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<ProductColumnObject>();
            }
        }

        public ProductColumnObject GetProductColumn(long productColumnId)
        {
            try
            {
                return _productColumnManager.GetProductColumn(productColumnId);
                
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new ProductColumnObject();
            }
        }

        public List<ProductColumnObject> Search(string searchCriteria)
        {
            try
            {
                var objList = _productColumnManager.Search(searchCriteria);
                if (objList == null || !objList.Any())
                {
                    return new List<ProductColumnObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<ProductColumnObject>();
            }
        }

        public List<ProductColumnObject> GetProductColumns(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                var objList = _productColumnManager.GetProductColumns(itemsPerPage, pageNumber, out countG);
                if (objList == null || !objList.Any())
                {
                    return new List<ProductColumnObject>();
			    }
                
                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                countG = 0;
                return new List<ProductColumnObject>();
            }
        }
        
	}

}
