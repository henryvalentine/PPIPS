using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Helpers;
using ImportPermitPortal.Services.Services;

namespace ImportPermitPortal.Controllers
{
     [Authorize]
    public class BankController : Controller
    {
        public ImporterObject GetLoggedOnCompanyInfo()
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
       [Authorize(Roles = "Super_Admin")]
       public ActionResult GetBankObjects(JQueryDataTableParamModel param)
       {
           try
           {
               IEnumerable<BankObject> filteredParentMenuObjects;
               var countG = 0;
               
               var pagedParentMenuObjects = GetBanks(param.iDisplayLength, param.iDisplayStart, out countG);

               if (!string.IsNullOrEmpty(param.sSearch))
               {
                   filteredParentMenuObjects = new BankServices().Search(param.sSearch);
               }
               else
               {
                   filteredParentMenuObjects = pagedParentMenuObjects;
               }

               if (!filteredParentMenuObjects.Any())
               {
                   return Json(new List<BankObject>(), JsonRequestBehavior.AllowGet);
               }

               var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
               Func<BankObject, string> orderingFunction = (c => sortColumnIndex == 1 ? c.Name : sortColumnIndex == 2 ? c.SortCode : c.LastName);
                
               var sortDirection = Request["sSortDir_0"]; // asc or desc
               filteredParentMenuObjects = sortDirection == "desc" ? filteredParentMenuObjects.OrderBy(orderingFunction) : filteredParentMenuObjects.OrderByDescending(orderingFunction);

               var displayedPersonnels = filteredParentMenuObjects;

               var result = from c in displayedPersonnels
                            select new[] { Convert.ToString(c.BankId), c.Name, c.SortCode, c.NotificationEmail, c.LastName};
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
               return Json(new List<BankObject>(), JsonRequestBehavior.AllowGet);
           }
       }

       [HttpGet]
       [Authorize(Roles = "Banker")]
       public ActionResult GetBankBranchObjects(JQueryDataTableParamModel param)
       {
           try
           {
               var importerInfo = GetLoggedOnUserInfo();
               if (importerInfo == null || importerInfo.Id < 1)
               {
                   return Json(new List<BankBranchObject>(), JsonRequestBehavior.AllowGet);
               }

               IEnumerable<BankBranchObject> filteredParentMenuObjects;
               var countG = 0;

               var pagedParentMenuObjects = GetBankBranches(param.iDisplayLength, param.iDisplayStart, out countG, importerInfo.Id);

               if (!string.IsNullOrEmpty(param.sSearch))
               {
                   filteredParentMenuObjects = new BankServices().SearchBankBranches(param.sSearch, importerInfo.Id);
               }
               else
               {
                   filteredParentMenuObjects = pagedParentMenuObjects;
               }

               if (!filteredParentMenuObjects.Any())
               {
                   return Json(new List<BankBranchObject>(), JsonRequestBehavior.AllowGet);
               }

               var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
               Func<BankBranchObject, string> orderingFunction = (c => sortColumnIndex == 1 ? c.Name : c.BranchCode);

               var sortDirection = Request["sSortDir_0"]; // asc or desc
               filteredParentMenuObjects = sortDirection == "desc" ? filteredParentMenuObjects.OrderBy(orderingFunction) : filteredParentMenuObjects.OrderByDescending(orderingFunction);

               var displayedPersonnels = filteredParentMenuObjects;

               var result = from c in displayedPersonnels
                            select new[] { Convert.ToString(c.Id), c.Name, c.BranchCode};
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
               return Json(new List<BankBranchObject>(), JsonRequestBehavior.AllowGet);
           }
       }

       [HttpGet]
       [Authorize(Roles = "Banker")]
       public ActionResult GetBankUsers(JQueryDataTableParamModel param)
       {
           try
           {
               var importerInfo = GetLoggedOnCompanyInfo();
               if (importerInfo == null || importerInfo.Id < 1)
               {
                   return Json(new List<UserProfileObject>(), JsonRequestBehavior.AllowGet);
               }

               IEnumerable<UserProfileObject> filteredUsers;
               var countG = 0;

               var pagedParentMenuObjects = GetBankUsers(param.iDisplayLength, param.iDisplayStart, out countG, importerInfo.Id);

               if (!string.IsNullOrEmpty(param.sSearch))
               {
                   filteredUsers = new BankServices().SearchUsers(param.sSearch, importerInfo.Id);
               }
               else
               {
                   filteredUsers = pagedParentMenuObjects;
               }

               if (!filteredUsers.Any())
               {
                   return Json(new List<UserProfileObject>(), JsonRequestBehavior.AllowGet);
               }

               var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
               Func<UserProfileObject, string> orderingFunction = (c => sortColumnIndex == 1 ? c.LastName : sortColumnIndex == 2 ? c.FirstName.ToString(CultureInfo.InvariantCulture) :
                 sortColumnIndex == 3 ? c.Email : sortColumnIndex == 4 ? c.PhoneNumber : c.StatusStr);

               var sortDirection = Request["sSortDir_0"]; // asc or desc
               filteredUsers = sortDirection == "desc" ? filteredUsers.OrderBy(orderingFunction) : filteredUsers.OrderByDescending(orderingFunction);

               var displayedPersonnels = filteredUsers;

               var result = from c in displayedPersonnels
                            select new[] { Convert.ToString(c.Id), c.LastName, c.FirstName, c.Email,c.PhoneNumber, c.StatusStr
                             };
               return Json(new
               {
                   param.sEcho,
                   iTotalRecords = countG,
                   iTotalDisplayRecords = filteredUsers.Count(),
                   aaData = result
               },
              
               JsonRequestBehavior.AllowGet);

           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return Json(new List<UserProfileObject>(), JsonRequestBehavior.AllowGet);
           }
       }

       [HttpGet]
       public ActionResult GetUsersByBank(int id)
       {
           try
           {
               var users = GetBankUsersByBank(id);
               return Json(users, JsonRequestBehavior.AllowGet);
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return Json(new List<UserProfileObject>(), JsonRequestBehavior.AllowGet);
           }
       }
         
       private GenericValidator AddCompany(ImporterObject company)
        {
            var gVal = new GenericValidator();

            try
            {
                var appStatus = new ImporterServices().AddImporter(company);
                if (appStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = appStatus == -2 ? "Company failed. Please try again." : "Company Information already exists";
                    return gVal;
                }

                gVal.Code = appStatus;
                gVal.Error = "Company was successfully added.";
                return gVal;
                
            }
            catch (Exception)
            {
                gVal.Error = "Company processing failed. Please try again later";
                gVal.Code = -1;
                return gVal;
            }
        }

       public GenericValidator EditCompany(ImporterObject company)
       {
           var gVal = new GenericValidator();

           try
           {
               var appStatus = new ImporterServices().UpdateImporter(company);
               if (appStatus < 1)
               {
                   gVal.Code = -1;
                   gVal.Error = appStatus == -2 ? "Company failed. Please try again." : "Company Information already exists";
                   return gVal;
               }

               gVal.Code = appStatus;
               gVal.Error = "Company was successfully updated.";
               return gVal;

           }
           catch (Exception)
           {
               gVal.Error = "Company processing failed. Please try again later";
               gVal.Code = -1;
               return gVal;
           }
       }

       [Authorize(Roles = "Super_Admin")]
       [HttpPost]
        public ActionResult AddBank(BankObject bank)
        {
            var gVal = new GenericValidator();

            try
            {
                var validationResult = ValidateBank(bank);

                if (validationResult.Code == 1)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                var company = new ImporterObject
                {
                    Id = 0,
                    Name = bank.Name,
                    TIN = bank.TIN,
                    RCNumber = bank.RCNumber,
                    IsActive = true,
                    DateAdded = DateTime.Now.ToString("dd/MM/yyyy")
                };
                var cmStatus = AddCompany(company);
                if (cmStatus.Code < 1)
                {
                    return Json(cmStatus, JsonRequestBehavior.AllowGet);
                }

                bank.ImporterId = cmStatus.Code;
                var appStatus = new BankServices().AddBank(bank);
                if (appStatus < 1)
                {
                    validationResult.Code = -1;
                    validationResult.Error = appStatus == -2 ? "Bank processing failed. Please try again." : "Bank Information already exists";
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = appStatus;
                gVal.Error = "Bank was successfully added.";
                return Json(gVal, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                gVal.Error = "Bank processing failed. Please try again later";
                gVal.Code = -1;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }
         
        [HttpPost]
        [Authorize(Roles = "Super_Admin")]
        public ActionResult EditBank(BankObject bank)
        {
            var gVal = new GenericValidator();

            try
            {
                var stat = ValidateBank(bank);

                if (stat.Code < 1)
                {
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                
                if (Session["_bank"] == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet); 
                }

                var oldbank = Session["_bank"] as BankObject;

                if (oldbank == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                 var company = new ImporterObject
                {
                    Id = oldbank.ImporterId,
                    Name = bank.Name,
                    TIN = bank.TIN,
                    RCNumber = bank.RCNumber
                };

                var cmStatus = EditCompany(company);
                if (cmStatus.Code < 1)
                {
                    return Json(cmStatus, JsonRequestBehavior.AllowGet);
                }
                
                oldbank.Name = bank.Name;
                oldbank.NotificationEmail = bank.NotificationEmail;
                oldbank.SortCode = bank.SortCode;
                var docStatus = new BankServices().UpdateBank(oldbank);
                if (docStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = docStatus == -3 ? "Bank already exists." : "Bank information could not be updated. Please try again later";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = oldbank.BankId;
                gVal.Error = "Bank information was successfully updated";
                return Json(gVal, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                gVal.Code = -1;
                gVal.Error = "Bank information could not be updated. Please try again later";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }
       
        [HttpPost]
        [Authorize(Roles = "Banker")]
        public ActionResult AddBankBranch(BankBranchObject bankBranch) 
        {
            var gVal = new GenericValidator();

            try
            {
                if (string.IsNullOrEmpty(bankBranch.Name) || string.IsNullOrEmpty(bankBranch.BranchCode))
                {
                    gVal.Code = -1;
                    gVal.Error = "All fields marked '*' are required.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                var importerInfo = GetLoggedOnUserInfo();
                if (importerInfo == null || importerInfo.Id < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Your session has timed out";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                bankBranch.ImporterId = importerInfo.Id;
                var appStatus = new BankServices().AddBankBranch(bankBranch);
                if (appStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = appStatus == -2 ? "Bank Branch Processing failed. Please try again." : "Bank Branch Information already exists";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = appStatus;
                gVal.Error = "Bank Branch was successfully added.";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                gVal.Error = "Bank Branch processing failed. Please try again later";
                gVal.Code = -1;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Banker")]
        public ActionResult UpdateBankBranch(BankBranchObject bankBranch)
        {
            var gVal = new GenericValidator();
            try
            {
                if (string.IsNullOrEmpty(bankBranch.Name) || string.IsNullOrEmpty(bankBranch.BranchCode))
                {
                    gVal.Code = -1;
                    gVal.Error = "All fields marked '*' are required.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                if (Session["_bankBranch"] == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var oldbankBranch = Session["_bankBranch"] as BankBranchObject;

                if (oldbankBranch == null || oldbankBranch.Id < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                oldbankBranch.Name = bankBranch.Name.Trim();
                oldbankBranch.BranchCode = bankBranch.BranchCode.Trim();
                var appStatus = new BankServices().UpdateBankBranch(oldbankBranch);
                if (appStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = appStatus == -2 ? "Bank Branch Processing failed. Please try again." : "Bank Branch Information already exists";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = appStatus;
                gVal.Error = "Bank Branch was successfully updated.";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }

            catch (Exception)
            {
                gVal.Error = "Bank Branch processing failed. Please try again later";
                gVal.Code = -1;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetBanks()
        {
            try
            {
                return Json(GetBankLists(), JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(new GenericEligibilityList(), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetBank(long id)
        {
            try
            {
                var bank = new BankServices().GetBank(id);
                if (bank == null || bank.BankId < 1)
                {
                    return Json(new BankObject(), JsonRequestBehavior.AllowGet);
                }

                Session["_bank"] = bank;

                return Json(bank, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                return Json(new BankObject(), JsonRequestBehavior.AllowGet);
            }
        }

   
        [Authorize(Roles = "Banker")]
        public ActionResult GetBankBranch(int id)
        {
            try
            {
                if (id < 1)
                {
                    return Json(new BankBranchObject(), JsonRequestBehavior.AllowGet);
                }

                var bank = new BankServices().GetBankBranch(id);
                if (bank == null || bank.BankId < 1)
                {
                    return Json(new BankBranchObject(), JsonRequestBehavior.AllowGet);
                }

                Session["_bankBranch"] = bank;

                return Json(bank, JsonRequestBehavior.AllowGet);

            }

            catch (Exception)
            {
                return Json(new BankBranchObject(), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetBankBranches()
        {
            try
            {
                var importerInfo = GetLoggedOnUserInfo();
                if (importerInfo == null || importerInfo.Id < 1)
                {
                    return Json(new List<BankBranchObject>(), JsonRequestBehavior.AllowGet);
                }
                var branches = new BankServices().GetBankBranches(importerInfo.Id);
                return Json(branches, JsonRequestBehavior.AllowGet);
            }

            catch (Exception)
            {
                return Json(new BankBranchObject(), JsonRequestBehavior.AllowGet);
            }
        }
      
        private List<UserProfileObject> GetBankUsers(int? itemsPerPage, int? pageNumber, out int countG, long Id)
        {
            try
            {
                var users = new BankServices().GetBankUsers(itemsPerPage, pageNumber, out countG, Id);
                if (!users.Any())
                {
                    countG = 0;
                    return new List<UserProfileObject>();
                }

                return users;

            }
            catch (Exception)
            {
                countG = 0;
                return new List<UserProfileObject>();
            }
        }

        private List<UserProfileObject> GetBankUsersByBank(int bankId)
        {
            try
            {
                return new BankServices().GetBankUsersByBank(bankId) ?? new List<UserProfileObject>();
            }
            catch (Exception)
            {
                return new List<UserProfileObject>();
            }
        }

        public ActionResult GetBankAdmin(int id)
        {
            try
            {
                var bankAdmin = new BankServices().GetBankAdmin(id);
                if (bankAdmin == null || bankAdmin.BankId < 1)
                {
                    return Json(new UserProfileObject(), JsonRequestBehavior.AllowGet);
                }

                Session["_bankAdmin"] = bankAdmin;

                return Json(bankAdmin, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(new UserProfileObject(), JsonRequestBehavior.AllowGet);
            }
        }

         [Authorize(Roles = "Super_Admin")]
         [HttpPost]
        public ActionResult DeleteBank(long id)
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
                var delStatus = new BankServices().DeleteBank(id);
                if (delStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Bank could not be deleted. Please try again later.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = 5;
                gVal.Error = "Bank Information was successfully deleted";
                return Json(gVal, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }
         
        private GenericValidator ValidateBank(BankObject bank)
        {
            var gVal = new GenericValidator();
            try
            {
                if (string.IsNullOrEmpty(bank.Name))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide Bank Name.";
                    return gVal;
                }

                if (string.IsNullOrEmpty(bank.SortCode))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide Bank Sort Code.";
                    return gVal;
                }

                gVal.Code = 5;
                return gVal;
                
            }
            catch (Exception)
            {
                
                gVal.Code = -1;
                gVal.Error = "Bank Validation failed. Please provide all required fields and try again.";
                return gVal;
            }
        }

        private List<BankObject> GetBanks(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                return new BankServices().GetBanks(itemsPerPage, pageNumber, out countG);
            }
            catch (Exception)
            {
                countG = 0;
                return new List<BankObject>();
            }
        }
        private List<BankBranchObject> GetBankBranches(int? itemsPerPage, int? pageNumber, out int countG, long impoterId)
        {
            return new BankServices().GetBankBranches(itemsPerPage, pageNumber, out countG, impoterId);
        }
        private List<BankObject> GetBankLists()
        {
            try
            {
                return new BankServices().GetBanks();

            }
            catch (Exception)
            {
                return new List<BankObject>();
            }
        }

        private List<DocumentTypeObject> GetDocumentTypes()
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
         
        public bool DownloadContentFromFolder(string path)
        {
            try
            {
                Response.Clear();
                var filename = Path.GetFileName(path);
                HttpContext.Response.Buffer = true;
                HttpContext.Response.Charset = "";
                HttpContext.Response.Cache.SetCacheability(HttpCacheability.NoCache);
                Response.ContentType = GetMimeType(filename);
                HttpContext.Response.AddHeader("Content-Disposition", "attachment;filename=\"" + filename + "\"");
                Response.WriteFile(Server.MapPath(path));
                Response.End();
                return true;
            }

            catch (Exception ex)
            {
                return false;
            }
        }

        private static string GetMimeType(string fileName)
        {
            string mimeType = "application/unknown";
            var extension = Path.GetExtension(fileName);
            if (extension != null)
            {
                var ext = extension.ToLower();
                var regKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
                if (regKey != null && regKey.GetValue("Content Type") != null)
                    mimeType = regKey.GetValue("Content Type").ToString();
            }
            return mimeType;
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
    }
}
