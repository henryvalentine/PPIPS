using System;
using System.Collections.Generic;
using System.Linq;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Managers.Managers;

namespace ImportPermitPortal.Services.Services
{
	public class FaqCategoryServices
	{
        private readonly FaqCategoryManager _faqCategoryManager;
        public FaqCategoryServices()
		{
            _faqCategoryManager = new FaqCategoryManager();
		}

        public long AddFaqCategory(FaqCategoryObject faqCategory)
		{
			try
			{
                return _faqCategoryManager.AddFaqCategory(faqCategory);
			}
			catch (Exception ex)
			{
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
				return 0;
			}
		}

        public long DeleteFaqCategory(long faqCategoryId)
        {
            try
            {
                return _faqCategoryManager.DeleteFaqCategory(faqCategoryId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long UpdateFaqCategory(FaqCategoryObject faqCategory)
        {
            try
            {
                return _faqCategoryManager.UpdateFaqCategory(faqCategory);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

     

        public List<FaqCategoryObject> GetFaqCategories()
        {
            try
            {
                var objList = _faqCategoryManager.GetFaqCategories();
                if (objList == null || !objList.Any())
                {
                    return new List<FaqCategoryObject>();
                }
                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<FaqCategoryObject>();
            }
        }


     

        public FaqCategoryObject GetFaqCategory(long faqCategoryId)
        {
            try
            {
                return _faqCategoryManager.GetFaqCategory(faqCategoryId);
                
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new FaqCategoryObject();
            }
        }

        public List<FaqCategoryObject> GetFaqCategories(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                var objList = _faqCategoryManager.GetFaqCategories(itemsPerPage, pageNumber, out countG);
                if (objList == null || !objList.Any())
                {
                    return new List<FaqCategoryObject>();
			    }
                
                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                countG = 0;
                return new List<FaqCategoryObject>();
            }
        }

        public List<FaqCategoryObject> Search(string searchCriteria)
        {
            try
            {
                var objList = _faqCategoryManager.Search(searchCriteria);
                if (objList == null || !objList.Any())
                {
                    return new List<FaqCategoryObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<FaqCategoryObject>();
            }
        }
	}

}
