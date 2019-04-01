using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Helpers;
using ImportPermitPortal.Services.Services;

namespace ImportPermitPortal.Controllers
{

      [Authorize(Roles = "Super_Admin")]
    public class MessageTemplateController : Controller
    {
          
       [HttpGet]
       public ActionResult GetMessageTemplateObjects(JQueryDataTableParamModel param)
       {
           try
           {
               IEnumerable<MessageTemplateObject> filteredParentMenuObjects;
               var countG = 0;
               
               var pagedParentMenuObjects = GetMessageTemplates(param.iDisplayLength, param.iDisplayStart, out countG);

               if (!string.IsNullOrEmpty(param.sSearch))
               {
                   filteredParentMenuObjects = new MessageTemplateServices().Search(param.sSearch);
               }
               else
               {
                   filteredParentMenuObjects = pagedParentMenuObjects;
               }

               if (!filteredParentMenuObjects.Any())
               {
                   return Json(new List<MessageTemplateObject>(), JsonRequestBehavior.AllowGet);
               }

               var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
               Func<MessageTemplateObject, string> orderingFunction = (c => sortColumnIndex == 1 ? c.Subject : c.EventTypeName);
                
               var sortDirection = Request["sSortDir_0"]; // asc or desc
               filteredParentMenuObjects = sortDirection == "desc" ? filteredParentMenuObjects.OrderBy(orderingFunction) : filteredParentMenuObjects.OrderByDescending(orderingFunction);

               var displayedPersonnels = filteredParentMenuObjects;

               var result = from c in displayedPersonnels
                            select new[] { Convert.ToString(c.Id), c.Subject, c.EventTypeName};
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
               return Json(new List<MessageTemplateObject>(), JsonRequestBehavior.AllowGet);
           }
       }
          
        [HttpPost]
        public ActionResult AddMessageTemplate(MessageTemplateObject messageTemplate)
        {
            var gVal = new GenericValidator();

            try
            {
                
                var validationResult = ValidateMessageTemplate(messageTemplate);

                if (validationResult.Code == 1)
                {
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                var appStatus = new MessageTemplateServices().AddMessageTemplate(messageTemplate);
                if (appStatus < 1)
                {
                    validationResult.Code = -1;
                    validationResult.Error = appStatus == -2 ? "Message Template processing failed. Please try again." : "Message Template Information already exists";
                    return Json(validationResult, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = appStatus;
                gVal.Error = "Message Template was successfully added.";
                return Json(gVal, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                gVal.Error = "Message Template processing failed. Please try again later";
                gVal.Code = -1;
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost]
        public ActionResult EditMessageTemplate(MessageTemplateObject messageTemplate)
        {
            var gVal = new GenericValidator();

            try
            {
                var stat = ValidateMessageTemplate(messageTemplate);

                if (stat.Code < 1)
                {
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }
                
                if (Session["_messageTemplate"] == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet); 
                }

                var oldmessageTemplate = Session["_messageTemplate"] as MessageTemplateObject;

                if (oldmessageTemplate == null)
                {
                    gVal.Code = -1;
                    gVal.Error = "Session has timed out.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                oldmessageTemplate.Subject = messageTemplate.Subject;
                oldmessageTemplate.EventTypeId = messageTemplate.EventTypeId;
                oldmessageTemplate.MessageContent = messageTemplate.MessageContent;
                oldmessageTemplate.Footer = messageTemplate.Footer;
                var docStatus = new MessageTemplateServices().UpdateMessageTemplate(oldmessageTemplate);
                if (docStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = docStatus == -3 ? "Message Template already exists." : "Message Template information could not be updated. Please try again later";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = oldmessageTemplate.Id;
                gVal.Error = "Message Template information was successfully updated";
                return Json(gVal, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                gVal.Code = -1;
                gVal.Error = "Message Template information could not be updated. Please try again later";
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }
       
        public ActionResult GetMessageTemplates()
        {
            try
            {
                return Json(GetMessageTemplateLists(), JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(new GenericEligibilityList(), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetMessageTemplate(long id)
        {
            try
            {
                var messageTemplate = new MessageTemplateServices().GetMessageTemplate(id);
                if (messageTemplate == null || messageTemplate.Id < 1)
                {
                    return Json(new MessageTemplateObject(), JsonRequestBehavior.AllowGet);
                }

                Session["_messageTemplate"] = messageTemplate;

                return Json(messageTemplate, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                return Json(new MessageTemplateObject(), JsonRequestBehavior.AllowGet);
            }
        }

      
         [HttpPost]
        public ActionResult DeleteMessageTemplate(long id)
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
                var delStatus = new MessageTemplateServices().DeleteMessageTemplate(id);
                if (delStatus < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Message Template could not be deleted. Please try again later.";
                    return Json(gVal, JsonRequestBehavior.AllowGet);
                }

                gVal.Code = 5;
                gVal.Error = "Message Template Information was successfully deleted";
                return Json(gVal, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                return Json(gVal, JsonRequestBehavior.AllowGet);
            }
        }

       
        private GenericValidator ValidateMessageTemplate(MessageTemplateObject messageTemplate)
        {
            var gVal = new GenericValidator();
            try
            {
                if (messageTemplate.EventTypeId < 1)
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide Message Template Event.";
                    return gVal;
                }
                if (string.IsNullOrEmpty(messageTemplate.Subject))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide Message Template Subject.";
                    return gVal;
                }

                if (string.IsNullOrEmpty(messageTemplate.MessageContent))
                {
                    gVal.Code = -1;
                    gVal.Error = "Please provide Message Template Content.";
                    return gVal;
                }

                gVal.Code = 5;
                return gVal;
                
            }
            catch (Exception)
            {
                
                gVal.Code = -1;
                gVal.Error = "Message Template Validation failed. Please provide all required fields and try again.";
                return gVal;
            }
        }

        private List<MessageTemplateObject> GetMessageTemplates(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                return new MessageTemplateServices().GetMessageTemplates(itemsPerPage, pageNumber, out countG);
            }
            catch (Exception)
            {
                countG = 0;
                return new List<MessageTemplateObject>();
            }
        }

        
        private List<MessageTemplateObject> GetMessageTemplateLists()
        {
            try
            {
                return new MessageTemplateServices().GetMessageTemplates();

            }
            catch (Exception)
            {
                return new List<MessageTemplateObject>();
            }
        }

        public ActionResult GetMessageEvents()
        {
            try
            {
                var eventList = EnumToObjList.ConvertEnumToList(typeof (MessageEventEnum));
                return Json(eventList.OrderBy(m => m.Name).ToList(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new List<GenericObject>(), JsonRequestBehavior.AllowGet);
            }
        }

    }
}
