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
   public class GroupManager
    {

       public long AddGroup(GroupObject group)
       {
           try
           {
               if (group == null)
               {
                   return -2;
               }

               var groupEntity = ModelMapper.Map<GroupObject, Group>(group);
               if (groupEntity == null || string.IsNullOrEmpty(groupEntity.Name))
               {
                   return -2;
               }
               using (var db = new ImportPermitEntities())
               {
                   var returnStatus = db.Groups.Add(groupEntity);
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

       public long UpdateGroup(GroupObject group)
       {
           try
           {
               if (group == null)
               {
                   return -2;
               }

               var groupEntity = ModelMapper.Map<GroupObject, Group>(group);
               if (groupEntity == null || groupEntity.Id < 1)
               {
                   return -2;
               }

               using (var db = new ImportPermitEntities())
               {
                   db.Groups.Attach(groupEntity);
                   db.Entry(groupEntity).State = EntityState.Modified;
                   db.SaveChanges();
                   return group.Id;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return 0;
           }
       }
       public List<GroupObject> GetGroups()
        {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var groups = db.Groups.ToList();
                   if (!groups.Any())
                   {
                        return new List<GroupObject>();
                   }
                   var objList =  new List<GroupObject>();
                   groups.ForEach(app =>
                   {
                       var groupObject = ModelMapper.Map<Group, GroupObject>(app);
                       if (groupObject != null && groupObject.Id > 0)
                       {
                           objList.Add(groupObject);
                       }
                   });

                   return !objList.Any() ? new List<GroupObject>() : objList;
               }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return null;
           }
        }

        public List<GroupObject> GetGroups(int? itemsPerPage, int? pageNumber, out int countG)
        {
           try
           {
               if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
               {
                   var tpageNumber = (int) pageNumber;
                   var tsize = (int) itemsPerPage;

                   using (var db = new ImportPermitEntities())
                   {
                       var groups =
                           db.Groups.OrderByDescending(m => m.Id)
                               .Skip(tpageNumber)
                               .Take(tsize)
                               .ToList();
                       if (groups.Any())
                       {
                           var newList = new List<GroupObject>();
                           groups.ForEach(app =>
                           {
                               var groupObject = ModelMapper.Map<Group, GroupObject>(app);
                               if (groupObject != null && groupObject.Id > 0)
                               {
                                   newList.Add(groupObject);
                               }
                           });
                           countG = db.Groups.Count();
                           return newList;
                       }
                   }
                   
               }
               countG = 0;
               return new List<GroupObject>();
           }
           catch (Exception ex)
           {
               countG = 0;
               return new List<GroupObject>();
           }
       }

        
       public GroupObject GetGroup(long groupId)
       {
           try
           {
                using (var db = new ImportPermitEntities())
                {
                    var groups =
                        db.Groups.Where(m => m.Id == groupId)
                            .ToList();
                    if (!groups.Any())
                    {
                        return new GroupObject();
                    }

                    var app = groups[0];
                    var groupObject = ModelMapper.Map<Group, GroupObject>(app);
                    if (groupObject == null || groupObject.Id < 1)
                    {
                        return new GroupObject();
                    }
                    
                  return groupObject;
                }
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new GroupObject();
           }
       }

       public List<GroupObject> Search(string searchCriteria)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var groups =
                       db.Groups.Where(m => m.Name.ToLower().Trim() == searchCriteria.ToLower().Trim())
                       .ToList();

                   if (groups.Any())
                   {
                       var newList = new List<GroupObject>();
                       groups.ForEach(app =>
                       {
                           var groupObject = ModelMapper.Map<Group, GroupObject>(app);
                           if (groupObject != null && groupObject.Id > 0)
                           {
                               newList.Add(groupObject);
                           }
                       });

                       return newList;
                   }
               }
               return new List<GroupObject>();
           }

           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new List<GroupObject>();
           }
       }

       public long DeleteGroup(long groupId)
       {
           try
           {
               using (var db = new ImportPermitEntities())
               {
                   var myItems =
                       db.Groups.Where(m => m.Id == groupId).ToList();
                   if (!myItems.Any())
                   {
                       return 0;
                   }

                   var item = myItems[0];
                   db.Groups.Remove(item);
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
