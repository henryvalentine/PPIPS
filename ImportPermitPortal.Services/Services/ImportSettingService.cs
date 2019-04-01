using System;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Managers.Managers;

namespace ImportPermitPortal.Services.Services
{
	public class ImportSettingServices
	{
        private readonly ImportSettingManager _importSettingManager;
        public ImportSettingServices()
		{
            _importSettingManager = new ImportSettingManager();
		}

        public long AddImportSetting(ImportSettingObject importSetting)
		{
			try
			{
                return _importSettingManager.AddImportSetting(importSetting);
			}
			catch (Exception ex)
			{
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
				return 0;
			}
		}

        public long DeleteImportSetting(long importSettingId)
        {
            try
            {
                return _importSettingManager.DeleteImportSetting(importSettingId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long UpdateImportSetting(ImportSettingObject importSetting)
        {
            try
            {
                return _importSettingManager.UpdateImportSetting(importSetting);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }
        

        public ImportSettingObject GetImportSetting()
        {
            try
            {
                return _importSettingManager.GetImportSetting();
                
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new ImportSettingObject();
            }
        }

	}

}
