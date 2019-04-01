using System;
using System.Collections.Generic;
using System.Linq;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Managers.Managers;

namespace ImportPermitPortal.Services.Services
{
	public class DocumentTypeServices
	{
        private readonly DocumentTypeManager _documentTypeManager;
        public DocumentTypeServices()
		{
            _documentTypeManager = new DocumentTypeManager();
		}

        public long AddDocumentType(DocumentTypeObject documentType)
		{
			try
			{
                return _documentTypeManager.AddDocumentType(documentType);
			}
			catch (Exception ex)
			{
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
				return 0;
			}
		}

        public long DeleteDocumentType(long documentTypeId)
        {
            try
            {
                return _documentTypeManager.DeleteDocumentType(documentTypeId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long UpdateDocumentType(DocumentTypeObject documentType)
        {
            try
            {
                return _documentTypeManager.UpdateDocumentType(documentType);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }
        
        public List<DocumentTypeObject> GetDocumentTypes()
		{
			try
			{
                var objList = _documentTypeManager.GetDocumentTypes();
                if (objList == null || !objList.Any())
			    {
                    return new List<DocumentTypeObject>();
			    }
				return objList;
			}
			catch (Exception ex)
			{
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<DocumentTypeObject>();
			}
		}

        public List<DocumentTypeObject> GetApplicationStageDocumentTypes(RequirementProp requirementProp)
        {
            try
            {
                var objList = _documentTypeManager.GetApplicationStageDocumentTypes(requirementProp);
                if (objList == null || !objList.Any())
                {
                    return new List<DocumentTypeObject>();
                }
                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<DocumentTypeObject>();
            }
        }

        public List<DocumentTypeObject> GetNotificationStageDocumentTypes(int importClassId)
        {
            try
            {
                return _documentTypeManager.GetNotificationStageDocumentTypes(importClassId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<DocumentTypeObject>();
            }
        }

        public DocumentTypeObject GetDocumentType(long documentTypeId)
        {
            try
            {
                return _documentTypeManager.GetDocumentType(documentTypeId);
                
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new DocumentTypeObject();
            }
        }

        public List<DocumentTypeObject> GetDocumentTypes(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                var objList = _documentTypeManager.GetDocumentTypes(itemsPerPage, pageNumber, out countG);
                if (objList == null || !objList.Any())
                {
                    return new List<DocumentTypeObject>();
			    }
                
                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                countG = 0;
                return new List<DocumentTypeObject>();
            }
        }

        public List<DocumentTypeObject> Search(string searchCriteria)
        {
            try
            {
                var objList = _documentTypeManager.Search(searchCriteria);
                if (objList == null || !objList.Any())
                {
                    return new List<DocumentTypeObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<DocumentTypeObject>();
            }
        }
	}

}
