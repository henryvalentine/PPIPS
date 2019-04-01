using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Configuration;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Web.Mvc;
using ImportPermitPortal.BizObjects;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.EF.Model;
using ImportPermitPortal.Helpers;
using ImportPermitPortal.Models;
using ImportPermitPortal.Services.Services;
using Mandrill;
using Mandrill.Model;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using Newtonsoft.Json;
using WebGrease.Css.Extensions;

namespace ImportPermitPortal.Controllers
{

    public class AccountController : Controller     
    {
        public AccountController() : this(new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext())))
        {
        }

        public AccountController(UserManager<ApplicationUser> userManager)
        {
            UserManager = userManager;
        }

        public UserManager<ApplicationUser> UserManager { get; private set; }

        
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.Title = "Login|DPR-PPIPS"; 
            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginViewModel());
        }

        private ImporterObject GetUserInfo()
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

        public ActionResult GetLoggedOnUserInfo()
        {
            var gVal = new GenericValidator();
            try
            {
                //if (!Request.IsAuthenticated)
                //{
                //    gVal.IsAuthenticated = false;
                //    gVal.UserName = "Your are not authorized to view this content";
                //    return Json(gVal, JsonRequestBehavior.AllowGet);
                //}

                if (Session["_importerInfo"] == null)
                {
                    gVal.IsAuthenticated = false;
                    gVal.UserName = "Your are not authorized to view this content";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var importerInfo = Session["_importerInfo"] as ImporterObject;
                if (importerInfo == null || importerInfo.Id < 1)
                {
                    gVal.IsAuthenticated = false;
                    gVal.UserName = "Your are not authorized to view this content or Your Session has timed out";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.IsAuthenticated = true;
                gVal.UserName = importerInfo.UserProfileObject.FirstName;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                gVal.IsAuthenticated = false;
                gVal.UserName = "Your Session has timed out";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        public ImporterObject GetLoggedOnImporterInfo()
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
        public ActionResult GetAppUsers(JQueryDataTableParamModel param) 
        {
            try
            {  
                IEnumerable<UserProfileObject> filteredParentMenuObjects;
                var countG = 0;
                var importerInfo = GetLoggedOnImporterInfo();
                if (importerInfo.Id < 1)
                {
                    return Json(new List<UserProfileObject>(), JsonRequestBehavior.AllowGet);
                }
                var pagedParentMenuObjects = new EmployeeDeskServices().GetAppUsers(param.iDisplayLength, param.iDisplayStart, out countG, importerInfo.Id);

                if (!string.IsNullOrEmpty(param.sSearch))
                {
                    filteredParentMenuObjects = new EmployeeDeskServices().SearchAppUsers(param.sSearch, importerInfo.Id);
                }
                else
                {
                    filteredParentMenuObjects = pagedParentMenuObjects;
                }

                if (!filteredParentMenuObjects.Any())
                {
                    return Json(new List<UserProfileObject>(), JsonRequestBehavior.AllowGet);
                }

                filteredParentMenuObjects.ForEach(m =>
                {
                    var roles = UserManager.GetRoles(m.UserId);
                    if (roles.Any())
                    {
                        m.Role = roles[0];
                    }
                });
                
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<UserProfileObject, string> orderingFunction = (c => sortColumnIndex == 1 ? c.Name : sortColumnIndex == 2 ? c.PhoneNumber : sortColumnIndex == 3 ? c.Email : sortColumnIndex == 4 ? c.Role : c.StatusStr);

                var sortDirection = Request["sSortDir_0"]; // asc or desc
                filteredParentMenuObjects = sortDirection == "desc" ? filteredParentMenuObjects.OrderBy(orderingFunction) : filteredParentMenuObjects.OrderByDescending(orderingFunction);

                var displayedPersonnels = filteredParentMenuObjects;

                var result = from c in displayedPersonnels
                             select new[] { Convert.ToString(c.Id), c.Name, c.Email, c.PhoneNumber, c.Role, c.StatusStr };     
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
                return Json(new List<UserProfileObject>(), JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize(Roles = "Banker")]
        public ActionResult GetBankUser(long id)
        {
            try
            {
                if (id < 1)
                {
                    return Json(new UserProfileObject(), JsonRequestBehavior.AllowGet);
                }

                var bankUser = new BankServices().GetBankUser(id);
                if (bankUser == null || bankUser.Id < 1)
                {
                    return Json(new UserProfileObject(), JsonRequestBehavior.AllowGet);
                }

                var userRoles = UserManager.GetRoles(bankUser.UserId);
                if (!userRoles.Any())
                {
                    return Json(new UserProfileObject(), JsonRequestBehavior.AllowGet);
                }

                if (userRoles.Any(r => r == "Banker"))
                {
                    bankUser.RoleId = (int) BankRoleEnum.Banker;
                }
                else
                {
                    if (userRoles.Any(r => r == "Bank_User"))
                    {
                        bankUser.RoleId = (int)BankRoleEnum.Bank_User;
                    }
                }

                Session["_bankUser"] = bankUser;

                return Json(bankUser, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(new UserProfileObject(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Super_Admin")]
        public async Task<ActionResult> AddEmployeeDesk(EmployeeDeskObject employeeDesk)
        {
            var rep = new Reporter();
            try
            {


                if (employeeDesk == null)
                {
                    rep.IsNull = true;
                    return Json(rep, JsonRequestBehavior.AllowGet);
                }

                var ttxd = UserManager.FindByEmail(employeeDesk.Email);
                if (ttxd != null)
                {
                    rep.IsEmail = true;
                    return Json(rep, JsonRequestBehavior.AllowGet);
                }

                UserManager.UserLockoutEnabledByDefault = true;
                UserManager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
                UserManager.MaxFailedAccessAttemptsBeforeLockout = 5;


                using (var db = new ImportPermitEntities())
                {
                    var aspnetId = User.Identity.GetUserId();

                    //get the id of the userprofile table
                    var registeredGuys = db.AspNetUsers.Find(aspnetId);
                    var profileId = registeredGuys.UserProfile.Id;
                    var profile = db.UserProfiles.Find(profileId);

                    //get importer id for employee

                    var importerId = profile.Person.ImporterId;

                    //create the person
                    var personInfo = new Person
                    {
                        FirstName = employeeDesk.FirstName,
                        LastName = employeeDesk.LastName,
                        IsAdmin = false,
                        ImporterId = importerId
                    };

                    var person = db.People.Add(personInfo);
                    db.SaveChanges();

                    var user = new ApplicationUser
                    {
                        UserName = employeeDesk.Email,
                        Email = employeeDesk.Email,
                        PhoneNumber = employeeDesk.Phone,

                        UserInfo = new ApplicationDbContext.UserProfile
                        {
                            IsActive = true,
                            IsAdmin = false,
                            PersonId = person.Id
                        }
                    };

                    var rad = new Random();
                    var num = rad.Next();

                    var result = await UserManager.CreateAsync(user, num.ToString());


                    //add employee to desk
                    var desk = new EmployeeDesk
                    {
                        EmployeeId = user.UserInfo.Id,
                        ActivityTypeId = employeeDesk.ActivityTypeId,
                        CreatedAt = DateTime.Now,
                        GroupId = employeeDesk.GroupId,
                        IsUserAvailable = true,
                        JobCount = 0,
                        ZoneId = employeeDesk.ZoneId,
                        ApplicationCount = 0,
                        NotificationCount = 0,
                        RecertificationCount = 0
                    };

                    db.EmployeeDesks.Add(desk);
                    db.SaveChanges();
                    if (result.Succeeded)
                    {
                        UserManager.AddToRole(user.Id, "Employee");


                        var usrModel = new UserViewModel
                        {
                            IsUser = true,
                            SecurityStamp = user.SecurityStamp,
                            LastName = employeeDesk.LastName,
                            Email = user.Email,
                            Password = num.ToString(),
                            PhoneNumber = employeeDesk.Phone,
                            Id = user.UserInfo.Id
                        };
                        var status = await SendMail(usrModel);
                        if (!status)
                        {
                            rep.IsMailSent = true;
                            return Json(rep, JsonRequestBehavior.AllowGet);
                        }

                        rep.IsMailSent = false;
                        return Json(rep, JsonRequestBehavior.AllowGet);

                    }


                    rep.IsUserRegistered = false;
                    return Json(rep, JsonRequestBehavior.AllowGet);

                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                rep.IsError = true;
                return Json(rep, JsonRequestBehavior.AllowGet);
            }
        }
        
        public ActionResult PopulateDepot()
        {

            var gVal = new GenericValidator();
            try
            {
                var filePath = HostingEnvironment.MapPath(Path.Combine("~/BulkUploads", "depotLicenses.json"));
                if (string.IsNullOrEmpty(filePath))
                {
                    gVal.Code = -1;
                    gVal.Error = "Path Error.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                List<LicenseRefObject> licenses;
                var serializer = new JsonSerializer();
                using (var re = System.IO.File.OpenText(filePath))
                using (var reader = new JsonTextReader(re))
                {
                    licenses = serializer.Deserialize<List<LicenseRefObject>>(reader);
                }


                var depotList = new List<DepotObject>();

                licenses.ForEach(x =>
                {
                    depotList.Add(new DepotObject
                    {
                        Id = 0,
                        Name = x.ImporterName,
                        JettyId = x.JettyId,
                        DepotLicense = x.RefCode,
                        IssueDate = x.IssueDate,
                        ExpiryDate = x.ExpiryDate,
                        Status = x.Status
                    });
                });
                var successCount = depotList.Count();
                var status = new DepotServices().AddDepot(depotList);
                if (status < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = " Error.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = status;
                gVal.Error = successCount == status ? "Depot was successfully populated with +" + depotList.Count() + " Items." : "Upload was not successfull";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                ErrorLogger.LoggError(e.StackTrace, e.Source, e.Message);
                gVal.Code = -1;
                gVal.Error = "Uploads failed";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            ViewBag.Title = "Login|DPR-PPIPS";
            try
            {
                var baseUrl = Request.Url.Scheme + "://" + Request.Url.Authority;

                if (ModelState.IsValid)
                {
                    var user = await UserManager.FindAsync(model.Email, model.Password);
                    if (user == null || string.IsNullOrEmpty(user.Id))
                    {

                        ViewBag.RegisterationSuccessful = "User information could not be verified. Please contact an Administrator.";
                        ViewBag.ConfirmStat = -1;
                        return View(model);
                    }

                    if (!user.EmailConfirmed)
                    {
                       ViewBag.RegisterationSuccessful = "Please go to your email and click on the activation link sent to you to activate your account.";
                       ViewBag.ConfirmStat = -1;
                        return View(model);
                    }

                    if (string.IsNullOrEmpty(user.Id))
                    {
                        ViewBag.RegisterationSuccessful = "User information could not be verified. Please contact an Administrator.";
                        ViewBag.ConfirmStat = -1;
                        return View(model);
                    }

                    var userRoles = UserManager.GetRoles(user.Id);
                    if (!userRoles.Any())
                    {
                        ViewBag.RegisterationSuccessful = "Invalid account. Please contact an Administrator.";
                        ViewBag.ConfirmStat = -1;
                        return View(model);
                    }

                    var isApplicant = userRoles.Any(m => m == "Applicant");
                    var profile = (userRoles.Any(m => m == "Super_Admin")) ? GetAdminUserImporter(user.Id) : GetUserImporter(user.Id, isApplicant);

                    if (profile == null || profile.Id < 1)
                    {
                        ViewBag.RegisterationSuccessful = "Your account information could not be retrieved. Please try again.";
                        ViewBag.ConfirmStat = -1;
                        return View(model);
                    }

                    if (!profile.UserProfileObject.IsActive)
                    {
                        ViewBag.RegisterationSuccessful =  "Your account is deactivated. Please contact an administrator.";
                        ViewBag.ConfirmStat = -1;
                        return View(model);
                    }

                    await SignInAsync(user, model.RememberMe);
                    Session["_importerInfo"] = profile;

                    if (userRoles.Any(m => m == "Banker"))
                    {
                        return Redirect(baseUrl + "/bnkAd.html#/");
                    }
                    else
                    {
                        if (userRoles.Any(m => m == "Bank_User"))
                        {

                            return Redirect(baseUrl + "/bnkUsr.html#/");
                        }
                        else
                        {
                            if (userRoles.Any(m => m == "Super_Admin"))
                            {

                                return Redirect(baseUrl + "/ngx.html#/");
                            }
                            else
                            {
                                if (userRoles.Any(m => m == "Depot_Owner"))
                                {

                                    return Redirect(baseUrl + "/depot_Owner.html#/");
                                }
                                else
                                {
                                    if (userRoles.Any(m => m == "Employee"))
                                    {
                                        if (userRoles.Any(m => m == "DownstreamDirector"))
                                        {

                                            return Redirect(baseUrl + "/ngappr.html#/");
                                        }

                                        if (userRoles.Any(m => m == "Verifier"))
                                        {
                                            return Redirect(baseUrl + "/ngVf.html#/");
                                        }

                                        return Redirect(baseUrl + "/nge.html#/");
                                    }
                                    else
                                    {
                                        if (userRoles.Any(m => m == "Support"))
                                        {
                                            return Redirect(baseUrl + "/ngs.html#/");
                                        }
                                        else
                                        {
                                            if (userRoles.Any(m => m == "ICT"))
                                            {
                                                return Redirect(baseUrl + "/ngi.html#/");
                                            }
                                            else
                                            {
                                                if (userRoles.Any(m => m == "Accounts"))
                                                {
                                                    return Redirect(baseUrl + "/nga.html#/");
                                                }
                                                else
                                                {
                                                    if (userRoles.Any(m => m == "Verifier"))
                                                    {
                                                        return Redirect(baseUrl + "/ngVf.html#/");
                                                    }
                                                    return RedirectToLocal(returnUrl);
                                                }
                                                
                                            }
                                        }
                                    }
                                    
                                }
                            }
                           
                        }
                    }

                }
                ViewBag.RegisterationSuccessful =  "Invalid process call.";
                ViewBag.ConfirmStat = -1;
                return View(model);
            }
            catch (Exception e)
            {
                ModelState.AddModelError("", e.InnerException.Message);
                ViewBag.ConfirmStat = -1;
                return View(model);
            }
        }

        private async Task<GenericValidator> AddUser(RegisterViewModel model)
        {
            var gVal = new GenericValidator();
            try
            {
                var company = new ImporterObject
                {
                    Name = model.ImporterName,
                    TIN = model.TIN,
                    RCNumber = model.RCNumber
                };

                var ttxd = UserManager.FindByEmail(model.Email);

                if (ttxd != null)
                {
                    gVal.Code = -1;
                    gVal.Error = "The Email address " + model.Email + " is already taken";
                    return gVal;
                }

                var ttxy = UserManager.Users.Count(m => m.PhoneNumber.Trim().Replace(" ", "") == model.PhoneNumber.Trim().Replace(" ", ""));
                if (ttxy > 0)
                {
                    gVal.Code = -1;
                    gVal.Error = "The Phone number " + model.PhoneNumber + " is already taken";
                    return gVal;
                }

                UserManager.UserLockoutEnabledByDefault = true;
                UserManager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
                UserManager.MaxFailedAccessAttemptsBeforeLockout = 5;

                var processedImporterStatus = new ImporterServices().AddImporter(company);
                if (processedImporterStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Registration could not be processed. Please try again later.";
                    return gVal;
                }

                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,

                    UserInfo = new ApplicationDbContext.UserProfile
                    {
                        Id = processedImporterStatus,
                        IsActive = true,
                        IsAdmin = true
                    }
                };

                var rad = new Random();
                var password = rad.Next().ToString();

                var result = await UserManager.CreateAsync(user, password);

                if (result.Succeeded)
                {
                    UserManager.AddToRole(user.Id, "Applicant");


                    var usrModel = new UserViewModel
                    {
                        IsUser = true,
                        SecurityStamp = user.SecurityStamp,
                        LastName = model.LastName,
                        Email = user.Email,
                        Password = password,
                        PhoneNumber = model.PhoneNumber,
                        Id = user.UserInfo.Id
                    };
                    var status = await SendMail(usrModel);
                    if (!status)
                    {
                        gVal.Code = -3;
                        gVal.Error = "User Registeration was successfully completed, but a notification email could not be sent to the user. Please pass the following information to the user: ";
                        return gVal;
                    }

                    gVal.Code = 5;
                    gVal.Error = "Registeration was successfully completed. Please login with your credentials";
                    return gVal;
                }
                gVal.Error = "User could not be created. Please try again.";
                gVal.UserId = user.Id;
                return gVal;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                gVal.Error = "User could not be created. Please try again.";
                return gVal;
            }
        }

        private async Task<string> GetAccessToken(AccessModel model)
        {
            try
            {
                var accessToken = await ServiceRequest.GetUserAccessToken(model);
                if (accessToken == null || string.IsNullOrEmpty(accessToken.AccessToken))
                {
                    return "";
                }

                Session["_token"] = accessToken.AccessToken;

                if (!model.IsExisting)
                {
                    var asciiStr = Encoding.ASCII.GetString(Convert.FromBase64String(accessToken.EXN_TGD));
                    var sysUser = JsonConvert.DeserializeObject<SystemUserObject>(asciiStr);
                    if (sysUser == null || string.IsNullOrEmpty(sysUser.Id))
                    {
                        return "";
                    }

                    var regModel = new RegisterViewModel
                    {
                        UserName = sysUser.Name,
                        FirstName = sysUser.FirstName,
                        LastName = sysUser.LastName,
                        Email = sysUser.Email,
                        PhoneNumber = sysUser.PhoneNumber,
                        ImporterName = sysUser.CompanyName,
                        TIN = sysUser.TIN,
                        IsUser = sysUser.IsUser,
                        RCNumber = sysUser.RCNumber,
                    };

                    var res = await AddUser(regModel);
                    if (res.Code < 1)
                    {
                        ModelState.AddModelError("", "User information could not be verified. Please contact an Administrator.");
                        return "";
                    }
                    return res.UserId;
                }

                return accessToken.AccessToken;

            }
            catch (Exception e)
            {
                return "";
            }
        }

        private ImporterObject GetAdminUserImporter(string userId)
        {
            try
            {
                var importerInfo = new ImporterServices().GetAdminImporterUser(userId);
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

        private ImporterObject GetUserImporter(string userId, bool isApplicant)
        {
            try
            {
                var importerInfo = new ImporterServices().GetImporterByLoggedOnUser(userId, isApplicant);
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

        [AllowAnonymous]
        public ActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        [Authorize]
        public ActionResult Landing()
        {
            return View();
        }

        public ActionResult TransactionRedirect()
        {
            if (Session["_importerInfo"] == null)
            {
                return RedirectToAction("Login");
            }

            var importerInfo = Session["_importerInfo"] as ImporterObject;
            if (importerInfo == null || importerInfo.Id < 1)
            {
                return RedirectToAction("Login");
            }
            
            return Redirect("http://localhost:20017/ngy.html#/");
            
        }

        [HttpPost]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            var gVal = new GenericValidator();
            try
            {
                var validateResult = ValidateInput(model);
                if (validateResult.Code < 1)
                {
                    gVal.Code = validateResult.Code;
                    gVal.Error = validateResult.Error;
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var ttxd = UserManager.FindByEmail(model.Email);

                if (ttxd != null)
                {
                    gVal.Code = -1;
                    gVal.Error = "The Email address " + model.Email + " is already taken";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var ttxy = UserManager.Users.Count(m => m.PhoneNumber.Trim().Replace(" ", "") == model.PhoneNumber.Trim().Replace(" ", ""));
                if (ttxy > 0)
                {
                    gVal.Code = -1;
                    gVal.Error = "The Phone number " + model.PhoneNumber + " is already taken";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                model.IsImporter = true;
                var accountStatus = await CreateAccount(model);

                if (accountStatus.Code < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = accountStatus.Error;
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                gVal.Code = 5;
                gVal.Error = accountStatus.Error;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                gVal.Code = -1;
                gVal.Error = "User could not be created. Please try again.";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Super_Admin")]
        public async Task<ActionResult> RegisterDepotInformation(UserProfileObject model) 
        {
            var gVal = new GenericValidator();
            try
            {
                var validateResult = ValidateInput(model);
                if (validateResult.Code < 1)
                {
                    gVal.Code = validateResult.Code;
                    gVal.Error = validateResult.Error;
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var ttxd = UserManager.FindByEmail(model.Email);

                if (ttxd != null)
                {
                    gVal.Code = -1;
                    gVal.Error = "The Email address " + model.Email + " is already taken";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var ttxy = UserManager.Users.Count(m => m.PhoneNumber.Trim().Replace(" ", "") == model.PhoneNumber.Trim().Replace(" ", ""));
                if (ttxy > 0)
                {
                    gVal.Code = -1;
                    gVal.Error = "The Phone number " + model.PhoneNumber + " is already taken";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
               
                var accountStatus = await CreateAccount(model);

                if (accountStatus.Code < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "User could not be created. Please try again.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                gVal.Code = 5;
                gVal.Error = accountStatus.Error;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                gVal.Code = -1;
                gVal.Error = "User could not be created. Please try again.";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }
        
        public ActionResult GetMyMessageObjects(JQueryDataTableParamModel param)
        {
            try
            {
                var importerInfo = GetUserInfo();
                if (importerInfo == null || importerInfo.Id < 1)
                {
                    return Json(new List<MessageObject>(), JsonRequestBehavior.AllowGet);
                }
                IEnumerable<MessageObject> filteredParentMenuObjects;
                var countG = 0;

                var pagedParentMenuObjects = new MessageServices().GetMyMessages(param.iDisplayLength, param.iDisplayStart, out countG, importerInfo.UserProfileObject.Id);

                if (!string.IsNullOrEmpty(param.sSearch))
                {
                    filteredParentMenuObjects = new MessageServices().SearchMessages(param.sSearch, importerInfo.UserProfileObject.Id);
                    countG = filteredParentMenuObjects.Count();

                }
                else
                {
                    filteredParentMenuObjects = pagedParentMenuObjects;

                }

                if (!filteredParentMenuObjects.Any())
                {
                    return Json(new List<MessageObject>(), JsonRequestBehavior.AllowGet);
                }

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<MessageObject, string> orderingFunction = (c => sortColumnIndex == 1 ? c.EventTypeName : c.DateSentStr);

                var sortDirection = Request["sSortDir_0"]; // asc or desc
                filteredParentMenuObjects = sortDirection == "desc" ? filteredParentMenuObjects.OrderBy(orderingFunction) : filteredParentMenuObjects.OrderByDescending(orderingFunction);

                var displayedPersonnels = filteredParentMenuObjects; //    

                var result = from c in displayedPersonnels
                             select new[] { Convert.ToString(c.Id),
                                 c.EventTypeName, c.DateSentStr
                             };
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
                return Json(new List<MessageObject>(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult GetMyMessageObject(long id)
        {
            try
            {
                if (id < 1)
                {
                    return Json(new MessageObject(), JsonRequestBehavior.AllowGet);
                }

                var message = GetMyMessage(id);
                return Json(message, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return Json(new MessageObject(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult GetMyLatestMessages()
        {
            try
            {
                var importerInfo = GetUserInfo();
                if (importerInfo == null || importerInfo.Id < 1)
                {
                    return Json(new List<MessageObject>(), JsonRequestBehavior.AllowGet);
                }

                var messages = new MessageServices().GetMyLatestMessages(importerInfo.UserProfileObject.Id);
                return Json(messages, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return Json(new List<MessageObject>(), JsonRequestBehavior.AllowGet);
            }
        }
        
        private List<MessageObject> GetMyMessages(long userId)
        {
            try
            {
                return new MessageServices().GetMyMessages(userId);
            }
            catch (Exception)
            {
                return new List<MessageObject>();
            }
        }

        private MessageObject GetMyMessage(long messageId)
        {
            try
            {
                return new MessageServices().GetMessage(messageId);
            }
            catch (Exception)
            {
                return new MessageObject();
            }
        }

        [HttpPost]
        public async Task<ActionResult> UpdateDepotAccount(UserProfileObject model)
        {
            var gVal = new GenericValidator();
            try
            {
                var validateResult = ValidateInputUpdate(model);
                if (validateResult.Code < 1)
                {
                    gVal.Code = validateResult.Code;
                    gVal.Error = validateResult.Error;
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                if (Session["_usProfObj"] == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Your session has timed out";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var account = Session["_usProfObj"] as UserProfileObject;

                if (account == null || account.CompanyId < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Your session has timed out";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var ttxd = UserManager.FindByEmail(model.Email);

                if (ttxd != null && account.UserId != ttxd.Id)
                {
                    gVal.Code = -1;
                    gVal.Error = "The Email address " + model.Email + " is already taken";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var ttxy = UserManager.Users.Count(m => m.PhoneNumber.Trim().Replace(" ", "") == model.PhoneNumber.Trim().Replace(" ", "") && account.UserId != m.Id);
                if (ttxy > 0 )
                {
                    gVal.Code = -1;
                    gVal.Error = "The Phone number " + model.PhoneNumber + " is already taken";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                account.PhoneNumber = model.PhoneNumber;
                account.FirstName = model.FirstName;
                account.LastName = model.LastName;
                account.Email = model.Email;
                account.IsActive = model.IsActive;
                account.JettyId = model.JettyId;
                account.DepotLicense = model.DepotLicense;
                account.IssueDate = model.IssueDate;
                account.ExpiryDate = model.ExpiryDate;
                account.Status = model.Status;

                var accountStatus = await UpdateAccount(account);

                if (accountStatus.Code < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Depot information could not be updated. Please try again.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                gVal.Code = 5;
                gVal.Error = accountStatus.Error;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                gVal.Code = -1;
                gVal.Error = "User information could not be Updated. Please try again.";
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }
        
        private async Task<GenericValidator> CreateAccount(RegisterViewModel model)
        {
            var gVal = new GenericValidator();
            try
            {
                var company = new ImporterObject
                {
                    Name = model.ImporterName,
                    TIN = model.TIN.ToLower().Replace("tin", ""),
                    RCNumber = model.RCNumber.ToLower().Replace("rc", ""),
                    IsActive = true,
                    StructureId = model.StructureId,
                    DateAdded = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss tt")
                };

                var person = new PersonObject
                {
                    FirstName = model.ImporterName,
                    LastName = model.ImporterName,
                    IsAdmin = true,
                    IsImporter = model.IsImporter
                };

                long importerId;
                var personId = new ImporterServices().AddImporterAndPerson(company, person, out importerId);
                if (personId < 1 || importerId < 0)
                {
                    gVal.Code = -1;
                    gVal.Error = personId == -3 ? "Company Information already exists" : "Registration could not be processed. Please try again later.";
                    return gVal;
                }
               
                var stopDate = DateTime.Parse("2016-03-31");
                if (DateTime.Today <= stopDate)
                {
                   AddUserPermit(company.Name, importerId);
                }

                UserManager.UserLockoutEnabledByDefault = true;
                UserManager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
                UserManager.MaxFailedAccessAttemptsBeforeLockout = 5;

                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,

                    UserInfo = new ApplicationDbContext.UserProfile
                    {
                        IsActive = true,
                        IsAdmin = true,
                        PersonId = personId
                    }
                };

                var result = await UserManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    var usrModel = new UserViewModel
                    {
                        IsUser = false,
                        SecurityStamp = user.SecurityStamp,
                        LastName = model.ImporterName,
                        Email = model.Email,
                        Password = model.Password,
                        PhoneNumber = model.PhoneNumber,
                        Id = user.UserInfo.Id
                    };

                    UserManager.AddToRole(user.Id, "Applicant");

                    var status = await SendMail(usrModel);
                    if (!status)
                    {
                        gVal.Code = -3;
                        gVal.Error = "User Registeration was successfully completed, but a notification email could not be sent to the user. Please pass the following information to the user: ";
                        return gVal;
                    }
                    gVal.Code = 5;
                    gVal.Error = "Account successfully created.";
                    return gVal;
                }
                gVal.Code = -1;
                var errors = "";
                result.Errors.ForEach(e =>
                {
                    errors += e;
                });
                gVal.Error = errors;
                return gVal;
            }
            catch (Exception ex)
            {
                gVal.Code = -1;
                gVal.Error = "User could not be created. Please try again.";
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return gVal;
            }
        }

        private GenericValidator AddUserPermit(string importerName, long importerId)
        {
            var gVal = new GenericValidator();
            try
            {
                var filePath = System.Web.Hosting.HostingEnvironment.MapPath(Path.Combine("~/BulkUploads", "depotLicenses.json"));
                if (string.IsNullOrEmpty(filePath))
                {
                    return gVal;
                }

                List<LicenseRefObject> licenses;
                var serializer = new JsonSerializer();
                using (var re = System.IO.File.OpenText(filePath))
                using (var reader = new JsonTextReader(re))
                {
                    licenses = serializer.Deserialize<List<LicenseRefObject>>(reader);
                }

                //Honeywell Oil and Gas Limited

                var impName = importerName.ToLower().Trim();

                if (impName.Contains("ltd"))
                {
                    impName = impName.Replace("ltd", "limited");
                }
                
                var license = new LicenseRefObject();

                licenses.ForEach(x =>
                {
                    var nm = x.ImporterName.ToLower().Trim();

                    if (nm.Contains("ltd"))
                    {
                        nm = nm.Replace("ltd", "limited");
                    }

                    if (nm.Replace(" ", string.Empty) == impName.Replace(" ", string.Empty))
                    {
                        license = x;
                    }
                });

                if (string.IsNullOrEmpty(license.RefCode) || license.JettyId < 1)
                {
                    gVal.Code = -1;
                    return gVal;
                }

                var depot = new DepotObject
                {
                    Name = importerName,
                    JettyId =license.JettyId,
                    ImporterId = importerId,
                    DepotLicense = license.RefCode,
                    IssueDate =license.IssueDate,
                    ExpiryDate =license.ExpiryDate,
                    Status = license.Status
                };

                var status = new DepotServices().AddDepot(depot);
                if (status < 1)
                {
                    gVal.Code = -1;
                    return gVal;
                }
                gVal.Code = 5;
                return gVal;
            }
            catch (Exception e)
            {
                ErrorLogger.LoggError(e.StackTrace, e.Source, e.Message);
                gVal.Code = -1;
                return gVal;
            }
        }

        private async Task<GenericValidator> CreateAccount(UserProfileObject model)
        {
            var gVal = new GenericValidator();
            try
            {
                var personId = new ImporterServices().AddImporterDepotAndPerson(model);
                if (personId < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Registration could not be processed. Please try again later.";
                    return gVal;
                }

                UserManager.UserLockoutEnabledByDefault = true;
                UserManager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
                UserManager.MaxFailedAccessAttemptsBeforeLockout = 5;

                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,

                    UserInfo = new ApplicationDbContext.UserProfile
                    {
                        IsActive = true,
                        IsAdmin = true,
                        PersonId = personId
                    }
                };

                var rad = new Random();
                var password = rad.Next().ToString();

                var result = await UserManager.CreateAsync(user, password);

                if (result.Succeeded)
                {
                    var usrModel = new UserViewModel
                    {
                        IsUser = true,
                        SecurityStamp = user.SecurityStamp,
                        LastName = model.CompanyName,
                        Email = model.Email,
                        Password = password,
                        PhoneNumber = model.PhoneNumber,
                        Id = user.UserInfo.Id
                    };

                    UserManager.AddToRole(user.Id, "Depot_Owner");

                    var status = await SendMail(usrModel);
                    if (!status)
                    {
                        gVal.Code = -3;
                        gVal.Error = "User Registeration was successfully completed, but a notification email could not be sent to the user. Please pass the following information to the user: ";
                        return gVal;
                    }
                    gVal.Code = 5;
                    gVal.Error = "Registeration was successfully completed. A confirmation email has been sent to " + model.Email;
                    return gVal;
                }
                gVal.Code = -1;
                var errors = "";
                result.Errors.ForEach(e =>
                {
                    errors += e;
                });
                gVal.Error = errors;
                return gVal;
            }
            catch (Exception ex)
            {
                gVal.Code = -1;
                gVal.Error = "User could not be created. Please try again.";
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return gVal;
            }
        }

        private async Task<GenericValidator> UpdateAccount(UserProfileObject model)
        {
            var gVal = new GenericValidator();
            try
            {
                var user = UserManager.FindById(model.UserId);
                if (user == null || string.IsNullOrEmpty(user.Id))
                {
                    gVal.Code = -1;
                    gVal.Error = "User information could not be verified. Please try again later.";
                    return gVal;
                }

                user.UserName = model.Email;
                user.Email = model.Email;
                user.PhoneNumber = model.PhoneNumber;

                var result = await UserManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    var errorStr = result.Errors.Aggregate("", (current, error) => current + (error + " "));
                    gVal.Code = -1;
                    gVal.Error = errorStr;
                    return gVal;
                }

                var processedImporterStatus = new ImporterServices().UpdateImporterDepotAndPerson(model);
                if (processedImporterStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Processed failed. Please try again later.";
                    return gVal;
                }
                
                gVal.Code = 5;
                gVal.Error = "User Account information was successfully updated.";
                return gVal;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                gVal.Code = -1;
                gVal.Error = "User infromation could not be updated. Please try again.";
                return gVal;
            }
        }

        private async Task<GenericValidator> UpdateUserAccount(UserProfileObject model)
        {
            var gVal = new GenericValidator();
            try
            {
                var user = UserManager.FindById(model.UserId);
                if (user == null || string.IsNullOrEmpty(user.Id))
                {
                    gVal.Code = -1;
                    gVal.Error = "User information could not be verified. Please try again later.";
                    return gVal;
                }

                user.UserName = model.Email;
                user.Email = model.Email;
                user.PhoneNumber = model.PhoneNumber;

                var result = await UserManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    var errorStr = result.Errors.Aggregate("", (current, error) => current + (error + " "));
                    gVal.Code = -1;
                    gVal.Error = errorStr;
                    return gVal;
                }

                var processedImporterStatus = new AspNetUserServices().UpdateUser(model);
                if (processedImporterStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Processed failed. Please try again later.";
                    return gVal;
                }

                gVal.Code = 5;
                gVal.Error = "User Account information was successfully updated.";
                return gVal;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                gVal.Code = -1;
                gVal.Error = "User infromation could not be updated. Please try again.";
                return gVal;
            }
        }

        public ActionResult AccountCreationSuccess()
        {
            if (Session["_mod"] == null)
            {
                ViewBag.Email = "";
                return View();
            }
            var model = Session["_mod"] as RegisterViewModel;
            if (model != null)
            {
                ViewBag.Email = model.Email;
                return View();
            }

            ViewBag.Email = "";
            return View();
        }

        [Authorize]
        public ActionResult RegisterUser()
        {
            return View(new UserViewModel());
        }

        [HttpPost]
        [Authorize(Roles = "Super_Admin")]
        public async Task<ActionResult> RegisterUser(UserViewModel model)
        {
            var gVal = new GenericValidator();
            try
            {
                var validateResult = ValidateAppUser(model);
                if (validateResult.Code < 1) 
                {
                    gVal.Code = -1;
                    gVal.Error = validateResult.Error;
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var importerInfo = GetLoggedOnImporterInfo();
                if (importerInfo.Id < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Your session has timed out. Please refresh the page.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var ttxd = UserManager.FindByEmail(model.Email);
                if (ttxd != null)
                {
                    gVal.Code = -1;
                    gVal.Error = "The Email address " + model.Email + " is already taken";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var ttxy = UserManager.Users.Count(m => m.PhoneNumber.Trim().Replace(" ", "") == model.PhoneNumber.Trim().Replace(" ", ""));
                if (ttxy > 0)
                {
                    gVal.Code = -1;
                    gVal.Error = "The Phone number " + model.PhoneNumber + " is already taken";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                
                 var personInfo = new PersonObject
                                    {
                                        FirstName = model.FirstName,
                                        LastName = model.LastName,
                                        IsAdmin = false,
                                        ImporterId = importerInfo.Id
                                    };

                 var personId = new AspNetUserServices().AddDprUser(personInfo);
                if (personId < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "User account could not be Created";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                UserManager.UserLockoutEnabledByDefault = true;
                UserManager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
                UserManager.MaxFailedAccessAttemptsBeforeLockout = 5;

                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,

                    UserInfo = new ApplicationDbContext.UserProfile  
                    {
                        IsActive = model.IsActive,
                        PersonId = personId,
                        IsAdmin = false
                    }
                };

                var rad = new Random();
                var password = rad.Next().ToString();

                var result = await UserManager.CreateAsync(user, password);

                if (result.Succeeded)
                {
                    UserManager.AddToRole(user.Id, model.Role);
                    model.IsUser = true;
                    model.Password = password;
                    model.SecurityStamp = user.SecurityStamp;
                    model.Id = user.UserInfo.Id;

                    var status = await SendMail(model);
                    if (!status)
                    {
                        gVal.Password = password;
                        gVal.Email = model.Email;
                        gVal.Code = 7;
                        gVal.Error = "User Registeration was successfully completed, but a notification email could not be sent to the user. Please pass the following information to the user: Password : " + password;
                        return Json(gVal, JsonRequestBehavior.AllowGet);
                    }

                    gVal.Code = 5;
                    gVal.Error = "User was successfully registered.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = -1;
                gVal.Error = "Please provide all required inputs and try again";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                gVal.Code = -1;
                gVal.Error = "User could not be created. Please try again.";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost]
        public async Task<ActionResult> UpdateUser(UserViewModel model)
        {
            var gVal = new GenericValidator();
            try
            {
                var validateResult = ValidateAppUser(model);
                if (validateResult.Code < 1)
                {
                    gVal.Code = validateResult.Code;
                    gVal.Error = validateResult.Error;
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                if (Session["_sysAppUser"] == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Your session has timed out";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var account = Session["_sysAppUser"] as UserProfileObject;

                if (account == null || account.Id < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Your session has timed out";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var ttxd = UserManager.FindByEmail(model.Email);

                if (ttxd != null && account.UserId != ttxd.Id)
                {
                    gVal.Code = -1;
                    gVal.Error = "The Email address " + model.Email + " is already taken";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var ttxy = UserManager.Users.Count(m => m.PhoneNumber.Trim().Replace(" ", "") == model.PhoneNumber.Trim().Replace(" ", "") && account.UserId != m.Id);
                if (ttxy > 0)
                {
                    gVal.Code = -1;
                    gVal.Error = "The Phone number " + model.PhoneNumber + " is already taken";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                account.PhoneNumber = model.PhoneNumber;
                account.FirstName = model.FirstName;
                account.LastName = model.LastName;
                account.Email = model.Email;
                account.IsActive = model.IsActive;

                var accountStatus = await UpdateUserAccount(account);

                if (accountStatus.Code < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Depot information could not be updated. Please try again.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                gVal.Code = 5;
                gVal.Error = accountStatus.Error;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                gVal.Code = -1;
                gVal.Error = "User information could not be Updated. Please try again.";
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetRoles()
        {
            try
            {
                var roles = EnumToObjList.ConvertEnumToList(typeof(SpecialRolesEnum));

                return Json(roles, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new List<ProductObject>(), JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize(Roles = "Super_Admin")]
        public ActionResult GetAppUser(long id)
        {
            var gVal = new GenericValidator();
            try
            {
                
                if (id < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Invalid selection!";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                
                var user = new EmployeeDeskServices().GetAppUser(id);
                if (user.Id < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "User information could not be retrieved.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                Session["_sysAppUser"] = user;

                var userRoles = UserManager.GetRoles(user.UserId);
                if (!userRoles.Any())
                {
                    gVal.Code = -1;
                    gVal.Error = "User information could not be retrieved.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                user.Role = userRoles[0];
                gVal.Code = 5;
                return Json(user, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                gVal.Code = -1;
                gVal.Error = "User information could not be retrieved. Please try again.";
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Super_Admin")]
        public async Task<ActionResult> RegisterBankAdmin(UserViewModel model)
        {
            var gVal = new GenericValidator();
            try
            {
                var validateResult = ValidateBankUser(model);
                if (validateResult.Code < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = validateResult.Error;
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                var branch = new BankBranchObject
                {
                    BranchCode = model.BranchCode,
                    Name = model.BankBranchName,
                    BankId = model.BankId
                };
                var bankBranchId = new BankServices().AddBankBranchForReg(branch);
                if (bankBranchId < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "User information could not be processed. Please try again later.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var ttxd = UserManager.FindByEmail(model.Email);
                if (ttxd != null)
                {
                    gVal.Code = -1;
                    gVal.Error = "The Email address " + model.Email + " is already taken";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var ttxy = UserManager.Users.Count(m => m.PhoneNumber.Trim().Replace(" ", "") == model.PhoneNumber.Trim().Replace(" ", ""));
                if (ttxy > 0)
                {
                    gVal.Code = -1;
                    gVal.Error = "The Phone number " + model.PhoneNumber + " is already taken";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                 var personObj = new UserProfileObject
                {
                    PhoneNumber = model.PhoneNumber,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    IsActive = model.IsActive,
                    CompanyId = model.ImporterId,
                    BankId = model.BankId
                  
                 };

                 var tt = new AspNetUserServices().AddUserAndPersonInfo(personObj);
                 if (tt < 1)
                 {
                     gVal.Code = -1;
                     gVal.Error = "User was updated but with errors";
                     return Json(gVal, JsonRequestBehavior.AllowGet);
                 } 

                UserManager.UserLockoutEnabledByDefault = true;
                UserManager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
                UserManager.MaxFailedAccessAttemptsBeforeLockout = 5;

                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,

                    UserInfo = new ApplicationDbContext.UserProfile
                    {
                        PersonId = tt,
                        IsActive = true,
                        IsAdmin = true
                    }
                };

                var rad = new Random();
                var password = rad.Next().ToString();

                var result = await UserManager.CreateAsync(user, password);

                if (!result.Succeeded)
                {
                    var errorStr = result.Errors.Aggregate("", (current, error) => current + (error + " "));
                    gVal.Code = -1;
                    gVal.Error = errorStr;
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                UserManager.AddToRole(user.Id, "Banker");

                var bankUser = new BankUserObject
                {
                    BranchId = bankBranchId,
                    UserId = user.UserInfo.Id
                };

                var bankUserStatus = new AspNetUserServices().AddBankUser2(bankUser);
                if (bankUserStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "User was updated but with errors";
                }

                model.IsUser = true;
                model.Password = password;
                model.SecurityStamp = user.SecurityStamp;
                model.Id = user.UserInfo.Id;
                var status = await SendMail(model);
                if (!status)
                {
                    gVal.Code = 7;
                    gVal.Password = password;
                    gVal.Email = model.Email;
                    gVal.Error = "User Registeration was successfully completed, but a notification email could not be sent to the user. Please pass the following information to the user: ";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = 5;
                gVal.Error = string.IsNullOrEmpty(gVal.Error) ? "User Registeration was successfully completed. An email has also been sent to notify the user." : gVal.Error;
                return Json(gVal, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                gVal.Code = -1;
                gVal.Error = "User could not be created. Please try again.";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Super_Admin")]
        public async Task<ActionResult> UpdateBankAdmin(UserViewModel model)
        {
            var gVal = new GenericValidator();
            try
            {
                var validateResult = ValidateBankUser(model);
                if (validateResult.Code < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = validateResult.Error;
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                if (Session["_bankAdmin"] == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var userInfo = Session["_bankAdmin"] as UserProfileObject;

                if (userInfo == null || userInfo.Id < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }


                var branch = new BankBranchObject
                {
                    Id = userInfo.BranchId,
                    BranchCode = model.BranchCode,
                    Name = model.BankBranchName,
                    BankId = model.BankId
                };

                var bankBranchId = 0;
                if (string.IsNullOrEmpty(userInfo.BankBranchName) && userInfo.BranchId < 1)
                {
                    bankBranchId = new BankServices().AddBankBranchForReg(branch);

                    var bankUser = new BankUserObject
                    {
                        BranchId = bankBranchId,
                        UserId = userInfo.Id
                    };

                    if (bankBranchId < 1)
                    {
                        gVal.Code = -1;
                        gVal.Error = "User Bank Branch information could not be processed. Please try again later.";
                        return Json(gVal, JsonRequestBehavior.AllowGet);
                    }

                    var bankUserStatus = new AspNetUserServices().AddBankUser2(bankUser);
                    if (bankUserStatus < 1)
                    {
                        gVal.Code = -1;
                        gVal.Error = "User was updated but with errors";
                    }
                }
                else
                {
                    bankBranchId = new BankServices().UpdateBankBranchForReg(branch);
                    if (bankBranchId < 1)
                    {
                        gVal.Code = -1;
                        gVal.Error = "User Bank Branch information could not be processed. Please try again later.";
                        return Json(gVal, JsonRequestBehavior.AllowGet);
                    }
                }

                
                var ttxd = UserManager.Users.Count(m => m.Email == model.Email && m.Id != userInfo.UserId);
                if (ttxd > 0)
                {
                    gVal.Code = -1;
                    gVal.Error = "The Email address " + model.Email + " is already taken";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var ttxy = UserManager.Users.Count(m => m.PhoneNumber.Trim().Replace(" ", "") == model.PhoneNumber.Trim().Replace(" ", "") && m.Id != userInfo.UserId);
                if (ttxy > 0)
                {
                    gVal.Code = -1;
                    gVal.Error = "The Phone number " + model.PhoneNumber + " is already taken";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var user = UserManager.FindById(userInfo.UserId);
                if (user == null || string.IsNullOrEmpty(user.Id))
                {
                    gVal.Code = -1;
                    gVal.Error = "User information could not be verified. Please try again later.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                user.UserName = model.Email;
                user.Email = model.Email;
                user.PhoneNumber = model.PhoneNumber;

                var result = await UserManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    var errorStr = result.Errors.Aggregate("", (current, error) => current + (error + " "));
                    gVal.Code = -1;
                    gVal.Error = errorStr;
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                
                userInfo.PhoneNumber = model.PhoneNumber;
                userInfo.FirstName = model.FirstName;
                userInfo.LastName = model.LastName;
                userInfo.Email = model.Email;
                userInfo.IsActive = model.IsActive;

                var tt = new AspNetUserServices().UpdateUserAndPersonInfo(userInfo);
                if (tt < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "User was updated but with errors";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = 5;
                gVal.Error = "User Account information was successfully updated.";
                return Json(gVal, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                gVal.Code = -1;
                gVal.Error = "User information could not be updated. Please try again.";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Banker")]
        public async Task<ActionResult> RegisterBankUser(UserViewModel model)
        {
            var gVal = new GenericValidator();
            try
            {
                var validateResult = ValidateUserInput(model);
                if (validateResult.Code < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = validateResult.Error;
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var importerInfo = GetLoggedOnImporterInfo();
                if (importerInfo.Id < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Your session has timed out. Please refresh the page.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var ttxd = UserManager.FindByEmail(model.Email);
                if (ttxd != null)
                {
                    gVal.Code = -1;
                    gVal.Error = "The Email address " + model.Email + " is already taken";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var ttxy = UserManager.Users.Count(m => m.PhoneNumber.Trim().Replace(" ", "") == model.PhoneNumber.Trim().Replace(" ", ""));
                if (ttxy > 0)
                {
                    gVal.Code = -1;
                    gVal.Error = "The Phone number " + model.PhoneNumber + " is already taken";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var personObj = new UserProfileObject
                {
                    PhoneNumber = model.PhoneNumber,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    IsActive = model.IsActive,
                    CompanyId = importerInfo.Id
                };

                var tt = new AspNetUserServices().AddUserAndPersonInfo(personObj);
                if (tt < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "User was updated but with errors";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                } 

                UserManager.UserLockoutEnabledByDefault = true;
                UserManager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
                UserManager.MaxFailedAccessAttemptsBeforeLockout = 5;

                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,

                    UserInfo = new ApplicationDbContext.UserProfile
                    {
                        PersonId = tt,
                        IsActive = true,
                        IsAdmin = false
                    }
                };

                var rad = new Random();
                var password = rad.Next().ToString();

                var result = await UserManager.CreateAsync(user, password);


                if (result.Succeeded)
                {
                    var role = "";
                    if (model.RoleId == (int)BankRoleEnum.Banker)
                    {
                        role = BankRoleEnum.Banker.ToString();
                    }
                    else
                    {
                        if (model.RoleId == (int)BankRoleEnum.Bank_User)
                        {
                            role = BankRoleEnum.Bank_User.ToString();
                        }
                    }

                    UserManager.AddToRole(user.Id, role);

                    var bankUser = new BankUserObject
                    {
                        UserId = user.UserInfo.Id,
                        BranchId = model.BranchId
                    };

                    var branchInfo = new AspNetUserServices().AddBankUserWithBranch(bankUser);
                    
                    model.IsUser = true;
                    model.Password = password;
                    model.SecurityStamp = user.SecurityStamp;
                    model.Id = user.UserInfo.Id;

                    var status = await SendMail(model);
                    if (!status)
                    {
                        gVal.Password = password;
                        gVal.Email = model.Email;
                        gVal.Code = 7;
                        gVal.Error = "User Registeration was successfully completed, but a notification email could not be sent to the user. Please pass the following information to the user. Password: " + password;
                        return Json(gVal, JsonRequestBehavior.AllowGet);
                    }

                    if (branchInfo < 1)
                    {
                        gVal.Code = -1;
                        gVal.Error = "User was registered but could not be added to the selected Branch.";
                        return Json(gVal, JsonRequestBehavior.AllowGet);
                    }
                    gVal.Code = 5;
                    gVal.Error = "User account was successfully created and an email has been sent to notify the user.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = -1;
                gVal.Error = "Please provide all required inputs and try again.";
                return Json(gVal, JsonRequestBehavior.AllowGet);


            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                gVal.Code = -1;
                gVal.Error = "User could not be created. Please try again.";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Banker")]
        public async Task<ActionResult> UpdateBankUser(UserViewModel model)
        {
            var gVal = new GenericValidator();
            try
            {
                var validateResult = ValidateUserInput(model);
                if (validateResult.Code < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = validateResult.Error;
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                if (Session["_bankUser"] == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var userInfo = Session["_bankUser"] as UserProfileObject;

                if (userInfo == null || userInfo.Id < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var ttxd = UserManager.Users.Count(m => m.Email == model.Email && m.Id != userInfo.UserId);
                if (ttxd > 0)
                {
                    gVal.Code = -1;
                    gVal.Error = "The Email address " + model.Email + " is already taken";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var ttxy = UserManager.Users.Count(m => m.PhoneNumber.Trim().Replace(" ", "") == model.PhoneNumber.Trim().Replace(" ", "") && m.Id != userInfo.UserId);
                if (ttxy > 0)
                {
                    gVal.Code = -1;
                    gVal.Error = "The Phone number " + model.PhoneNumber + " is already taken";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var user = UserManager.FindById(userInfo.UserId);
                if (user == null || string.IsNullOrEmpty(user.Id))
                {
                    gVal.Code = -1;
                    gVal.Error = "User information could not be verified. Please try again later.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }


                user.UserName = model.Email;
                user.Email = model.Email;
                user.PhoneNumber = model.PhoneNumber;
                
                var result = await UserManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    var errorStr = result.Errors.Aggregate("", (current, error) => current + (error + " "));
                    gVal.Code = -1;
                    gVal.Error = errorStr;
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                userInfo.PhoneNumber = model.PhoneNumber;
                userInfo.FirstName = model.FirstName;
                userInfo.LastName = model.LastName;
                userInfo.Email = model.Email;
                userInfo.IsActive = model.IsActive;

                var tt = new AspNetUserServices().UpdateUserAndPersonInfo(userInfo);
                if (tt < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "User was updated but with errors";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var role = "";
                if (userInfo.RoleId != model.RoleId)
                {
                    if (model.RoleId == (int)BankRoleEnum.Banker)
                    {
                        role = BankRoleEnum.Banker.ToString();
                    }
                    else
                    {
                        if (model.RoleId == (int)BankRoleEnum.Bank_User)
                        {
                            role = BankRoleEnum.Bank_User.ToString();
                        }
                    }
                    var previousRole = Enum.GetName(typeof (BankRoleEnum), userInfo.RoleId);
                    UserManager.AddToRole(user.Id, role);
                    UserManager.RemoveFromRole(user.Id, previousRole);
                }

                if (userInfo.BranchId != model.BranchId)
                {

                    var bankUser = new BankUserObject
                    {
                        UserId = userInfo.Id,
                        BranchId = model.BranchId
                    };

                    var branchInfo = new AspNetUserServices().UpdateBankUser(bankUser);
                    if (branchInfo < 1)
                    {
                        gVal.Code = -1;
                        gVal.Error = "User information was updated but with errors.";
                        return Json(gVal, JsonRequestBehavior.AllowGet);
                    }
                }

                gVal.Code = 5;
                gVal.Error = "User Account information was successfully updated.";
                return Json(gVal, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                gVal.Code = -1;
                gVal.Error = "User could not be created. Please try again.";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        private async Task<bool> SendMail(UserViewModel model)
        {
            try
            {
                if (model == null || string.IsNullOrEmpty(model.SecurityStamp))
                {
                    return false;
                }
                
                var type = 0;
                const string label = "Activate your Account!";
                if (model.IsUser)
                {
                    type = (int) MessageEventEnum.New_User;
                }

                else
                {
                    type = (int)MessageEventEnum.New_Account;
                }

                if (type < 1)
                {
                    type = (int) MessageEventEnum.New_Account;
                }
               
                var msgBody = "";
                var msg = new MessageTemplateServices().GetMessageTempWithExpiry(type);
                if (msg.Id < 1)
                {
                    return false;
                }

                var emMs = new MessageObject
                {
                    UserId = model.Id,
                    MessageTemplateId = msg.Id,
                    Status = (int)MessageStatus.Pending,
                    DateSent = DateTime.Now,
                    MessageBody = msg.MessageContent
                };

                var sta = new MessageServices().AddMessage(emMs);
                if (sta < 1)
                {
                    return false;
                }

                if (Request.Url != null)
                {
                    msg.MessageContent = msg.MessageContent.Replace("\n", "<br/>");
                    msg.Subject = msg.Subject.Replace("\n", "<br/>");
                    msgBody = "DPR/DS/IEP/" + DateTime.Today.ToString("ddMMyyyy") + "/" + sta + "<br/><br/>";
                    if (msg.MessageContent.Contains("{hours}") && msg.MessageLifeSpan > 0)
                    {
                        msg.MessageContent = msg.MessageContent.Replace("{hours}", msg.MessageLifeSpan.ToString());
                    }

                    if (!model.IsUser)
                    {
                        msg.Subject  = msg.Subject.Replace("{$company}", model.LastName);
                    }

                    if (model.IsUser)
                    {
                        msg.Subject = msg.Subject.Replace("{firstname lastname}", model.FirstName + " " + model.LastName);
                        msg.MessageContent = msg.MessageContent.Replace("{user}", model.LastName).Replace("{password}", model.Password);
                    }

                    msgBody += "<br/><br/>" + msg.MessageContent.Replace("{email verification link}", "<a style=\"color:green; cursor:pointer\" title=\"Activate Account\" href=" + Url.Action("ConfirmEmail", "Account", new { email = model.Email, code = model.SecurityStamp, ixf = sta }, Request.Url.Scheme) + ">" + label +"</a>").Replace("\n", "<br/>");

                    msgBody += "<br/><br/>" + msg.Footer.Replace("\n", "<br/>");
                }
                
                if (Request.Url != null)
                {

                    
                    var config = WebConfigurationManager.OpenWebConfiguration(HttpContext.Request.ApplicationPath);
                    var settings = (MailSettingsSectionGroup)config.GetSectionGroup("system.net/mailSettings");

                    var apiKey = System.Configuration.ConfigurationManager.AppSettings["mandrillApiKey"];
                    var appName = System.Configuration.ConfigurationManager.AppSettings["AplicationName"];

                    if (settings == null || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(appName))
                    {
                        return false;
                    }

                    #region Using Mandrill
                    var api = new MandrillApi(apiKey);
                    var receipint = new List<MandrillMailAddress> { new MandrillMailAddress(model.Email) };
                    var message = new MandrillMessage()
                    {
                        AutoHtml = true,
                        To = receipint,
                        FromEmail = settings.Smtp.From,
                        FromName = appName,
                        Subject = msg.Subject,
                        Html = msgBody
                    };

                    var result = await api.Messages.SendAsync(message);

                    if (result[0].Status != MandrillSendMessageResponseStatus.Sent)
                    {
                        emMs.Status = (int)MessageStatus.Failed;
                    }
                    else
                    {
                        emMs.Status = (int)MessageStatus.Sent;
                    }
                    #endregion

                    emMs.Id = sta;
                    emMs.MessageBody = msgBody;
                    var tts = new MessageServices().UpdateMessage(emMs);
                    if (tts < 1)
                    {
                        return false;
                    }

                    return true;

                }

                return false;
            }
            catch (Exception e)
            {
                ErrorLogger.LoggError(e.StackTrace, e.Source, e.Message);
                return false;
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> SendSupportRequest(SupportRequestObject model)
        { 
            var gVal = new GenericValidator();
            try
            {
                var importerInfo = GetUserInfo();
                if (importerInfo.Id < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Your session has timed out. Please refresh the page.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                if (string.IsNullOrEmpty(model.Title))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please add a tile to your request/complaint.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                if (string.IsNullOrEmpty(model.MessageBody))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please add a tile to your request/complaint";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                
                if (Request.Url != null)
                {
                    var config = WebConfigurationManager.OpenWebConfiguration(HttpContext.Request.ApplicationPath);
                    var settings = (MailSettingsSectionGroup)config.GetSectionGroup("system.net/mailSettings");

                    var apiKey = System.Configuration.ConfigurationManager.AppSettings["mandrillApiKey"];
                    var appName = System.Configuration.ConfigurationManager.AppSettings["AplicationName"];

                    if (settings == null || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(appName))
                    {
                        gVal.Code = -1;
                        gVal.Error = "Your request could not be processed. Please try again.";
                        return Json(gVal, JsonRequestBehavior.AllowGet);
                    }

                    #region Using Mandrill
                    var api = new MandrillApi(apiKey);
                    var receipint = new List<MandrillMailAddress> { new MandrillMailAddress("ppips@dpr.gov.ng") };
                    var message = new MandrillMessage()
                    {
                        AutoHtml = true,
                        To = receipint,
                        FromEmail = importerInfo.UserProfileObject.Email,
                        FromName = appName,
                        Subject = model.Title,
                        Html = model.MessageBody
                    };

                    var result = await api.Messages.SendAsync(message);

                    if (result[0].Status != MandrillSendMessageResponseStatus.Sent)
                    {
                        gVal.Code = -1;
                        gVal.Error = "Your message could not bee sent. Please try again.";
                    }
                    else
                    {
                        gVal.Code = 5;
                        gVal.Error = "Your message has been sent. Be rest assured it will be handled as soon as possible.";
                    }

                    #endregion
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = -1;
                gVal.Error = "Internal server error. Your request could not be processed. Please try again.";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                ErrorLogger.LoggError(e.StackTrace, e.Source, e.Message);
                gVal.Code = -1;
                gVal.Error = "Internal server error. Your request could not be processed. Please try again.";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        private async Task<bool> SendPasswordChangeMail(UserViewModel model)
        {
            try
            {
                if (model == null)
                {
                    return false;
                }

                var msgBody = "";

                if (Request.Url != null)
                {
                    var str =  "<br/>Your account password on Petroleum Products Import Permit System has been Changed. Your new Account details are as follows: <br/>";
                    str += "<br/>User Name: " + model.Email + "<br/>Password: " + model.Password;
                    msgBody = str;
                }
               

                var config = WebConfigurationManager.OpenWebConfiguration(HttpContext.Request.ApplicationPath);
                var settings = (MailSettingsSectionGroup)config.GetSectionGroup("system.net/mailSettings");

                var apiKey = System.Configuration.ConfigurationManager.AppSettings["mandrillApiKey"];
                var appName = System.Configuration.ConfigurationManager.AppSettings["AplicationName"];

                if (settings == null || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(appName))
                {
                    return false;
                }

                #region Using Mandrill
                var api = new MandrillApi(apiKey);
                var receipint = new List<MandrillMailAddress> { new MandrillMailAddress(model.Email) };
                var message = new MandrillMessage()
                {
                    AutoHtml = true,
                    To = receipint,
                    FromEmail = settings.Smtp.From,
                    FromName = appName,
                    Subject = "Your Account Information has changed",
                    Html = msgBody
                };

                var result = await api.Messages.SendAsync(message);

                if (result[0].Status != MandrillSendMessageResponseStatus.Sent)
                {
                   return false;
                }
              
                return true;
                #endregion
            }
            catch (Exception e)
            {
                ErrorLogger.LoggError(e.StackTrace, e.Source, e.Message);
                return false;
            }
        }
        
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string email, string code, long ixf)
        {
            ViewBag.Title = "Confirm Email|DPR-PPIPS";
            try
            {
                if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(email) || ixf < 1)
                {
                    ViewBag.ConfirmStat = -1;
                    ViewBag.RegisterationSuccessful = "Your user account could not be verified. <br/>Please contact the Administrator.";
                    return View("Login", new LoginViewModel());
                }

                const int type = (int)MessageEventEnum.Account_Confirmation;
                var tokenVerificationResult = new AspNetUserServices().ActivateAccount(email, code, type, ixf);
                
                if (tokenVerificationResult.ErrorCode > 0)
                {
                    ViewBag.ConfirmStat = -1;
                    ViewBag.RegisterationSuccessful = "The activation link has expired. <br/>Please request for a fresh activation link.";
                    return View("Login", new LoginViewModel());
                }
                
                if (tokenVerificationResult.UserId < 1)
                {
                    ViewBag.ConfirmStat = -1;
                    ViewBag.RegisterationSuccessful = "Your user account could not be verified. <br/>Please try again or request for a fresh confirmation email.";
                    return View("Login", new LoginViewModel());
                }

                var msgBody = "";

                var msg = tokenVerificationResult;

                var emMs = new MessageObject
                {
                    UserId = msg.UserId,
                    MessageTemplateId = msg.Id,
                    Status = (int)MessageStatus.Pending,
                    DateSent = DateTime.Now,
                    MessageBody = msgBody
                };

                var sta = new MessageServices().AddMessage(emMs);
                if (sta < 1)
                {
                    ViewBag.ConfirmStat = -1;
                    ViewBag.RegisterationSuccessful = "Your user account could not be verified. <br/>Please try again or request for a fresh confirmation email.";
                    return View("Login", new LoginViewModel());
                }

                if (Request.Url != null)
                {
                    msg.MessageContent = msg.MessageContent.Replace("\n", "<br/>");
                    msg.Subject = msg.Subject.Replace("\n", "<br/>");
                    msgBody = "DPR/DS/IEP/" + DateTime.Today.ToString("ddMMyyyy") + "/" + sta + "<br/>";


                    if (msg.IsImporter)
                    {
                        msgBody += "<br/>" + msg.MessageContent.Replace("{company}", msg.UserName);
                    }
                    else
                    {
                        msgBody += "Dear " + msg.UserName + ",<br/>" + "Welcome to the Petroleum Products Import Permit System (PPIPS). You have successfully confirmed your account.";
                        
                    }
                    msgBody += "<br/><br/>" + msg.Footer.Replace("\n", "<br/>");
                }

                var config = WebConfigurationManager.OpenWebConfiguration(HttpContext.Request.ApplicationPath);
                var settings = (MailSettingsSectionGroup)config.GetSectionGroup("system.net/mailSettings");
                var apiKey = System.Configuration.ConfigurationManager.AppSettings["mandrillApiKey"];
                var appName = System.Configuration.ConfigurationManager.AppSettings["AplicationName"];

                if (settings == null || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(appName))
                {
                    ViewBag.ConfirmStat = -1;
                    ViewBag.RegisterationSuccessful = "Your request could not be processed. Please try again later.";
                    return View("Login", new LoginViewModel());
                }

                #region Using Mandrill
                var api = new MandrillApi(apiKey);
                var receipint = new List<MandrillMailAddress> { new MandrillMailAddress(email) };
                var message = new MandrillMessage()
                {
                    AutoHtml = true,
                    To = receipint,
                    FromEmail = settings.Smtp.From,
                    FromName = appName,
                    Subject = msg.Subject,
                    Html = msgBody
                };

                var result = await api.Messages.SendAsync(message);

                if (result[0].Status != MandrillSendMessageResponseStatus.Sent)
                {
                    emMs.Status = (int)MessageStatus.Failed;
                }
                else
                {
                    emMs.Status = (int)MessageStatus.Sent;
                }
                #endregion

              
                emMs.Id = sta;
                emMs.MessageBody = msgBody;
                var tts = new MessageServices().UpdateMessage(emMs);
                if (tts < 1)
                {
                    ViewBag.ConfirmStat = -1;
                    ViewBag.RegisterationSuccessful = "Your user account could not be verified. <br/>Please contact the Administrator.";
                    return View("Login", new LoginViewModel());
                }
                
                ServiceHelper.SendSmsNotification(tokenVerificationResult.PhoneNumber, msg.Subject);
                ViewBag.ConfirmStat = 5;
                ViewBag.RegisterationSuccessful = "Your account has been successfully activated. <br/>A welcome message bearing some important instructions has been sent to your mail. You can Login now.";
                return View("Login", new LoginViewModel());
            }
            catch (Exception)
            {
                ViewBag.ConfirmStat = -1;
                ViewBag.RegisterationSuccessful = "Your user account could not be verified. <br/>Please try again or request for a fresh confirmation email.";
                return View("Login", new LoginViewModel());
            }

        }
        
        [HttpPost]
        public async Task<ActionResult> RequestActivationLink(string rEmail)
        {
            var gVal = new GenericValidator();
            try
            {
                var user = UserManager.FindByEmail(rEmail);
                if (user == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "This Email does not exist on our data bank.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                int code;
                long id;
                var newStamp = new AspNetUserServices().GetSecurityStamp(rEmail, out code, out id);
                if (string.IsNullOrEmpty(newStamp) || code < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Request could not be processed. Please try again.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                const int type = (int)MessageEventEnum.Activation_Link_Request;
                var msg = new MessageTemplateServices().GetMessageTemp(type);
                if (msg.Id < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Your request could not be processed. Please try again.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var emMs = new MessageObject
                {
                    UserId = id,
                    MessageTemplateId = msg.Id,
                    Status = (int)MessageStatus.Pending,
                    DateSent = DateTime.Now,
                    MessageBody = msg.MessageContent
                };

                var sta = new MessageServices().AddMessage(emMs);
                if (sta < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Your request could not be processed. Please try again.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                
                if (Request.Url != null)
                {
                    var str = "DPR/DS/IEP/" + DateTime.Today.ToString("ddMMyyyy") + "/" + sta + "<br/>";
                    msg.Subject = msg.Subject.Replace("\n", "<br/>");
                    msg.MessageContent = msg.MessageContent.Replace("{hours}", msg.MessageLifeSpan.ToString());
                    str += "<br/><br/>" +
                           msg.MessageContent.Replace("{email verification link}",
                               "<a style=\"color:green; cursor:pointer\" title=\"Activate Account\" href=" + Url.Action("ConfirmEmail", "Account", new { email = rEmail, code = newStamp, ixf = sta }, Request.Url.Scheme) + ">Confirm Activation Request</a>").Replace("\n", "<br/>");
                    

                    str += "<br/><br/>" + msg.Footer.Replace("\n", "<br/>");

                    var config = WebConfigurationManager.OpenWebConfiguration(HttpContext.Request.ApplicationPath);
                    var settings = (MailSettingsSectionGroup)config.GetSectionGroup("system.net/mailSettings");

                    var apiKey = System.Configuration.ConfigurationManager.AppSettings["mandrillApiKey"];
                    var appName = System.Configuration.ConfigurationManager.AppSettings["AplicationName"];

                    if (settings == null || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(appName))
                    {
                        gVal.Code = -1;
                        gVal.Error = "Your request could not be processed. Please try again.";
                        return Json(gVal, JsonRequestBehavior.AllowGet);
                    }

                    #region Using Mandrill
                    var api = new MandrillApi(apiKey);
                    var receipint = new List<MandrillMailAddress> { new MandrillMailAddress(rEmail) };
                    var message = new MandrillMessage()
                    {
                        AutoHtml = true,
                        To = receipint,
                        FromEmail = settings.Smtp.From,
                        FromName = appName,
                        Subject = msg.Subject,
                        Html = str
                    };

                    var result = await api.Messages.SendAsync(message);

                    if (result[0].Status != MandrillSendMessageResponseStatus.Sent)
                    {
                        emMs.Status = (int)MessageStatus.Failed;
                    }
                    else
                    {
                        emMs.Status = (int)MessageStatus.Sent;
                    }
                    #endregion

                    emMs.Id = sta;
                    emMs.MessageBody = str;
                    var tts = new MessageServices().UpdateMessage(emMs);
                    if (tts < 1)
                    {
                        gVal.Code = -1;
                        gVal.Error = "Your request could not be processed. <br/>Please contact the Administrator.";
                        return Json(gVal, JsonRequestBehavior.AllowGet);
                    }
                    gVal.Code = 5;
                    gVal.Error = "An activation link has been sent to your email.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                   
                }
                gVal.Code = -1;
                gVal.Error = "Your request could not be processed. Please try again.";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }

            catch (Exception)
            {
                gVal.Code = -1;
                gVal.Error = "Your request could not be processed. Please try again.";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public async Task<ActionResult> RequestNewPassword(string rEmail)
        {
            var gVal = new GenericValidator();
            try
            {
                var user = UserManager.FindByEmail(rEmail);
                if (user == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "This Email does not exist on our data bank.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                
                const int type = (int)MessageEventEnum.Password_Reset;
                var msg = new MessageTemplateServices().GetMessageTemplate(type, rEmail);
                if (msg.Id < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Your request could not be processed. Please try again.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                

                if (Request.Url != null)
                {
                    var str = "DPR/DS/IEP/" + DateTime.Today.ToString("ddMMyyyy") + "/" + msg.Id + "<br/><br/>";
                    msg.Subject = msg.Subject.Replace("\n", "<br/>");
                    str += "<br/><br/>" + msg.MessageContent.Replace("{Link to reset password}", "<a style=\"color:green; cursor:pointer\" title=\"Activate Account\" href=" + Url.Action("ConfirmPasswordReset", "Account", new { email = rEmail }, Request.Url.Scheme) + ">Confirm Password request</a>").Replace("\n", "<br/>");
                    
                    str += "<br/><br/>" + msg.Footer.Replace("\n", "<br/>");

                    var emMs = new MessageObject
                    {
                        UserId = msg.UserId,
                        MessageTemplateId = msg.Id,
                        Status = (int)MessageStatus.Pending,
                        DateSent = DateTime.Now,
                        MessageBody = str
                    };
                    var sta = new MessageServices().AddMessage(emMs);
                    if (sta < 1)
                    {
                        gVal.Code = -1;
                        gVal.Error = "Your request could not be processed. Please try again.";
                        return Json(gVal, JsonRequestBehavior.AllowGet);
                    }

                    var config = WebConfigurationManager.OpenWebConfiguration(HttpContext.Request.ApplicationPath);
                    var settings = (MailSettingsSectionGroup)config.GetSectionGroup("system.net/mailSettings");
                    var apiKey = System.Configuration.ConfigurationManager.AppSettings["mandrillApiKey"];
                    var appName = System.Configuration.ConfigurationManager.AppSettings["AplicationName"];

                    if (settings == null || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(appName))
                    {
                        gVal.Code = -1;
                        gVal.Error = "Your request could not be processed. Please try again.";
                        return Json(gVal, JsonRequestBehavior.AllowGet);
                    }
                   
                    #region Using Mandrill
                    var api = new MandrillApi(apiKey);
                    var receipint = new List<MandrillMailAddress> {new MandrillMailAddress(rEmail)};
                    var message = new MandrillMessage()
                    {
                        AutoHtml = true,
                        To = receipint,
                        FromEmail = settings.Smtp.From,
                        FromName = appName,
                        Subject = msg.Subject,
                        Html = str
                    };
                    
                    var result = await api.Messages.SendAsync(message);

                    if (result[0].Status != MandrillSendMessageResponseStatus.Sent)
                    {
                        emMs.Status = (int)MessageStatus.Failed;
                    }
                    else
                    {
                        emMs.Status = (int)MessageStatus.Sent;
                    }
                    #endregion
                   
                    emMs.Id = sta;
                    emMs.MessageBody = str;
                    var tts = new MessageServices().UpdateMessage(emMs);
                    if (tts < 1)
                    {
                        gVal.Code = -1;
                        gVal.Error = "Your request could not be processed. Please try again.";
                        return Json(gVal, JsonRequestBehavior.AllowGet);
                    }



                    gVal.Code = 5;
                    gVal.Error = "An password reset link has been sent to your email.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                   
                }
                gVal.Code = -1;
                gVal.Error = "Your request could not be processed. Please try again.";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }

            catch (Exception)
            {
                gVal.Code = -1;
                gVal.Error = "Your request could not be processed. Please try again.";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }
        
        [AllowAnonymous]
        public ActionResult ConfirmPasswordReset(string email)
        {
            ViewBag.Title = "Confirm Password Reset Request|DPR-PPIPS";
            try
            {
                if (string.IsNullOrEmpty(email))
                {
                    ViewBag.ConfirmStat = -1;
                    ViewBag.RegisterationSuccessful = "Your user account could not be verified. <br/>Please contact the Administrator.";
                    return View("Login", new LoginViewModel());
                }
                
                var user = UserManager.FindByEmail(email);
                if (user == null || string.IsNullOrEmpty(user.Email))
                {
                    ViewBag.ConfirmStat = -1;
                    ViewBag.RegisterationSuccessful = "This Email does not exist on our system.";
                    return View("Login", new LoginViewModel());
                }
                ViewBag.Email = email;
                return View();
            }
            catch (Exception)
            {
                ViewBag.ConfirmStat = -1;
                ViewBag.RegisterationSuccessful = "Your user account could not be verified. <br/>Please try again or request for a fresh confirmation email.";
                return View("Login", new LoginViewModel());
            }

        }

        [AllowAnonymous]
        public async Task<ActionResult> PasswordReset(UserViewModel model)
        {
            ViewBag.Email = model.Email;
            try
            {
                if (string.IsNullOrEmpty(model.Email))
                {
                    ViewBag.ConfirmStat = -1;
                    ViewBag.RegisterationSuccessful = "Please provide your Email.";
                    return View("ConfirmPasswordReset");
                }

                if (string.IsNullOrEmpty(model.Password) || string.IsNullOrEmpty(model.ConfirmPassword))
                {
                    ViewBag.ConfirmStat = -1;
                    ViewBag.RegisterationSuccessful = "Please provide Password/Confirm Password..";
                    return View("ConfirmPasswordReset");
                }

                if (model.Password !=model.ConfirmPassword)
                {
                    ViewBag.ConfirmStat = -1;
                    ViewBag.RegisterationSuccessful = "The Passwords do not match.";
                    return View("ConfirmPasswordReset");
                }

                var hash = new PasswordHasher().HashPassword(model.Password);
                if (string.IsNullOrEmpty(hash))
                {
                    ViewBag.ConfirmStat = -1;
                    ViewBag.RegisterationSuccessful = "Your password could not be reset. <br/>Please try again or contact an Administrator.";
                    return View("ConfirmPasswordReset");
                }

                const int type = (int)MessageEventEnum.Activation_Link_Request;

                var msg = new AspNetUserServices().UpdatePassword(model.Email, hash, type);
                if (msg.Id < 1)
                {
                    ViewBag.ConfirmStat = -1;
                    ViewBag.RegisterationSuccessful = "Your password could not be reset. <br/>Please try again or contact an Administrator.";
                    return View("ConfirmPasswordReset");
                }
                
                var emMs = new MessageObject
                {
                    UserId = msg.UserId,
                    MessageTemplateId = msg.Id,
                    Status = (int)MessageStatus.Pending,
                    DateSent = DateTime.Now,
                    MessageBody = msg.MessageContent
                };

                var sta = new MessageServices().AddMessage(emMs);
                if (sta < 1)
                {
                    ViewBag.ConfirmStat = -1;
                    ViewBag.RegisterationSuccessful = "Process failed.";
                    return View("ConfirmPasswordReset");
                }

                var msgBody = "";

                if (Request.Url != null)
                {
                    msgBody = "DPR/DS/IEP/" + DateTime.Today.ToString("ddMMyyyy") + "/" + sta + "<br/><br/>";
                    msg.Subject = msg.Subject.Replace("\n", "<br/>");
                    msgBody += msg.Subject;

                    msgBody += "<br/>" + msg.MessageContent.Replace("\n", "<br/>");

                    msgBody += "<br/><br/>" + msg.Footer.Replace("\n", "<br/>");
                }

                var config = WebConfigurationManager.OpenWebConfiguration(HttpContext.Request.ApplicationPath);
                var settings = (MailSettingsSectionGroup)config.GetSectionGroup("system.net/mailSettings");
                var apiKey = System.Configuration.ConfigurationManager.AppSettings["mandrillApiKey"];
                var appName = System.Configuration.ConfigurationManager.AppSettings["AplicationName"];

                if (settings == null || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(appName))
                {
                    ViewBag.ConfirmStat = -1;
                    ViewBag.RegisterationSuccessful = "Your password could not be reset. <br/>Please try again or contact an Administrator.";
                    return View("ConfirmPasswordReset");
                }

                #region Using Mandrill
                var api = new MandrillApi(apiKey);
                var receipint = new List<MandrillMailAddress> { new MandrillMailAddress(model.Email) };
                var message = new MandrillMessage()
                {
                    AutoHtml = true,
                    To = receipint,
                    FromEmail = settings.Smtp.From,
                    FromName = appName,
                    Subject = msg.Subject,
                    Html = msgBody
                };

                var result = await api.Messages.SendAsync(message);

                if (result[0].Status != MandrillSendMessageResponseStatus.Sent)
                {
                    emMs.Status = (int)MessageStatus.Failed;
                }
                else
                {
                    emMs.Status = (int)MessageStatus.Sent;
                }
                #endregion
                

                emMs.Id = sta;
                emMs.MessageBody = msgBody;
                var tts = new MessageServices().UpdateMessage(emMs);
                if (tts < 1)
                {
                    ViewBag.ConfirmStat = -1;
                    ViewBag.RegisterationSuccessful = "Your password was successfully reset but some proceses could not complete successfully. <br/>Please try again or contact an Administrator.";
                    return View("ConfirmPasswordReset");
                }
               
                ViewBag.ConfirmStat = 5;
                ViewBag.RegisterationSuccessful = "You have successfully reset your account password. You can Login now.";
                return View("Login", new LoginViewModel());
            }
            catch (Exception)
            {
                ViewBag.ConfirmStat = -1;
                ViewBag.RegisterationSuccessful = "Process failed. Please try again.";
                return View("ConfirmPasswordReset");
            }

        }
        
        [HttpPost]
        [Authorize]
        public async Task<ActionResult> ChangePassword(ManageUserViewModel model)
        {
            var gVal = new GenericValidator();
            try
            {
                var importerInfo = GetLoggedOnImporterInfo();
                if (importerInfo.Id < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Your session has timed out. Please refresh the page.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                if (string.IsNullOrEmpty(model.NewPassword) || string.IsNullOrEmpty(model.ConfirmPassword))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide Password/Confirm Password.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                if (model.NewPassword != model.ConfirmPassword)
                {
                    gVal.Code = -1;
                    gVal.Error = "The Passwords do not match.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var hash = new PasswordHasher().HashPassword(model.NewPassword);
                if (string.IsNullOrEmpty(hash))
                {
                    gVal.Code = -1;
                    gVal.Error = "Your password could not be reset. <br/>Please try again or contact an Administrator.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                const int type = (int)MessageEventEnum.Password_Reset_Successful;
                var email = "";
                var msg = new AspNetUserServices().UpdatePassword(importerInfo.UserProfileObject.Id, hash, type, out email);
                if (msg.Id < 1 || string.IsNullOrEmpty(email))
                {
                    gVal.Code = -1;
                    gVal.Error = "Your password could not be reset. Please try again or contact an Administrator.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

               var emMs = new MessageObject
                {
                    UserId = msg.UserId,
                    MessageTemplateId = msg.Id,
                    Status = (int)MessageStatus.Pending,
                    DateSent = DateTime.Now,
                    MessageBody = msg.MessageContent
                };

                var sta = new MessageServices().AddMessage(emMs);
                if (sta < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Process failed.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var msgBody = "";

                if (Request.Url != null)
                {
                    msgBody = "DPR/DS/IEP/" + DateTime.Today.ToString("ddMMyyyy") + "/" + sta + "<br/><br/>";
                    msg.Subject = msg.Subject.Replace("\n", "<br/>");
                    msgBody += msg.Subject;

                    msgBody += "<br/>" + msg.MessageContent.Replace("\n", "<br/>");

                    msgBody += "<br/><br/>" + msg.Footer.Replace("\n", "<br/>");
                }

                var config = WebConfigurationManager.OpenWebConfiguration(HttpContext.Request.ApplicationPath);
                var settings = (MailSettingsSectionGroup)config.GetSectionGroup("system.net/mailSettings");

                var apiKey = System.Configuration.ConfigurationManager.AppSettings["mandrillApiKey"];
                var appName = System.Configuration.ConfigurationManager.AppSettings["AplicationName"];

                if (settings == null || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(appName))
                {
                    ViewBag.ConfirmStat = -1;
                    ViewBag.RegisterationSuccessful = "Your password could not be reset. <br/>Please try again or contact an Administrator.";
                    return View("ConfirmPasswordReset");
                }

                #region Using Mandrill
                var api = new MandrillApi(apiKey);
                var receipint = new List<MandrillMailAddress> { new MandrillMailAddress(email) };
                var message = new MandrillMessage()
                {
                    AutoHtml = true,
                    To = receipint,
                    FromEmail = settings.Smtp.From,
                    FromName = appName,
                    Subject = msg.Subject,
                    Html = msgBody
                };

                var result = await api.Messages.SendAsync(message);

                if (result[0].Status != MandrillSendMessageResponseStatus.Sent)
                {
                    emMs.Status = (int)MessageStatus.Failed;
                }
                else
                {
                    emMs.Status = (int)MessageStatus.Sent;
                }
                #endregion

                emMs.Id = sta;
                emMs.MessageBody = msgBody;
                var tts = new MessageServices().UpdateMessage(emMs);
                if (tts < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Your password was successfully reset but some errors.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
              

                gVal.Code = 5;
                gVal.Error = "You have successfully reset your account password. You can Login now.";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                gVal.Code = -1;
                gVal.Error = "Process failed. Please try again.";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost]
        [Authorize]
        public async  Task<ActionResult> ChangePasswordFromProfile(ManageUserViewModel model)
        {
            var gVal = new GenericValidator();
            try
            {
                if (model.UserId < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Error: Please refresh the page and try again.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                if (string.IsNullOrEmpty(model.NewPassword) || string.IsNullOrEmpty(model.ConfirmPassword))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide Password/Confirm Password.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                if (model.NewPassword != model.ConfirmPassword)
                {
                    gVal.Code = -1;
                    gVal.Error = "The Passwords do not match.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var hash = new PasswordHasher().HashPassword(model.NewPassword);
                if (string.IsNullOrEmpty(hash))
                {
                    gVal.Code = -1;
                    gVal.Error = "User's password could not be reset. <br/>Please try again or contact an Administrator.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                const int type = (int)MessageEventEnum.Password_Reset_Successful;
                var email = "";
                var msg = new AspNetUserServices().UpdatePassword(model.UserId, hash, type, out email);
                if (msg.Id < 1 || string.IsNullOrEmpty(email))
                {
                    gVal.Code = -1;
                    gVal.Error = "User's password could not be reset. Please try again or contact an Administrator.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var emMs = new MessageObject
                {
                    UserId = msg.UserId,
                    MessageTemplateId = msg.Id,
                    Status = (int)MessageStatus.Pending,
                    DateSent = DateTime.Now,
                    MessageBody = msg.MessageContent
                };

                var sta = new MessageServices().AddMessage(emMs);
                if (sta < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Process failed.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var msgBody = "";

                if (Request.Url != null)
                {
                    msgBody = "DPR/DS/IEP/" + DateTime.Today.ToString("ddMMyyyy") + "/" + sta + "<br/><br/>";
                    msg.Subject = msg.Subject.Replace("\n", "<br/>");
                    msgBody += msg.Subject;

                    msgBody += "<br/>" + msg.MessageContent.Replace("\n", "<br/>");

                    msgBody += "<br/><br/>" + msg.Footer.Replace("\n", "<br/>");
                }

                var config = WebConfigurationManager.OpenWebConfiguration(HttpContext.Request.ApplicationPath);
                var settings = (MailSettingsSectionGroup)config.GetSectionGroup("system.net/mailSettings");

                var apiKey = System.Configuration.ConfigurationManager.AppSettings["mandrillApiKey"];
                var appName = System.Configuration.ConfigurationManager.AppSettings["AplicationName"];

                if (settings == null || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(appName))
                {
                    gVal.Code = -1;
                    gVal.Error = "Your request could not be processed. Please try again.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                #region Using Mandrill
                var api = new MandrillApi(apiKey);
                var receipint = new List<MandrillMailAddress> { new MandrillMailAddress(email) };
                var message = new MandrillMessage()
                {
                    AutoHtml = true,
                    To = receipint,
                    FromEmail = settings.Smtp.From,
                    FromName = appName,
                    Subject = msg.Subject,
                    Html = msgBody
                };

                var result = await api.Messages.SendAsync(message);

                if (result[0].Status != MandrillSendMessageResponseStatus.Sent)
                {
                    emMs.Status = (int)MessageStatus.Failed;
                }
                else
                {
                    emMs.Status = (int)MessageStatus.Sent;
                }
                #endregion

                emMs.Id = sta;
                emMs.MessageBody = msgBody;
                var tts = new MessageServices().UpdateMessage(emMs);
                if (tts < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "User's password was successfully reset but with errors.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
               

                gVal.Code = 5;
                gVal.Error = "User's password was successfully updated.";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                gVal.Code = -1;
                gVal.Error = "Process failed. Please try again.";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> ChangePasswordByAdmin(ManageUserViewModel model)
        {
            var gVal = new GenericValidator(); 
            try
            {
                if (model.UserId < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Error: Please refresh the page and try again.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                if (string.IsNullOrEmpty(model.NewPassword) || string.IsNullOrEmpty(model.ConfirmPassword))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide Password/Confirm Password.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                if (model.NewPassword != model.ConfirmPassword)
                {
                    gVal.Code = -1;
                    gVal.Error = "The Passwords do not match.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var hash = new PasswordHasher().HashPassword(model.NewPassword);
                if (string.IsNullOrEmpty(hash))
                {
                    gVal.Code = -1;
                    gVal.Error = "Your password could not be reset. <br/>Please try again or contact an Administrator.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                const int type = (int)MessageEventEnum.Password_Reset_Successful;
                var email = "";
                var msg = new AspNetUserServices().UpdatePasswordByAdmin(model.UserId, hash, type, out email);
                if (msg.Id < 1 || string.IsNullOrEmpty(email))
                {
                    gVal.Code = -1;
                    gVal.Error = "User's password could not be reset. Please try again or contact an Administrator.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var emMs = new MessageObject
                {
                    UserId = msg.UserId,
                    MessageTemplateId = msg.Id,
                    Status = (int)MessageStatus.Pending,
                    DateSent = DateTime.Now,
                    MessageBody = msg.MessageContent
                };

                var sta = new MessageServices().AddMessage(emMs);
                if (sta < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Process failed.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                var msgBody = "";

                if (Request.Url != null)
                {
                    msgBody = "DPR/DS/IEP/" + DateTime.Today.ToString("ddMMyyyy") + "/" + sta + "<br/><br/>";
                    msg.Subject = msg.Subject.Replace("\n", "<br/>");
                    msgBody += msg.Subject;

                    msgBody += "<br/>" + msg.MessageContent.Replace("\n", "<br/>");

                    msgBody += "<br/><br/>" + msg.Footer.Replace("\n", "<br/>");
                }

                var config = WebConfigurationManager.OpenWebConfiguration(HttpContext.Request.ApplicationPath);
                var settings = (MailSettingsSectionGroup)config.GetSectionGroup("system.net/mailSettings");

                var apiKey = System.Configuration.ConfigurationManager.AppSettings["mandrillApiKey"];
                var appName = System.Configuration.ConfigurationManager.AppSettings["AplicationName"];

                if (settings == null || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(appName))
                {
                    gVal.Code = -1;
                    gVal.Error = "Your request could not be processed. Please try again.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                #region Using Mandrill
                var api = new MandrillApi(apiKey);
                var receipint = new List<MandrillMailAddress> { new MandrillMailAddress(email) };
                var message = new MandrillMessage()
                {
                    AutoHtml = true,
                    To = receipint,
                    FromEmail = settings.Smtp.From,
                    FromName = appName,
                    Subject = msg.Subject,
                    Html = msgBody
                };

                var result = await api.Messages.SendAsync(message);

                if (result[0].Status != MandrillSendMessageResponseStatus.Sent)
                {
                    emMs.Status = (int)MessageStatus.Failed;
                }
                else
                {
                    emMs.Status = (int)MessageStatus.Sent;
                }
                #endregion

                emMs.Id = sta;
                emMs.MessageBody = msgBody;
                var tts = new MessageServices().UpdateMessage(emMs);
                if (tts < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "User password was successfully reset but some errors.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
               

                gVal.Code = 5;
                gVal.Error = "User's password was successfully reset.";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                gVal.Code = -1;
                gVal.Error = "Process failed. Please try again.";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost]
        public async Task<ActionResult> SignOn(RegisterViewModel model)
        {
            try
            {
                var validateResult = ValidateInput(model);
                if (validateResult.Code < 1)
                {
                    ModelState.AddModelError("", validateResult.Error);
                    return View(model);
                }

                var company = new ImporterObject
                {
                    Name = model.ImporterName,
                    TIN = model.TIN,
                    RCNumber = model.RCNumber
                };

                var ttxd = UserManager.FindByEmail(model.Email);
                if (ttxd != null)
                {
                    ViewBag.RegCode = -1;
                    ViewBag.RegError = "The Email address " + model.Email + " is already taken";
                    return View(model);
                }

                var ttxy = UserManager.Users.Count(m => m.PhoneNumber.Trim().Replace(" ", "") == model.PhoneNumber.Trim().Replace(" ", ""));
                if (ttxy > 0)
                {
                    ViewBag.RegCode = -1;
                    ViewBag.RegError = "The Phone number " + model.PhoneNumber + " is already taken";
                    return View(model);
                }

                UserManager.UserLockoutEnabledByDefault = true;
                UserManager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
                UserManager.MaxFailedAccessAttemptsBeforeLockout = 5;

                var processedImporterStatus = new ImporterServices().AddImporter(company);
                if (processedImporterStatus < 1)
                {
                    ViewBag.RegCode = -1;
                    ViewBag.RegError = "Registration could not be processed. Please try again later.";
                    return View(model);
                }
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,

                    UserInfo = new ApplicationDbContext.UserProfile
                    {
                        Id = processedImporterStatus,
                        IsActive = true
                    }
                };

                var result = await UserManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    ViewBag.RegisterationSuccessful = "Registeration was successfully completed. Please login with your credentials";
                    return View("Login", new LoginViewModel());
                }

                AddErrors(result);
                return View(model);


            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                ModelState.AddModelError("", "User could not be created. Please try again.");
                return View(model);
            }
        }

        private GenericValidator ValidateInput(RegisterViewModel model)
        {
            var gVal = new GenericValidator();
            try
            {

                if (string.IsNullOrEmpty(model.RcCh))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please solve the Recaptcha Challenge before proceeding.";
                    return gVal;
                }

                if (string.IsNullOrEmpty(model.ImporterName))
                {
                    gVal.Code = -1;
                    gVal.Error = model.ImportEligibilityId == (int)AccountTypeEnum.Financial_Institution ? "Please provide Bank name" : "Please provide Importer name";
                    return gVal;
                }

                if (string.IsNullOrEmpty(model.Email))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide Email";
                    return gVal;
                }

                if (string.IsNullOrEmpty(model.PhoneNumber))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide Phone Number";
                    return gVal;
                }

                if (model.StructureId < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Please select Business Structure";
                    return gVal;
                }

                gVal.Code = 5;
                return gVal;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                gVal.Code = -1;
                gVal.Error = "Validation failed. Please try again.";
                return gVal;
            }
        }

        private GenericValidator ValidateInput(UserProfileObject model)
        {
            var gVal = new GenericValidator();
            try
            {
                if (string.IsNullOrEmpty(model.CompanyName))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide Depot name";
                    return gVal;
                }

                if (string.IsNullOrEmpty(model.Email))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide Email";
                    return gVal;
                }

                if (string.IsNullOrEmpty(model.PhoneNumber))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide Phone Number";
                    return gVal;
                }

                if (string.IsNullOrEmpty(model.DepotLicense))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide Depot License";
                    return gVal;
                }


                if (model.IssueDate == null )
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide License Issue Date";
                    return gVal;
                }

                if (model.ExpiryDate == null )
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide License Expiry Date";
                    return gVal;
                }

                if (model.IssueDate != null && model.IssueDate.Value.Year < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide valid License Issue Date";
                    return gVal;
                }
                if (model.ExpiryDate != null && model.ExpiryDate.Value.Year < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide valid License Expiry Date";
                    return gVal;
                }

                if (model.IssueDate.Value > model.ExpiryDate.Value)
                {
                    gVal.Code = -1;
                    gVal.Error = "Depot License Issue Date must not be later than the Expiry Date";
                    return gVal;
                }
                
                gVal.Code = 5;
                return gVal;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                gVal.Code = -1;
                gVal.Error = "Validation failed. Please try again.";
                return gVal;
            }
        }
        private GenericValidator ValidateInputUpdate(UserProfileObject model)
        {
            var gVal = new GenericValidator();
            try
            {

                if (string.IsNullOrEmpty(model.CompanyName))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide Depot name";
                    return gVal;
                }

                if (string.IsNullOrEmpty(model.Email))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide Email";
                    return gVal;
                }

                if (string.IsNullOrEmpty(model.PhoneNumber))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide Phone Number";
                    return gVal;
                }

                gVal.Code = 5;
                return gVal;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                gVal.Code = -1;
                gVal.Error = "Validation failed. Please try again.";
                return gVal;
            }
        }
        private GenericValidator ValidateInput(UserViewModel model)
        {
            var gVal = new GenericValidator();
            try
            {
                if (string.IsNullOrEmpty(model.Password))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide Password";
                    return gVal;
                }

                if (string.IsNullOrEmpty(model.Email))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide Email";
                    return gVal;
                }

                if (string.IsNullOrEmpty(model.PhoneNumber))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide Phone Number";
                    return gVal;
                }

                gVal.Code = 5;
                return gVal;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                gVal.Code = -1;
                gVal.Error = "Validation failed. Please try again.";
                return gVal;
            }
        }

        private GenericValidator ValidateUserInput(UserViewModel model)
        {
            var gVal = new GenericValidator(); 
            try
            {
                if (string.IsNullOrEmpty(model.Email))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide Email";
                    return gVal;
                }

                if (string.IsNullOrEmpty(model.FirstName))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide Other Names";
                    return gVal;
                }

                if (string.IsNullOrEmpty(model.LastName))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide Last Name";
                    return gVal;
                }

                if (model.BranchId < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Please select Bank Branch";
                    return gVal;
                }

                if (model.RoleId < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Please select a role";
                    return gVal;
                }

                if (string.IsNullOrEmpty(model.PhoneNumber))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide Phone Number";
                    return gVal;
                }

                gVal.Code = 5;
                return gVal;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                gVal.Code = -1;
                gVal.Error = "Validation failed. Please try again.";
                return gVal;
            }
        }

        private GenericValidator ValidateBankUser(UserViewModel model)
        {
            var gVal = new GenericValidator();
            try
            {
                if (string.IsNullOrEmpty(model.Email))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide Email";
                    return gVal;
                }

                if (string.IsNullOrEmpty(model.FirstName))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide Other Names";
                    return gVal;
                }

                if (string.IsNullOrEmpty(model.LastName))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide Last Name";
                    return gVal;
                }

                if (string.IsNullOrEmpty(model.BankBranchName)) 
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide Bank Branch Name";
                    return gVal;
                }

                if (string.IsNullOrEmpty(model.BranchCode))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide Bank Branch Code";
                    return gVal;
                }

                if (model.BankId < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "An unknown error was encountered. Please refresh the page and try again.";
                    return gVal;
                }
                
                if (string.IsNullOrEmpty(model.PhoneNumber))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide Phone Number";
                    return gVal;
                }

                gVal.Code = 5;
                return gVal;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                gVal.Code = -1;
                gVal.Error = "Validation failed. Please try again.";
                return gVal;
            }
        }
        private GenericValidator ValidateAppUser(UserViewModel model)  
        {
            var gVal = new GenericValidator(); 
            try
            {
                if (string.IsNullOrEmpty(model.Email))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide Email";
                    return gVal;
                }

                if (string.IsNullOrEmpty(model.PhoneNumber))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide Phone Number";
                    return gVal;
                }

                if (string.IsNullOrEmpty(model.Role))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please select a role for the user";
                    return gVal;
                }

                gVal.Code = 5;
                return gVal;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                gVal.Code = -1;
                gVal.Error = "Validation failed. Please try again.";
                return gVal;
            }
        }

        public ActionResult GetFeeStructures()
        {
            try
            {
                var fees = new FeeServices().GetFees();
                return Json(fees,
                   JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return Json(new List<FeeObject>(), JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Disassociate(string loginProvider, string providerKey)
        {
            ManageMessageId? message = null;
            IdentityResult result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(), new UserLoginInfo(loginProvider, providerKey));
            if (result.Succeeded)
            {
                message = ManageMessageId.RemoveLoginSuccess;
            }
            else
            {
                message = ManageMessageId.Error;
            }
            return RedirectToAction("Manage", new { Message = message });
        }
        
        public ActionResult Manage(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : message == ManageMessageId.Error ? "An error has occurred."
                : "";
            ViewBag.HasLocalPassword = HasPassword();
            ViewBag.ReturnUrl = Url.Action("Manage");
            return View();
        }
        
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var user = await UserManager.FindAsync(loginInfo.Login);
            if (user != null)
            {
                await SignInAsync(user, isPersistent: false);
                return RedirectToLocal(returnUrl);
            }
            else
            {
                // If the user does not have an account, then prompt the user to create an account
                ViewBag.ReturnUrl = returnUrl;
                ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { UserName = loginInfo.DefaultUserName });
            }
        }
        
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser() { UserName = model.UserName };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInAsync(user, false);
                        return RedirectToLocal(returnUrl);
                    }
                }

                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }
        
        [HttpPost]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut();
            Session["_importerInfo"] = null;
            return RedirectToAction("Login", "Account");
        }

        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }
        
        protected override void Dispose(bool disposing)
        {
            if (disposing && UserManager != null)
            {
                UserManager.Dispose();
                UserManager = null;
            }
            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private async Task SignInAsync(ApplicationUser user, bool isPersistent)
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            var identity = await UserManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
            AuthenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = isPersistent }, identity);
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private bool HasPassword()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            Error
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            //if (Url.IsLocalUrl(returnUrl))
            //{
            //    return Redirect(returnUrl);
            //}
            //else
            //{

            return Redirect("http://localhost:20017/ngy.html#/");
           
            //}
        }
        
        private class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            //public override void ExecuteResult(ControllerContext context)
            //{
            //    var properties = new AuthenticationProperties() { RedirectUri = RedirectUri };
            //    if (UserId != null)
            //    {
            //        properties.Dictionary[XsrfKey] = UserId;
            //    }
            //    HttpContextBaseExtensions.GetOwinContext(context.HttpContext).Authentication.Challenge(properties, LoginProvider);
            //}
        }
        #endregion
    }

   

}