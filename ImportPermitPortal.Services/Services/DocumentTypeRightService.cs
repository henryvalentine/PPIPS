using System;
using System.Collections.Generic;
using System.Linq;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Managers.Managers;

namespace ImportPermitPortal.Services.Services
{
	public class DocumentTypeRightServices
	{
        private readonly DocumentTypeRightManager _documentTypeRightManager;
        public DocumentTypeRightServices()
		{
            _documentTypeRightManager = new DocumentTypeRightManager();
		}

        public long AddDocumentTypeRight(DocumentTypeRightObject documentTypeRight)
		{
			try
			{
                return _documentTypeRightManager.AddDocumentTypeRight(documentTypeRight);
			}
			catch (Exception ex)
			{
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
				return 0;
			}
		}

        public long UpdateDocumentTypeRights(List<DocumentTypeRightObject> documentTypeRights, List<DocumentTypeRightObject> newReqs)
        {
            try
            {
                return _documentTypeRightManager.UpdateDocumentTypeRights(documentTypeRights, newReqs);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }
        public long AddDocumentTypeRights(List<DocumentTypeRightObject> documentTypeRights)
		{
			try
			{
                return _documentTypeRightManager.AddDocumentTypeRights(documentTypeRights);
			}
			catch (Exception ex)
			{
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
				return 0;
			}
		}

        public long DeleteDocumentTypeRight(long documentTypeRightId)
        {
            try
            {
                return _documentTypeRightManager.DeleteDocumentTypeRight(documentTypeRightId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long UpdateDocumentTypeRight(DocumentTypeRightObject documentTypeRight)
        {
            try
            {
                return _documentTypeRightManager.UpdateDocumentTypeRight(documentTypeRight);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public List<DocumentTypeRightObject> GetDocumentTypeRightsByRole(string roleId)
		{
			try
			{
                var objList = _documentTypeRightManager.GetDocumentTypeRightsByRole(roleId);
                if (objList == null || !objList.Any())
			    {
                    return new List<DocumentTypeRightObject>();
			    }
				return objList;
			}
			catch (Exception ex)
			{
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<DocumentTypeRightObject>();
			}
		}

        public List<DocumentTypeRightObject> GetDocumentRightsByRole(string roleId)
        {
            try
            {
                var objList = _documentTypeRightManager.GetDocumentRightsByRole(roleId);
                if (objList == null || !objList.Any())
                {
                    return new List<DocumentTypeRightObject>();
                }
                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<DocumentTypeRightObject>();
            }
        }

        public List<DocumentTypeObject> GetDocumentRightsByRoles()
        {
            try
            {
                var objList = _documentTypeRightManager.GetDocumentRightsByRoles();
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

        public List<DocumentTypeRightObject> GetDocumentTypeRightsByDocumentType(int documentTypeId)
        {
            try
            {
                var objList = _documentTypeRightManager.GetDocumentTypeRightsByDocumentType(documentTypeId);
                if (objList == null || !objList.Any())
                {
                    return new List<DocumentTypeRightObject>();
                }
                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<DocumentTypeRightObject>();
            }
        }

        public DocumentTypeRightObject GetDocumentTypeRight(long documentTypeRightId)
        {
            try
            {
                return _documentTypeRightManager.GetDocumentTypeRight(documentTypeRightId);
                
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new DocumentTypeRightObject();
            }
        }

        public List<AspNetRoleObject> GetRoles()
        {
            try
            {
                return _documentTypeRightManager.GetRoles();

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<AspNetRoleObject>();
            }
        }


        public List<DocumentTypeRightObject> Search(string searchCriteria)
        {
            try
            {
                var objList = _documentTypeRightManager.Search(searchCriteria);
                if (objList == null || !objList.Any())
                {
                    return new List<DocumentTypeRightObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<DocumentTypeRightObject>();
            }
        }
        public List<DocumentTypeRightObject> GetDocumentTypeRights(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                var objList = _documentTypeRightManager.GetDocumentTypeRights(itemsPerPage, pageNumber, out countG);
                if (objList == null || !objList.Any())
                {
                    return new List<DocumentTypeRightObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                countG = 0;
                return new List<DocumentTypeRightObject>();
            }
        }
	}

}
