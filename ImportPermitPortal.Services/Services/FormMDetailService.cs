using System;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Managers.Managers;

namespace ImportPermitPortal.Services.Services
{
	public class FormMDetailServices
	{
        private readonly FormMDetailManager _formMDetailManager;
        public FormMDetailServices()
		{
            _formMDetailManager = new FormMDetailManager();
		}

        public long AddFormMDetail(FormMDetailObject formMDetail)
		{
			try
			{
                return _formMDetailManager.AddFormMDetail(formMDetail);
			}
			catch (Exception ex)
			{
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
				return 0;
			}
		}

        public long DeleteFormMDetail(long formMDetailId)
        {
            try
            {
                return _formMDetailManager.DeleteFormMDetail(formMDetailId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long UpdateFormMDetail(FormMDetailObject formMDetail)
        {
            try
            {
                return _formMDetailManager.UpdateFormMDetail(formMDetail);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }
        
       
        public FormMDetailObject GetFormMDetail(long formMDetailId)
        {
            try
            {
                return _formMDetailManager.GetFormMDetail(formMDetailId);
                
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new FormMDetailObject();
            }
        }

	}

}
