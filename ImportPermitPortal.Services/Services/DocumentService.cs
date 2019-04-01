using System;
using System.Collections.Generic;
using System.Linq;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Managers.Managers;

namespace ImportPermitPortal.Services.Services
{
	public class DocumentServices
	{
        private readonly DocumentManager _documentManager;
        public DocumentServices()
		{
            _documentManager = new DocumentManager();
		}

        public long AddDocument(DocumentObject document)
		{
			try
			{
                return _documentManager.AddDocument(document);
			}
			catch (Exception ex)
			{
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
				return 0;
			}
		}

        public long AddProductBankerDocument(DocumentObject document)
        {
           return _documentManager.AddProductBankerDocument(document);
        }

        public long AddSignDocument(SignOffDocumentObject document)
        {
            return _documentManager.AddSignDocument(document);
        }

        public long AddNotificationDocument(DocumentObject document)
        {
            try
            {
                return _documentManager.AddNotificationDocument(document);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long SaveNotificationDocumentUpdateDepot(DocumentObject document)
        {
            try
            {
                return _documentManager.SaveNotificationDocumentUpdateDepot(document);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long AddFormM(FormMDetailObject formM)
        {
			try
			{
                return _documentManager.AddFormM(formM);
			}
			catch (Exception ex)
			{
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
				return 0;
			}
		}

       public long UpdateFormM(FormMDetailObject formM)
        {
            try
            {
                return _documentManager.UpdateFormM(formM);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }
        public long DeleteDocument(long documentId)
        {
            try
            {
                return _documentManager.DeleteDocument(documentId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long UpdateDocument(DocumentObject document)
        {
            try
            {
                return _documentManager.UpdateDocument(document);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long UpdateBankerDocument(DocumentObject document)
        {
            try
            {
                return _documentManager.UpdateBankerDocument(document);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public List<DocumentObject> GetDocuments()
		{
			try
			{
                var objList = _documentManager.GetDocuments();
                if (objList == null || !objList.Any())
			    {
                    return new List<DocumentObject>();
			    }
				return objList;
			}
			catch (Exception ex)
			{
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<DocumentObject>();
			}
		}

        public DocumentObject GetDocument(long documentId)
        {
            try
            {
                return _documentManager.GetDocument(documentId);
                
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new DocumentObject();
            }
        }

        public FormMDetailObject GetFormMByProduct(long documentId)
        {
            try
            {
                return _documentManager.GetFormMByProduct(documentId);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new FormMDetailObject();
            }
        }

        public List<DocumentObject> GetDocuments(long appId, long CompanyId, bool isBanker)
        {
            try
            {
                var objList = _documentManager.GetDocuments(appId, CompanyId, isBanker);
                if (objList == null || !objList.Any())
                {
                    return new List<DocumentObject>();
			    }
                
                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<DocumentObject>();
            }
        }

        public List<DocumentObject> GetDocuments(long CompanyId)
        {
            try
            {
                var objList = _documentManager.GetDocuments(CompanyId);
                if (objList == null || !objList.Any())
                {
                    return new List<DocumentObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<DocumentObject>();
            }
        }
        public List<DocumentObject> Search(string searchCriteria)
        {
            try
            {
                var objList = _documentManager.Search(searchCriteria);
                if (objList == null || !objList.Any())
                {
                    return new List<DocumentObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<DocumentObject>();
            }
        }
	}

}
