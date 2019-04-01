using System;
using System.Collections.Generic;
using System.Linq;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Managers.Managers;

namespace ImportPermitPortal.Services.Services
{
	public class PageServices
	{
        private readonly PageManager _pageManager;
        public PageServices()
		{
            _pageManager = new PageManager();
		}

        public long AddPage(PageObject page)
		{
			try
			{
                return _pageManager.AddPage(page);
			}
			catch (Exception ex)
			{
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
				return 0;
			}
		}

        public long DeletePage(long pageId)
        {
            try
            {
                return _pageManager.DeletePage(pageId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long UpdatePage(PageObject page)
        {
            try
            {
                return _pageManager.UpdatePage(page);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }
        
        public List<PageObject> GetPages()
		{
			try
			{
                var objList = _pageManager.GetPages();
                if (objList == null || !objList.Any())
			    {
                    return new List<PageObject>();
			    }
				return objList;
			}
			catch (Exception ex)
			{
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<PageObject>();
			}
		}

        public PageObject GetPage(long pageId)
        {
            try
            {
                return _pageManager.GetPage(pageId);
                
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new PageObject();
            }
        }

        public List<PageObject> GetPages(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                var objList = _pageManager.GetPages(itemsPerPage, pageNumber, out countG);
                if (objList == null || !objList.Any())
                {
                    return new List<PageObject>();
			    }
                
                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                countG = 0;
                return new List<PageObject>();
            }
        }

        public List<PageObject> Search(string searchCriteria)
        {
            try
            {
                var objList = _pageManager.Search(searchCriteria);
                if (objList == null || !objList.Any())
                {
                    return new List<PageObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<PageObject>();
            }
        }
	}

}
