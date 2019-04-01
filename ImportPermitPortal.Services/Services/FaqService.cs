using System;
using System.Collections.Generic;
using System.Linq;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Managers.Managers;

namespace ImportPermitPortal.Services.Services
{
	public class FaqServices
	{
        private readonly FaqManager _faqManager;
        public FaqServices()
		{
            _faqManager = new FaqManager();
		}

        public long AddFaq(FaqObject faq)
		{
			try
			{
                return _faqManager.AddFaq(faq);
			}
			catch (Exception ex)
			{
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
				return 0;
			}
		}

        public long DeleteFaq(long faqId)
        {
            try
            {
                return _faqManager.DeleteFaq(faqId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long UpdateFaq(FaqObject faq)
        {
            try
            {
                return _faqManager.UpdateFaq(faq);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

     

        public List<FaqObject> GetFaqs()
        {
            try
            {
                var objList = _faqManager.GetFaqs();
                if (objList == null || !objList.Any())
                {
                    return new List<FaqObject>();
                }
                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<FaqObject>();
            }
        }


     

        public FaqObject GetFaq(long faqId)
        {
            try
            {
                return _faqManager.GetFaq(faqId);
                
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new FaqObject();
            }
        }

        public List<FaqObject> GetFaqs(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                var objList = _faqManager.GetFaqs(itemsPerPage, pageNumber, out countG);
                if (objList == null || !objList.Any())
                {
                    return new List<FaqObject>();
			    }
                
                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                countG = 0;
                return new List<FaqObject>();
            }
        }

        public List<FaqObject> Search(string searchCriteria)
        {
            try
            {
                var objList = _faqManager.Search(searchCriteria);
                if (objList == null || !objList.Any())
                {
                    return new List<FaqObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<FaqObject>();
            }
        }
	}

}
