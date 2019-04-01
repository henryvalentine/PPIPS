using System;
using System.Collections.Generic;
using System.Linq;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Managers.Managers;

namespace ImportPermitPortal.Services.Services
{
	public class StructureServices
	{
        private readonly StructureManager _structureManager;
        public StructureServices()
		{
            _structureManager = new StructureManager();
		}

        public long AddStructure(StructureObject structure)
		{
			try
			{
                return _structureManager.AddStructure(structure);
			}
			catch (Exception ex)
			{
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
				return 0;
			}
		}

        public long DeleteStructure(long structureId)
        {
            try
            {
                return _structureManager.DeleteStructure(structureId);
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
                return _structureManager.UpdateStructure(structure);
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
                var objList = _structureManager.GetStructures();
                if (objList == null || !objList.Any())
			    {
                    return new List<StructureObject>();
			    }
				return objList;
			}
			catch (Exception ex)
			{
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<StructureObject>();
			}
		}

        public StructureObject GetStructure(long structureId)
        {
            try
            {
                return _structureManager.GetStructure(structureId);
                
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new StructureObject();
            }
        }

        public List<StructureObject> GetStructures(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                var objList = _structureManager.GetStructures(itemsPerPage, pageNumber, out countG);
                if (objList == null || !objList.Any())
                {
                    return new List<StructureObject>();
			    }
                
                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                countG = 0;
                return new List<StructureObject>();
            }
        }

        public List<StructureObject> Search(string searchCriteria)
        {
            try
            {
                var objList = _structureManager.Search(searchCriteria);
                if (objList == null || !objList.Any())
                {
                    return new List<StructureObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<StructureObject>();
            }
        }
	}

}
