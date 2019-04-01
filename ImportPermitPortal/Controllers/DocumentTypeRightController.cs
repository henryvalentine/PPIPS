using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Helpers;
using ImportPermitPortal.Services.Services;

namespace ImportPermitPortal.Controllers
{
    [Authorize]
    public class DocumentTypeRightController : Controller
    {
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
       [HttpGet]
       public ActionResult GetDocumentsRequirementObjects(JQueryDataTableParamModel param)
       {
           try
           {
               IEnumerable<DocumentTypeRightObject> filteredParentMenuObjects;
               int countG;

               var pagedParentMenuObjects = GetDocumentsRequirements(param.iDisplayLength, param.iDisplayStart, out countG);

               if (!string.IsNullOrEmpty(param.sSearch))
               {
                   filteredParentMenuObjects = new DocumentTypeRightServices().Search(param.sSearch);
               }
               else
               {
                   filteredParentMenuObjects = pagedParentMenuObjects;
               }

               if (!filteredParentMenuObjects.Any())
               {
                   return Json(new List<DocumentTypeRightObject>(), JsonRequestBehavior.AllowGet);
               }

               //var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
               Func<DocumentTypeRightObject, string> orderingFunction = (c => c.RoleName);

               var sortDirection = Request["sSortDir_0"]; // asc or desc
               filteredParentMenuObjects = sortDirection == "desc" ? filteredParentMenuObjects.OrderBy(orderingFunction) : filteredParentMenuObjects.OrderByDescending(orderingFunction);

               var displayedPersonnels = filteredParentMenuObjects;

               var result = from c in displayedPersonnels
                            select new[] { Convert.ToString(c.RoleId), c.RoleName, c.DocumentTypeName};
               return Json(new
               {
                   param.sEcho,
                   iTotalRecords = countG,
                   iTotalDisplayRecords = countG,
                   aaData = result
               },
                  JsonRequestBehavior.AllowGet);

           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return Json(new List<DocumentsRequirementObject>(), JsonRequestBehavior.AllowGet);
           }
       }
       public ActionResult AddDocumentTypeRight(List<DocumentTypeRightObject> documentTypeRights)
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

                var validationResult = ValidateDocumentTypeRights(documentTypeRights);

                if (validationResult.Code == 1)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                var appStatus = new DocumentTypeRightServices().AddDocumentTypeRights(documentTypeRights);
                if (appStatus < 1)
                {
                    validationResult.Code = -1;
                    validationResult.Error = appStatus == -2 ? "DocumentTypeRight upload failed. Please try again." : "The Document Right Information already exists";
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = appStatus;
                gVal.Error = "Document Right was successfully added.";
                return Json(gVal, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                gVal.Error = "Document Right processing failed. Please try again later";
                gVal.Code = -1;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetGenericList()
        {
            try
            {
                var newList = new GenericDocumentRightsList
                {
                    DocumentTypes = GetDocumentTypes(),
                    Roles = GetRoless()
                };

                return Json(newList, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(new GenericEligibilityList(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult EditDocumentTypeRight(List<DocumentTypeRightObject> newReqs)
        {
            var gVal = new GenericValidator();

            try
            {
                var validationResult = ValidateDocumentTypeRights(newReqs);

                if (validationResult.Code == 1)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }
                
                if (Session["_documentTypeRights"] == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet); 
                }

                var olddocumentTypeRights = Session["_documentTypeRights"] as List<DocumentTypeRightObject>;

                if (olddocumentTypeRights == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var docStatus = new DocumentTypeRightServices().UpdateDocumentTypeRights(olddocumentTypeRights, newReqs);
                if (docStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = docStatus == -3 ? "Document Right already exists." : "Document Right information could not be updated. Please try again later";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = 5;
                gVal.Error = "Process completed successfully";
                return Json(gVal, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                gVal.Code = -1;
                gVal.Error = "Document Right information could not be updated. Please try again later";
                return Json(gVal, JsonRequestBehavior.AllowGet);
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

        public ActionResult GetDocumentTypeRight(string roleId)
        {
            try
            {
                var documentTypeRights = new DocumentTypeRightServices().GetDocumentRightsByRole(roleId);
                if (documentTypeRights == null || !documentTypeRights.Any())
                {
                    return Json(new DocumentTypeRightObject(), JsonRequestBehavior.AllowGet);
                }

                Session["_documentTypeRights"] = documentTypeRights;

                return Json(documentTypeRights, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                return Json(new DocumentTypeRightObject(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult DeleteDocumentTypeRight(long id)
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
                var delStatus = new DocumentTypeRightServices().DeleteDocumentTypeRight(id);
                if (delStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Document Right could not be deleted. Please try again later.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = 5;
                gVal.Error = "Document Right Information was successfully deleted";
                return Json(gVal, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }
        
        public ActionResult GetDocumentsReqsByImportStage(string id)
        {
            try
            {
                return Json(GetDocumentTypeRightsByRole(id), JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(new DocumentTypeRightObject(), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetImportEligibilityReqsByDocumentType(int id)
        {
            try
            {
                return Json(GetDocumentTypeRightsByDocumentType(id), JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(new DocumentTypeRightObject(), JsonRequestBehavior.AllowGet);
            }
        }

        private List<DocumentTypeRightObject> GetDocumentsRequirements(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                return new DocumentTypeRightServices().GetDocumentTypeRights(itemsPerPage, pageNumber, out countG);
            }
            catch (Exception)
            {
                countG = 0;
                return new List<DocumentTypeRightObject>();
            }
        }

        private List<AspNetRoleObject> GetRoless()
        {
            try
            {
                return new DocumentTypeRightServices().GetRoles();

            }
            catch (Exception)
            {
                return new List<AspNetRoleObject>();
            }
        }

        public List<DocumentTypeObject> GetDocumentTypes()
        {
            try
            {
                return new DocumentTypeServices().GetDocumentTypes();
            }
            catch (Exception)
            {
                return new List<DocumentTypeObject>();
            }
        }

        private List<DocumentTypeRightObject> GetDocumentTypeRightsByRole(string roleId)
        {
            try
            {
                return new DocumentTypeRightServices().GetDocumentTypeRightsByRole(roleId);
                
            }
            catch (Exception)
            {
                return new List<DocumentTypeRightObject>();
            }
        }
        private List<DocumentTypeRightObject> GetDocumentTypeRightsByDocumentType(int documentTypeId)
        {
            try
            {
                return new DocumentTypeRightServices().GetDocumentTypeRightsByDocumentType(documentTypeId);
                
            }
            catch (Exception)
            {
                return new List<DocumentTypeRightObject>();
            }
        }
        private GenericValidator ValidateDocumentTypeRight(DocumentTypeRightObject documentTypeRight)
        {
            var gVal = new GenericValidator();
            try
            {
                if (string.IsNullOrEmpty(documentTypeRight.RoleId))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please select a Role.";
                    return gVal;
                }

                if (documentTypeRight.DocumentTypeId < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Please select Document Type.";
                    return gVal;
                }

                gVal.Code = 5;
                return gVal;
                
            }
            catch (Exception)
            {
                
                gVal.Code = -1;
                gVal.Error = "Document Right Validation failed. Please provide all required fields and try again.";
                return gVal;
            }
        }

        private GenericValidator ValidateDocumentTypeRights(List<DocumentTypeRightObject> documentTypeRights)
        {
            var gVal = new GenericValidator();
            try
            {
                var count = 0;
                documentTypeRights.ForEach(m =>
                {
                    if (string.IsNullOrEmpty(m.RoleId))
                    {
                        gVal.Code = -1;
                        gVal.Error = "Please select a Role.";
                    }

                    if (m.DocumentTypeId < 1)
                    {
                        gVal.Code = -1;
                        gVal.Error = "Please select Document Type.";
                    }
                    count += 1;
                });

                if (count != documentTypeRights.Count)
                {
                    gVal.Error = "Validation failed. Please provide all required fields and try again.";
                    gVal.Code = -1;
                }
                else
                {
                    gVal.Code = 5;
                }
               
                return gVal;

            }
            catch (Exception)
            {

                gVal.Code = -1;
                gVal.Error = "Validation failed. Please provide all required fields and try again.";
                return gVal;
            }
        }

    }
}
