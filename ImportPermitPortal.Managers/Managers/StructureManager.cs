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
   public class StructureManager
    {

       public long AddStructure(StructureObject structure)
       {
           try
           {
               if (structure == null)
               {
                   return -2;
               }

               var structureEntity = ModelMapper.Map<StructureObject, Structure>(structure);
               if (structureEntity == null || string.IsNullOrEmpty(structureEntity.Name))
               {
                   return -2;
               }
               using (var db = new ImportPermitEntities())
               {
                   var returnStatus = db.Structures.Add(structureEntity);
                   db.SaveChanges();
                   return returnStatus.StructureId;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return 0;
           }
       }

       public long UpdateStructure(StructureObject structure)
       {
           try
           {
               if (structure == null)
               {
                   return -2;
               }

               var structureEntity = ModelMapper.Map<StructureObject, Structure>(structure);
               if (structureEntity == null || structureEntity.StructureId < 1)
               {
                   return -2;
               }

               using (var db = new ImportPermitEntities())
               {
                   db.Structures.Attach(structureEntity);
                   db.Entry(structureEntity).State = EntityState.Modified;
                   db.SaveChanges();
                   return structure.StructureId;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return 0;
           }
       }
       public List<StructureObject> GetStructures()
        {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var structures = db.Structures.ToList();
                   if (!structures.Any())
                   {
                        return new List<StructureObject>();
                   }
                   var objList =  new List<StructureObject>();
                   structures.ForEach(app =>
                   {
                       var structureObject = ModelMapper.Map<Structure, StructureObject>(app);
                       if (structureObject != null && structureObject.StructureId > 0)
                       {
                           objList.Add(structureObject);
                       }
                   });

                   return !objList.Any() ? new List<StructureObject>() : objList;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return null;
           }
        }

        public List<StructureObject> GetStructures(int? itemsPerPage, int? pageNumber, out int countG)
        {
           try
           {
               if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
               {
                   var tpageNumber = (int) pageNumber;
                   var tsize = (int) itemsPerPage;

                   using (var db = new ImportPermitEntities())
                   {
                       var structures =
                           db.Structures.OrderByDescending(m => m.StructureId)
                               .Skip(tpageNumber)
                               .Take(tsize)
                               .ToList();
                       if (structures.Any())
                       {
                           var newList = new List<StructureObject>();
                           structures.ForEach(app =>
                           {
                               var structureObject = ModelMapper.Map<Structure, StructureObject>(app);
                               if (structureObject != null && structureObject.StructureId > 0)
                               {
                                   newList.Add(structureObject);
                               }
                           });
                           countG = db.Structures.Count();
                           return newList;
                       }
                   }
                   
               }
               countG = 0;
               return new List<StructureObject>();
           }
           catch (Exception ex)
           {
               countG = 0;
               return new List<StructureObject>();
           }
       }

        
       public StructureObject GetStructure(long structureId)
       {
           try
           {
                using (var db = new ImportPermitEntities())
                {
                    var structures =
                        db.Structures.Where(m => m.StructureId == structureId)
                            .ToList();
                    if (!structures.Any())
                    {
                        return new StructureObject();
                    }

                    var app = structures[0];
                    var structureObject = ModelMapper.Map<Structure, StructureObject>(app);
                    if (structureObject == null || structureObject.StructureId < 1)
                    {
                        return new StructureObject();
                    }
                    
                  return structureObject;
                }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new StructureObject();
           }
       }

       public List<StructureObject> Search(string searchCriteria)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var structures =
                       db.Structures.Where(m => m.Name.ToLower().Trim() == searchCriteria.ToLower().Trim())
                       .ToList();

                   if (structures.Any())
                   {
                       var newList = new List<StructureObject>();
                       structures.ForEach(app =>
                       {
                           var structureObject = ModelMapper.Map<Structure, StructureObject>(app);
                           if (structureObject != null && structureObject.StructureId > 0)
                           {
                               newList.Add(structureObject);
                           }
                       });

                       return newList;
                   }
               }
               return new List<StructureObject>();
           }

           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new List<StructureObject>();
           }
       }

       public long DeleteStructure(long structureId)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var myItems =
                       db.Structures.Where(m => m.StructureId == structureId).ToList();
                   if (!myItems.Any())
                   {
                       return 0;
                   }

                   var item = myItems[0];
                   db.Structures.Remove(item);
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
