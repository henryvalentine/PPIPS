using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.DynamicMapper.DynamicMapper;
using ImportPermitPortal.EF.Model;

namespace ImportPermitPortal.Managers.Managers
{
   public class ImportSettingManager
    {

       public long AddImportSetting(ImportSettingObject ImportSetting)
       {
           try
           {
               if (ImportSetting == null)
               {
                   return -2;
               }

               var ImportSettingEntity = ModelMapper.Map<ImportSettingObject, ImportSetting>(ImportSetting);
               if (ImportSettingEntity == null || ImportSettingEntity.ApplicationExpiry < 1 || ImportSettingEntity.ApplicationLifeCycle < 1)
               {
                   return -2;
               }
               using (var db = new ImportPermitEntities())
               {
                   var returnStatus = db.ImportSettings.Add(ImportSettingEntity);
                   db.SaveChanges();
                   return returnStatus.Id;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return 0;
           }
       }
       public long UpdateImportSetting(ImportSettingObject ImportSetting)
       {
           try
           {
               if (ImportSetting == null)
               {
                   return -2;
               }

               var ImportSettingEntity = ModelMapper.Map<ImportSettingObject, ImportSetting>(ImportSetting);
               if (ImportSettingEntity == null || ImportSettingEntity.Id < 1)
               {
                   return -2;
               }

               using (var db = new ImportPermitEntities())
               {
                   db.ImportSettings.Attach(ImportSettingEntity);
                   db.Entry(ImportSettingEntity).State = EntityState.Modified;
                   db.SaveChanges();
                   return ImportSetting.Id;
               }
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
                using (var db = new ImportPermitEntities())
                {
                    var importSettings = db.ImportSettings.ToList();
                    if (!importSettings.Any())
                    {
                        return new ImportSettingObject();
                    }

                    var app = importSettings[0];
                    var importSettingObject = ModelMapper.Map<ImportSetting, ImportSettingObject>(app);
                    if (importSettingObject == null || importSettingObject.Id < 1)
                    {
                        return new ImportSettingObject();
                    }
                    
                  return importSettingObject;
                }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new ImportSettingObject();
           }
       }
       public long DeleteImportSetting(long ImportSettingId)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var myItems =
                       db.ImportSettings.Where(m => m.Id == ImportSettingId).ToList();
                   if (!myItems.Any())
                   {
                       return 0;
                   }

                   var item = myItems[0];
                   db.ImportSettings.Remove(item);
                   db.SaveChanges();
                   return 5;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return 0;
           }
       }
       
    }
}
