using System;
using System.Collections.Generic;
using System.Linq;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Managers.Managers;

namespace ImportPermitPortal.Services.Services
{
	public class GroupServices
	{
        private readonly GroupManager _groupManager;
        public GroupServices()
		{
            _groupManager = new GroupManager();
		}

        public long AddGroup(GroupObject group)
		{
			try
			{
                return _groupManager.AddGroup(group);
			}
			catch (Exception ex)
			{
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
				return 0;
			}
		}

        public long DeleteGroup(long groupId)
        {
            try
            {
                return _groupManager.DeleteGroup(groupId);
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
                return _groupManager.UpdateGroup(group);
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
                var objList = _groupManager.GetGroups();
                if (objList == null || !objList.Any())
			    {
                    return new List<GroupObject>();
			    }
				return objList;
			}
			catch (Exception ex)
			{
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<GroupObject>();
			}
		}

        public GroupObject GetGroup(long groupId)
        {
            try
            {
                return _groupManager.GetGroup(groupId);
                
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new GroupObject();
            }
        }

        public List<GroupObject> GetGroups(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                var objList = _groupManager.GetGroups(itemsPerPage, pageNumber, out countG);
                if (objList == null || !objList.Any())
                {
                    return new List<GroupObject>();
			    }
                
                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                countG = 0;
                return new List<GroupObject>();
            }
        }

        public List<GroupObject> Search(string searchCriteria)
        {
            try
            {
                var objList = _groupManager.Search(searchCriteria);
                if (objList == null || !objList.Any())
                {
                    return new List<GroupObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<GroupObject>();
            }
        }
	}

}
