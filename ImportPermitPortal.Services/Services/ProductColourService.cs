using System;
using System.Collections.Generic;
using System.Linq;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Managers.Managers;

namespace ImportPermitPortal.Services.Services
{
    public class ProductColourServices
    {
        private readonly ProductColourManager _productColourManager;
        public ProductColourServices()
        {
            _productColourManager = new ProductColourManager();
        }

        public long AddProductColour(ProductColourObject productColour)
        {
            try
            {
                return _productColourManager.AddProductColour(productColour);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }
        
        public long DeleteProductColour(long productColourId)
        {
            try
            {
                return _productColourManager.DeleteProductColour(productColourId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long UpdateProductColour(ProductColourObject productColour)
        {
            try
            {
                return _productColourManager.UpdateProductColour(productColour);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public List<ProductColourObject> GetProductColours()
        {
            try
            {
                var objList = _productColourManager.GetProductColours();
                if (objList == null || !objList.Any())
                {
                    return new List<ProductColourObject>();
                }
                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<ProductColourObject>();
            }
        }

        public ProductColourObject GetProductColour(long productColourId)
        {
            try
            {
                return _productColourManager.GetProductColour(productColourId);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new ProductColourObject();
            }
        }

        public List<ProductColourObject> GetProductColours(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                var objList = _productColourManager.GetProductColours(itemsPerPage, pageNumber, out countG);
                if (objList == null || !objList.Any())
                {
                    return new List<ProductColourObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                countG = 0;
                return new List<ProductColourObject>();
            }
        }

        public List<ProductColourObject> Search(string searchCriteria)
        {
            try
            {
                var objList = _productColourManager.Search(searchCriteria);
                if (objList == null || !objList.Any())
                {
                    return new List<ProductColourObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<ProductColourObject>();
            }
        }


       
    }

}
