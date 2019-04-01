using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Helpers;
using ImportPermitPortal.Models;
using ImportPermitPortal.Services.Services;

namespace ImportPermitPortal.Controllers
{
    [Authorize]
    public class DocumentController : Controller
    {
        public ActionResult MyDocuments()   
        {
            try
            {
                long appId = 0;

                if (Session["_appId"] != null)
                {
                    appId = (long)Session["_appId"];
                }

                var importerInfo = GetLoggedOnUserInfo();
                if (importerInfo.Id < 1)
                {
                    return Json(new List<Application>(), JsonRequestBehavior.AllowGet);
                }
                var documents = new DocumentServices().GetDocuments(appId, importerInfo.Id, User.IsInRole("Banker"));
                Session["_appId"] = null;
                return Json(documents, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return Json(new List<DocumentObject>(), JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize(Roles = "Banker,Bank_User")]
        public ActionResult GetAppDocs(long id)
        {
            try
            {
                var documents = new DocumentServices().GetDocuments(id);
                Session["_appId"] = null;
                return Json(documents, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return Json(new List<DocumentObject>(), JsonRequestBehavior.AllowGet);
            }
        }

        private ImporterObject GetLoggedOnUserInfo()
        {
            try
            {
                if (Session["_importerInfo"] == null)
                {
                    return new ImporterObject();
                }

                var importerInfo = Session["_importerInfo"] as ImporterObject;
                if (importerInfo == null || importerInfo.Id < 1)
                {
                    return new ImporterObject();
                }

                return importerInfo;

            }
            catch (Exception)
            {
                return new ImporterObject();
            }
        }
        
        public GenericValidator SaveDocument(DocumentObject document)
        {
            var gVal = new GenericValidator();

            try
            {
                var docStatus = new DocumentServices().AddDocument(document);
                if (docStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Document upload failed. Please try again.";
                    return gVal;
                }

                gVal.Code = docStatus;
                gVal.Error = "";
                return gVal;
                
            }
            catch (Exception)
            {
                gVal.Error = "Document processing failed. Please try again later";
                gVal.Code = -1;
                return gVal;
            }
        } 

        public GenericValidator SaveNotificationDocument(DocumentObject document)
        {
            var gVal = new GenericValidator();

            try
            {
                var docStatus = new DocumentServices().AddNotificationDocument(document);
                if (docStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Document upload failed. Please try again.";
                    return gVal;
                }

                gVal.Code = docStatus;
                gVal.Error = "";
                return gVal;
                
            }
            catch (Exception)
            {
                gVal.Error = "Document processing failed. Please try again later";
                gVal.Code = -1;
                return gVal;
            }
        } 

        public GenericValidator UpdateDocument(DocumentObject document)
        {
            var gVal = new GenericValidator();

            try
            {
                document.DateUploaded = DateTime.Now;
                var docStatus = new DocumentServices().UpdateDocument(document);
                if (docStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Document upload failed. Please try again.";
                    return gVal;
                }

                gVal.Code = docStatus;
                gVal.Error = "";
                return gVal;

            }
            catch (Exception)
            {
                gVal.Error = "Document processing failed. Please try again later";
                gVal.Code = -1;
                return gVal;
            }
        }

        [HttpPost]
        public ActionResult AddDocument(DocumentObject document)
        {
            var gVal = new GenericValidator();

            try
            {
                var importerInfo = GetLoggedOnUserInfo();
                if (importerInfo.Id < 1)
                {
                    gVal.Error = "Your session has timed out";
                    gVal.Code = -1;
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var validationResult = ValidateDocument(document);

                if (validationResult.Code == 1)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                var imagePath = SaveFile("");
                if (string.IsNullOrEmpty(imagePath))
                {
                    validationResult.Code = -1;
                    validationResult.Error = "Document upload failed. Please try again.";
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                document.ImporterId = importerInfo.Id;
                document.DateUploaded = DateTime.Now;
                document.UploadedById = document.UploadedById;
                document.DocumentPath = imagePath.Replace("~", "");
                document.Status = (int)AppStatus.Pending;
                var docStatus = new DocumentServices().AddDocument(document);
                if (docStatus < 1)
                {
                    validationResult.Code = -1;
                    validationResult.Error = "Document upload failed. Please try again.";
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = docStatus;
                gVal.Error = imagePath.Replace("~", "");
                gVal.Email = document.DateUploaded.ToString("dd/MM/yyyy");
                return Json(gVal, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                gVal.Error = "Document processing failed. Please try again later";
                gVal.Code = -1;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult EditDocument(DocumentObject document)
        {
            var gVal = new GenericValidator();

            try
            {
                if (document.DocumentTypeId < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Please select Document Type.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                if (document.ApplicationId < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Please select an Application Reference.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                
                if (Session["_document"] == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet); 
                }

                var olddocument = Session["_document"] as DocumentObject;

                if (olddocument == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                    
                var formerSampleImagePath = string.Empty;
                if (!string.IsNullOrEmpty(olddocument.DocumentPath))
                {
                    formerSampleImagePath = olddocument.DocumentPath;
                }

                var filePath = SaveFile(formerSampleImagePath);

                if (!string.IsNullOrEmpty(filePath))
                {
                    olddocument.DocumentPath = filePath.Replace("~", "");
                }

                olddocument.DateUploaded = DateTime.Now;

                var docStatus = new DocumentServices().UpdateDocument(olddocument);
                if (docStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Document information could not be updated. Please try again later";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                gVal.Code = olddocument.DocumentId;
                gVal.Error = olddocument.DocumentPath;
                gVal.Email = olddocument.DateUploaded.ToString("dd/MM/yyyy");
                Session["_document"] = null;
                return Json(gVal, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                gVal.Code = -1;
                gVal.Error = "Document information could not be updated. Please try again later";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult CreateFileSession(HttpPostedFileBase file)
        {
            var gVal = new GenericValidator();
            try
            {
                if (file != null && file.ContentLength > 0)
                {
                    gVal.Code = 5;
                    Session["_tempDoc"] = file;
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = -1;
                gVal.Error = "Document information could not be processed.";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                gVal.Code = -1;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult UploadSignOffDocument(HttpPostedFileBase file, long applicationId)
        {
            var gVal = new GenericValidator();
            try
            {
                if ( applicationId < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Document information could not be processed.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var importerInfo = GetLoggedOnUserInfo();
                if (importerInfo.UserProfileObject.Id < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Your session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var path = SaveSignOffFile(file);

                if (string.IsNullOrEmpty(path))
                {
                    gVal.Code = -1;
                    gVal.Error = "Document information Could not be saved.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var document = new SignOffDocumentObject
                {
                    DateUploaded = DateTime.Now,
                    ApplicationId = applicationId,
                    UploadedById = importerInfo.UserProfileObject.Id,
                    DocumentPath = path.Replace("~", ""),
                    IPAddress = ClientIpHelper.GetClientIpAddress(Request)
                };
                var tx = new DocumentServices().AddSignDocument(document);
                if (tx < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Document information Could not be saved.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                gVal.Error = "Document was successfully procesed";
                gVal.Path = path.Replace("~", "");
                gVal.Code = tx;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                gVal.Code = -1;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult CreateLtofFinFileSession(HttpPostedFileBase file) 
        {
            var gVal = new GenericValidator();
            try
            {
                if (file != null && file.ContentLength > 0)
                {
                    gVal.Code = 5;
                    Session["_newDoc"] = file;
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                gVal.Code = -1;
                gVal.Error = "Document information could not be processed.";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                gVal.Code = -1;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult SaveTempFile(HttpPostedFileBase file)
        {
            var gVal = new GenericValidator();
            try
            {
                var importerInfo = GetLoggedOnUserInfo();
                if (importerInfo.Id < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Your session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var path = PrepareAndSaveFile(file, importerInfo.Id, "");

                if (string.IsNullOrEmpty(path))
                {
                    gVal.Code = -1;
                    gVal.Error = "Document information Could not be saved.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                gVal.Path = PhysicalToVirtualPathMapper.MapPath(path).Replace("~", "");
                gVal.Code = 5;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                gVal.Code = -1;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        private string PrepareAndSaveFile(HttpPostedFileBase file, long userId, string formerFilePath)
        {
            try
            {
                var mainPath = "~/tempFiles/" + userId.ToString(CultureInfo.InvariantCulture);
                var subPath = HostingEnvironment.MapPath(mainPath);
                if (string.IsNullOrEmpty(subPath))
                {
                    return null;
                }

                if (!Directory.Exists(subPath))
                {
                    Directory.CreateDirectory(subPath);
                    var dInfo = new DirectoryInfo(subPath);
                    var dSecurity = dInfo.GetAccessControl();
                    dSecurity.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
                    dInfo.SetAccessControl(dSecurity);
                }
                var path = "";
                if (SaveToFolder(file, ref path, subPath, formerFilePath))
                {
                    return path;
                }
                return path;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return string.Empty;
            }
        }

        [HttpPost]
        public ActionResult SaveFormM(HttpPostedFileBase file, long id, long notificationId, DateTime dateIssued, string formMReference, double quantity, string letterOfCreditNo, long attachedDocumentId, int bankId)
        {

          var gVal = new GenericValidator();
            try
            {
                //if (string.IsNullOrEmpty(dateIssued))
                //{
                //    gVal.Code = -1;
                //    gVal.Error = "Please provide Date Issued.";
                //    return Json(gVal, JsonRequestBehavior.AllowGet);
                //}

                //DateTime date;
                //var res = DateTime.TryParse(dateIssued, out date);
                //if (!res || date.Year < 2)
                //{
                //    gVal.Code = -1;
                //    gVal.Error = "Please provide Date Issued.";
                //    return Json(gVal, JsonRequestBehavior.AllowGet);
                //}
                var formM = new FormMDetailObject
                {
                    NotificationId = notificationId,
                    DateIssued = dateIssued,
                    BankId = bankId,
                    FormMReference = formMReference,
                    Quantity = quantity,
                    LetterOfCreditNo = letterOfCreditNo,
                    AttachedDocumentId = attachedDocumentId,
                    DateAttached = DateTime.Now
                };
                if (formM.NotificationId < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Form M information could not be processed.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);  
                }

                var dstat = ValidateFormM(formM);
                if(dstat.Code < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = dstat.Error;
                    return Json(gVal, JsonRequestBehavior.AllowGet);  
                }

                var importerInfo = GetLoggedOnUserInfo();
                if (importerInfo.UserProfileObject.Id < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Your session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                
                if (file == null || file.ContentLength < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide a document to be attached.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var path = SaveFile2("", file, importerInfo.Id);

                if (string.IsNullOrEmpty(path))
                {
                    gVal.Code = -1;
                    gVal.Error = "Document information Could not be saved.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                
                formM.DocumentObject = new DocumentObject
                {
                    DocumentId =0,
                    ImporterId = importerInfo.Id,
                    NotificationId = formM.NotificationId ,
                    DocumentTypeId = (int)SpecialDocsEnum.Form_M ,
                    UploadedById = importerInfo.UserProfileObject.Id,
                    DateUploaded = DateTime.Now,
                    DocumentPath = path.Replace("~", "")
                };

                var tx = new DocumentServices().AddFormM(formM);
                if (tx < 1)
                {
                    DeleteFile(path);
                    gVal.Code = -1;
                    gVal.Error = "FormM information Could not be saved.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = tx;
                gVal.Error = "FormM information was successfully updated.";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                gVal.Code = -1;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }
        
        [HttpPost]
        public ActionResult UpdateFormM(HttpPostedFileBase file, long id, long notificationId, DateTime dateIssued, string formMReference, double quantity, string letterOfCreditNo, long attachedDocumentId, string oldfilePath)
        {

            var formM = new FormMDetailObject
            {
                Id = id,
                NotificationId = notificationId,
                DateIssued = dateIssued,
                FormMReference = formMReference,
                Quantity = quantity,
                LetterOfCreditNo = letterOfCreditNo,
                AttachedDocumentId = attachedDocumentId,
                DateAttached = DateTime.Now,
                DocumentPath = oldfilePath
            };

            var gVal = new GenericValidator();
            try
            {
                if (formM.NotificationId < 1 || formM.Id < 1 || string.IsNullOrEmpty(oldfilePath))
                {
                    gVal.Code = -1;
                    gVal.Error = "FormM information could not be processed.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var dstat = ValidateFormM(formM);
                if (dstat.Code < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = dstat.Error;
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var importerInfo = GetLoggedOnUserInfo();
                if (importerInfo.UserProfileObject.Id < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Your session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                if (file != null)
                {
                    var path = SaveFile2(oldfilePath, file, importerInfo.Id);

                    if (string.IsNullOrEmpty(path))
                    {
                        gVal.Code = -1;
                        gVal.Error = "FormM Document Could not be attached.";
                        return Json(gVal, JsonRequestBehavior.AllowGet);
                    }

                    formM.DocumentPath = path.Replace("~", "");
                    formM.DateAttached = DateTime.Now;
                }

                var tx = new DocumentServices().UpdateFormM(formM);
                if (tx < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "FormM information Could not be saved.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = tx;
                gVal.Error = "FormM information was successfully updated.";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                gVal.Code = -1;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }
        
        [HttpPost]
        public ActionResult SaveStageFile(HttpPostedFileBase file, int docTypeId, long importAppId)
        {
            var gVal = new GenericValidator();
            try
            {
                if (docTypeId < 1 || importAppId < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Document information could not be processed.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var importerInfo = GetLoggedOnUserInfo();
                if (importerInfo.UserProfileObject.Id < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Your session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var path = SaveFile2("", file, importerInfo.Id);

                if (string.IsNullOrEmpty(path))
                {
                    gVal.Code = -1;
                    gVal.Error = "Document information Could not be saved.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var document = new DocumentObject
                {
                    ImporterId = importerInfo.Id,
                    DateUploaded = DateTime.Now,
                    Status = (int) AppStatus.Pending,
                    DocumentId = 0,
                    ApplicationId = importAppId,
                    DocumentTypeId = docTypeId,
                    UploadedById = importerInfo.UserProfileObject.Id,
                    DocumentPath = path.Replace("~", "")
                };
                var tx = SaveDocument(document);
                if (tx.Code < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Document information Could not be saved.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Path = path.Replace("~", "");
                gVal.Code = tx.Code;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                gVal.Code = -1;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult SaveRefLetter(HttpPostedFileBase file, long applicationItemId, long applicationId, int bankId)
        {
            var gVal = new GenericValidator();
            try
            {
                if (applicationItemId < 1 || applicationId < 1 || bankId  < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Document information could not be processed.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var importerInfo = GetLoggedOnUserInfo();
                if (importerInfo.UserProfileObject.Id < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Your session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var path = SaveFile2("", file, importerInfo.Id);

                if (string.IsNullOrEmpty(path))
                {
                    gVal.Code = -1;
                    gVal.Error = "Document information Could not be saved.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                const int docTypeId = (int) SpecialDocsEnum.Bank_Reference_Letter;

                var document = new DocumentObject
                {
                    ImporterId = importerInfo.Id,
                    DateUploaded = DateTime.Now,
                    Status = (int)AppStatus.Pending,
                    DocumentId = 0,
                    ApplicationId = applicationId,
                    ApplicationItemId = applicationItemId,
                    DocumentTypeId = docTypeId,
                    BankId = bankId,
                    UploadedById = importerInfo.UserProfileObject.Id,
                    DocumentPath = path.Replace("~", "")
                };
                var tx = new DocumentServices().AddProductBankerDocument(document);
                if (tx < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Document information Could not be saved.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                gVal.Error = "Document was successfully procesed";
                gVal.Path = path.Replace("~", "");
                gVal.Code = tx;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                gVal.Code = -1;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

         [HttpPost]
        public ActionResult SaveRefLetterBySupport(HttpPostedFileBase file, long applicationItemId, long applicationId, int bankId, long importerId)
        {
            var gVal = new GenericValidator();
            try
            {
                if (applicationItemId < 1 || applicationId < 1 || bankId < 1 || importerId < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Document information could not be processed.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var importerInfo = GetLoggedOnUserInfo();
                if (importerInfo.UserProfileObject.Id < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Your session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var path = SaveFile2("", file, importerId);

                if (string.IsNullOrEmpty(path))
                {
                    gVal.Code = -1;
                    gVal.Error = "Document information Could not be saved.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                const int docTypeId = (int) SpecialDocsEnum.Bank_Reference_Letter;

                var document = new DocumentObject
                {
                    ImporterId = importerInfo.Id,
                    DateUploaded = DateTime.Now,
                    Status = (int)AppStatus.Pending,
                    DocumentId = 0,
                    ApplicationId = applicationId,
                    ApplicationItemId = applicationItemId,
                    DocumentTypeId = docTypeId,
                    BankId = bankId,
                    UploadedById = importerInfo.UserProfileObject.Id,
                    DocumentPath = path.Replace("~", "")
                };
                var tx = new DocumentServices().AddProductBankerDocument(document);
                if (tx < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Document information Could not be saved.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                gVal.Error = "Document was successfully procesed";
                gVal.Path = path.Replace("~", "");
                gVal.Code = tx;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                gVal.Code = -1;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost]
        public ActionResult UpdateRefLetter(HttpPostedFileBase file, long documentId)  
        {
            var gVal = new GenericValidator();
            try
            {
                if (documentId < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Document information could not be processed.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var importerInfo = GetLoggedOnUserInfo();
                if (importerInfo.UserProfileObject.Id < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Your session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var doc = new DocumentServices().GetDocument(documentId);
                if (doc.DocumentId < 1 || string.IsNullOrEmpty(doc.DocumentPath))
                {
                    gVal.Code = -1;
                    gVal.Error = "Document could not be processed. Please try again.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var path = SaveFile2(doc.DocumentPath, file, importerInfo.Id);

                if (string.IsNullOrEmpty(path))
                {
                    gVal.Code = -1;
                    gVal.Error = "Document information Could not be saved.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                doc.DateUploaded = DateTime.Now;
                doc.Status = (int) AppStatus.Pending;
                doc.UploadedById = importerInfo.UserProfileObject.Id;
                doc.DocumentPath = path.Replace("~", "");

                var tx = new DocumentServices().UpdateBankerDocument(doc);
                if (tx < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Document information Could not be saved.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Error = "Document was successfully procesed";
                gVal.Path = path.Replace("~", "");
                gVal.Code = tx;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                gVal.Code = -1;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult SaveTempFinLtFile(HttpPostedFileBase file)
        {
            var gVal = new GenericValidator();
            try
            {
                var importerInfo = GetLoggedOnUserInfo();
                if (importerInfo.Id < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Your session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var path = PrepareAndSaveFile(file, importerInfo.UserProfileObject.Id, "");

                if (string.IsNullOrEmpty(path))
                {
                    gVal.Code = -1;
                    gVal.Error = "Document information Could not be saved.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Path = PhysicalToVirtualPathMapper.MapPath(path).Replace("~", "");
                gVal.Code = 5;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                gVal.Code = -1;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult SaveNotificationFile(HttpPostedFileBase file, int docTypeId, long notificationId)
        {
            var gVal = new GenericValidator();
            try
            {
                if (docTypeId < 1 || notificationId < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Document information could not be processed.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var importerInfo = GetLoggedOnUserInfo();
                if (importerInfo.UserProfileObject.Id < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Your session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var path = SaveFile2("", file, importerInfo.Id);

                if (string.IsNullOrEmpty(path))
                {
                    gVal.Code = -1;
                    gVal.Error = "Document information Could not be saved.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var document = new DocumentObject
                {
                    ImporterId = importerInfo.Id,
                    DateUploaded = DateTime.Now,
                    Status = (int)AppStatus.Pending,
                    DocumentId = 0,
                    NotificationId = notificationId,
                    DocumentTypeId = docTypeId,
                    UploadedById = importerInfo.UserProfileObject.Id,
                    DocumentPath = path.Replace("~", "")
                };
                var tx = SaveNotificationDocument(document);
                if (tx.Code < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Document information Could not be saved.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                gVal.Code = tx.Code;
                gVal.Path = path.Replace("~", "");
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                gVal.Code = -1;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }
         
        [HttpPost]
        public ActionResult UpdateStageFile(HttpPostedFileBase file, long documentId)
        {
            var gVal = new GenericValidator();
            try
            {
                if (documentId < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Document information could not be processed.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var importerInfo = GetLoggedOnUserInfo();
                if (importerInfo.UserProfileObject.Id < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Your session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var doc = new DocumentServices().GetDocument(documentId);
                if (doc == null || doc.DocumentId < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Document information could not be processed.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var path = SaveFile2(doc.DocumentPath, file, importerInfo.Id);

                if (string.IsNullOrEmpty(path))
                {
                    gVal.Code = -1;
                    gVal.Error = "Document information Could not be saved.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                doc.DocumentPath = path;

                var tx = UpdateDocument(doc);
                if (tx.Code < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Document information Could not be saved.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = tx.Code;
                gVal.Path = doc.DocumentPath;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                gVal.Code = -1;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult SaveNewThrouput(HttpPostedFileBase file, long notificationId, int depotId)
        {
            var gVal = new GenericValidator();
            try
            {
                if (depotId < 1 || notificationId < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Discharge Depot information could not be processed.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var importerInfo = GetLoggedOnUserInfo();
                if (importerInfo.UserProfileObject.Id < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Your session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var path = SaveFile2("", file, importerInfo.Id);

                if (string.IsNullOrEmpty(path))
                {
                    gVal.Code = -1;
                    gVal.Error = "Discharge Depot information Could not be saved.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var document = new DocumentObject
                {
                    ImporterId = importerInfo.Id,
                    DateUploaded = DateTime.Now,
                    Status = (int)AppStatus.Pending,
                    DocumentId = 0,
                    NotificationId = notificationId,
                    DepotId = depotId,
                    DocumentTypeId = (int)SpecialDocsEnum.Throughput_agreement,
                    UploadedById = importerInfo.UserProfileObject.Id,
                    DocumentPath = path.Replace("~", "")
                };
                var tx = new DocumentServices().SaveNotificationDocumentUpdateDepot(document);
                if (tx < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Discharge Depot information Could not be saved.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                gVal.Code = tx;
                gVal.DocumentTypeId = (int)SpecialDocsEnum.Throughput_agreement;
                gVal.Error = "Discharge Depot information was successfully updated.";
                gVal.Path = path.Replace("~", "");
                gVal.FileName = SpecialDocsEnum.Throughput_agreement.ToString().Replace("_", " ");
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }

            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                gVal.Error = "Discharge Depot information Could not be updated.";
                gVal.Code = -1;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult UpdateDepot(long notificationId, int depotId)
        {
            var gVal = new GenericValidator();
            try
            {
                if (depotId < 1 || notificationId < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Throuput information could not be processed.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }


                var tx = new NotificationServices().UpdateDischargeDepot(notificationId, depotId);
                if (!tx)
                {
                    gVal.Code = -1;
                    gVal.Error = "Depot information Could not be saved.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = 5;
                gVal.Error = "Depot information was successfuly saved.";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                gVal.Error = "Throughput information Could not be saved.";
                gVal.Code = -1;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetDocumentTypes()
        {
            try
            {
               
                var documentTypes = new DocumentTypeServices().GetDocumentTypes();
                return Json(documentTypes, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                return Json(new List<DocumentTypeObject>(), JsonRequestBehavior.AllowGet); 
            }
        }

        public ActionResult GetApplications()
        {
            try
            {
                var applications = new ApplicationServices().GetApplications();

                if (!applications.Any())
                {
                    return Json(new List<Application>(), JsonRequestBehavior.AllowGet);
                }
                return Json(applications, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                return Json(new List<Application>(), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult SetDocSession(long id)
        {
            if (id < 1)
            {
                return Json(0, JsonRequestBehavior.AllowGet);
            }

            Session["_appId"] = id;
            return Json(5, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetDocument(long id)
        {
            try
            {
                var document = new DocumentServices().GetDocument(id);
                if (document == null || document.DocumentId < 1)
                {
                    return Json(new DocumentViewModel(), JsonRequestBehavior.AllowGet);
                }

                Session["_document"] = document;

                return Json(document, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                return Json(new DocumentViewModel(), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetFormMByProduct(long id)
        {
            try
            {
                var document = new DocumentServices().GetFormMByProduct(id);
                if (document == null || document.Id < 1)
                {
                    return Json(new FormMDetailObject(), JsonRequestBehavior.AllowGet);
                }

                Session["_formM"] = document;

                return Json(document, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(new FormMDetailObject(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult DeleteDocument(long id)
        {
            var gVal = new GenericValidator();
            try
            {
                if (id < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Invalid selection";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                var delStatus = new DocumentServices().DeleteDocument(id);
                if (delStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Docment could not be deleted. Please try again later.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = 5;
                gVal.Error = "Document Information was successfully deleted";
                return Json(gVal, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }
        private DocumentObject GetDocumentDetails(long id)
        {
            try
            {

                var document = new DocumentServices().GetDocument(id);

                if (document == null || document.DocumentId < 1)
                {
                    return new DocumentObject();
                }

                return document;
                
            }
            catch (Exception)
            {
                return new DocumentObject();
            }
        }

        private GenericValidator ValidateDocument(DocumentObject document)
        {
            var gVal = new GenericValidator();
            try
            {
                if (document.DocumentTypeId < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Please select Document Type.";
                    return gVal;
                }

                if (document.ApplicationId < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Please select an Application Reference.";
                    return gVal;
                }

                gVal.Code = 5;
                return gVal;
                
            }
            catch (Exception)
            {
                
                gVal.Code = -1;
                gVal.Error = "Document Validation failed. Please provide all required fields and try again.";
                return gVal;
            }
        }

        private GenericValidator ValidateFormM(FormMDetailObject document)
        {
            var gVal = new GenericValidator();
            try
            {

                if (document.NotificationId < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Notification information is empty or session has timed out.";
                    return gVal;
                }

                if (document.Quantity < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide product quantity.";
                    return gVal;
                }
                if (document.BankId < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "An unknown error was encountered. Please refresh the page and try again.";
                    return gVal;
                }
                if (string.IsNullOrEmpty(document.FormMReference))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please Reference.";
                    return gVal;
                }

                if (string.IsNullOrEmpty(document.LetterOfCreditNo))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please Letter Of Credit Number.";
                    return gVal;
                }

                if (document.DateIssued.Year < 2)
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide Date Issued.";
                    return gVal;
                }

                gVal.Code = 5;
                return gVal;
                
            }
            catch (Exception)
            {
                
                gVal.Code = -1;
                gVal.Error = "Document Validation failed. Please provide all required fields and try again.";
                return gVal;
            }
        }

        public string SaveFile(string formerPath)
        {
            try
            {
                var file = Session["_tempDoc"] as HttpPostedFileBase;

                if (file != null && file.ContentLength > 0)
                {
                    var mainPath = Server.MapPath("~/ImportDocuments");

                    if (!Directory.Exists(mainPath))
                    {
                        Directory.CreateDirectory(mainPath);
                        var dInfo = new DirectoryInfo(mainPath);
                        var dSecurity = dInfo.GetAccessControl();
                        dSecurity.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
                        dInfo.SetAccessControl(dSecurity);
                    }
                    var path = "";
                    if (SaveToFolder(file, ref path, mainPath, formerPath))
                    {
                        Session["_tempDoc"] = null;
                        return PhysicalToVirtualPathMapper.MapPath(path);
                    }
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return string.Empty;
            }
        }
        
        public string SaveFile2(string formerPath, HttpPostedFileBase file, long importerId)
        {
            try
            {
                if (file != null && file.ContentLength > 0)
                {
                    var mainPath = HostingEnvironment.MapPath("~/ImportDocuments/" + importerId.ToString(CultureInfo.InvariantCulture));
                    if (string.IsNullOrEmpty(mainPath))
                    {
                        return "";
                    }

                    if (!Directory.Exists(mainPath))
                    {
                        Directory.CreateDirectory(mainPath);
                        var dInfo = new DirectoryInfo(mainPath);
                        var dSecurity = dInfo.GetAccessControl();
                        dSecurity.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
                        dInfo.SetAccessControl(dSecurity);
                    }
                    var path = "";
                    if (SaveToFolder(file, ref path, mainPath, formerPath))
                    {
                        return PhysicalToVirtualPathMapper.MapPath(path);
                    }
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return string.Empty;
            }
        }

        public string SaveSignOffFile(HttpPostedFileBase file)
        {
            try
            {
                if (file != null && file.ContentLength > 0)
                {
                    var mainPath = HostingEnvironment.MapPath("~/ImportDocuments/SignOffDocuments");
                    if (string.IsNullOrEmpty(mainPath))
                    {
                        return "";
                    }

                    if (!Directory.Exists(mainPath))
                    {
                        Directory.CreateDirectory(mainPath);
                        var dInfo = new DirectoryInfo(mainPath);
                        var dSecurity = dInfo.GetAccessControl();
                        dSecurity.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
                        dInfo.SetAccessControl(dSecurity);
                    }
                    var path = "";
                    if (SaveToFolder(file, ref path, mainPath))
                    {
                        return PhysicalToVirtualPathMapper.MapPath(path);
                    }
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return string.Empty;
            }
        }
        private static string GenerateUniqueName()
        {
            return DateTime.Now.ToString("yyyyMMddHHmmssfff") + "_" + Guid.NewGuid();
        }
        private bool SaveToFolder(HttpPostedFileBase file, ref string path, string folderPath, string formerFilePath = null)
        {
            try
            {
                if (file.ContentLength > 0)
                {
                    var fileExtension = Path.GetExtension(file.FileName);
                    var fileName = GenerateUniqueName() + fileExtension;
                    var newPathv = Path.Combine(folderPath, fileName);
                    file.SaveAs(newPathv);
                    if (!string.IsNullOrWhiteSpace(formerFilePath))
                    {
                        if (!DeleteFile(formerFilePath))
                        {
                            return false;
                        }
                    }
                    path = newPathv.Replace("~", string.Empty);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return false;
            }
        }
        private bool DeleteFile(string filePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filePath))
                {
                    return false;
                }

                if (!filePath.StartsWith("~"))
                {
                    filePath = "~" + filePath;
                }

                System.IO.File.Delete(Server.MapPath(filePath));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
