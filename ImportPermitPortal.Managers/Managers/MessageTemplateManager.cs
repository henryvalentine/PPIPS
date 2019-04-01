using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.DynamicMapper.DynamicMapper;
using ImportPermitPortal.EF.Model;

namespace ImportPermitPortal.Managers.Managers
{
    public class MessageTemplateManager
    {
        public long AddMessageTemplate(MessageTemplateObject messageTemplate)
        {
            try
            {
                if (messageTemplate == null)
                {
                    return -2;
                }

                var messageTemplateEntity = ModelMapper.Map<MessageTemplateObject, MessageTemplate>(messageTemplate);

                if (messageTemplateEntity == null || messageTemplateEntity.EventTypeId < 1)
                {
                    return -2;
                }

                using (var db = new ImportPermitEntities())
                {
                    var returnStatus = db.MessageTemplates.Add(messageTemplateEntity);
                    db.SaveChanges();
                    return returnStatus.Id;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long UpdateMessageTemplate(MessageTemplateObject messageTemplate)
        {
            try
            {
                if (messageTemplate == null)
                {
                    return -2;
                }

                var messageTemplateEntity = ModelMapper.Map<MessageTemplateObject, MessageTemplate>(messageTemplate);
                if (messageTemplateEntity == null || messageTemplateEntity.Id < 1)
                {
                    return -2;
                }

                using (var db = new ImportPermitEntities())
                {
                    db.MessageTemplates.Attach(messageTemplateEntity);
                    db.Entry(messageTemplateEntity).State = EntityState.Modified;
                    db.SaveChanges();
                    return messageTemplate.Id;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }
        
        public List<MessageTemplateObject> GetMessageTemplates()
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var templates = db.MessageTemplates.ToList();
                    if (!templates.Any())
                    {
                        return new List<MessageTemplateObject>();
                    }
                    var objList = new List<MessageTemplateObject>();
                    templates.ForEach(app =>
                    {
                        var messageTemplateObject = ModelMapper.Map<MessageTemplate, MessageTemplateObject>(app);
                        if (messageTemplateObject != null && messageTemplateObject.Id > 0)
                        {
                            objList.Add(messageTemplateObject);
                        }
                    });

                    return !objList.Any() ? new List<MessageTemplateObject>() : objList;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return null;
            }
        }

      
        public List<MessageTemplateObject> GetMessageTemplates(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
                {
                    var tpageNumber = (int)pageNumber;
                    var tsize = (int)itemsPerPage;

                    using (var db = new ImportPermitEntities())
                    {
                        var templates =
                            db.MessageTemplates
                            .OrderByDescending(m => m.Id)
                             .Skip(tpageNumber).Take(tsize)
                             .ToList();
                        if (templates.Any())
                        {
                            var newList = new List<MessageTemplateObject>();
                            templates.ForEach(app =>
                            {
                                var messageTemplateObject = ModelMapper.Map<MessageTemplate, MessageTemplateObject>(app);
                                if (messageTemplateObject != null && messageTemplateObject.Id > 0)
                                {
                                    var msgEvent = Enum.GetName(typeof (MessageEventEnum), messageTemplateObject.EventTypeId);
                                    if (msgEvent != null)
                                    {
                                        messageTemplateObject.EventTypeName = msgEvent.Replace("_", " ");
                                    }

                                    newList.Add(messageTemplateObject);
                                }
                            });
                            countG = db.MessageTemplates.Count();
                            return newList;
                        }
                    }

                }
                countG = 0;
                return new List<MessageTemplateObject>();
            }
            catch (Exception ex)
            {
                countG = 0;
                return new List<MessageTemplateObject>();
            }
        }

        public MessageTemplateObject GetMessageTemplate(long messageTemplateId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var templates =
                        db.MessageTemplates.Where(m => m.Id == messageTemplateId)
                            .ToList();
                    if (!templates.Any())
                    {
                        return new MessageTemplateObject();
                    }

                    var app = templates[0];
                    var messageTemplateObject = ModelMapper.Map<MessageTemplate, MessageTemplateObject>(app);
                    if (messageTemplateObject == null || messageTemplateObject.Id < 1)
                    {
                        return new MessageTemplateObject();
                    }
                    var msgEvent = Enum.GetName(typeof(MessageEventEnum), messageTemplateObject.EventTypeId);
                    if (msgEvent != null)
                    {
                        messageTemplateObject.EventTypeName = msgEvent.Replace("_", " ");
                    }

                    return messageTemplateObject;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new MessageTemplateObject();
            }
        }

        public MessageTemplateObject GetMessageTemp(int messageEventId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var templates = db.MessageTemplates.Where(m => m.EventTypeId == messageEventId).ToList();
                    if (!templates.Any())
                    {
                        return new MessageTemplateObject();
                    }
                    var exp = db.ImportSettings.ToList();
                    if (!exp.Any())
                    {
                        return new MessageTemplateObject();
                    }
                    var app = templates[0];

                    var messageTemplateObject = ModelMapper.Map<MessageTemplate, MessageTemplateObject>(app);
                    if (messageTemplateObject == null || messageTemplateObject.Id < 1)
                    {
                        return new MessageTemplateObject();
                    }
                    
                    messageTemplateObject.MessageLifeSpan = exp[0].MessageLifeSpan;
                    return messageTemplateObject;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new MessageTemplateObject();
            }
        }


        public MessageTemplateObject GetMessageTemp(int messageEventId, string email)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var templates = db.MessageTemplates.Where(m => m.EventTypeId == messageEventId).ToList();
                    if (!templates.Any())
                    {
                        return new MessageTemplateObject();
                    }

                    var users = db.AspNetUsers.Where(e => e.Email == email).Include("UserProfile").ToList();
                    if (!users.Any())
                    {
                        return new MessageTemplateObject();
                    }

                    var exp = db.ImportSettings.ToList();
                    if (!exp.Any())
                    {
                        return new MessageTemplateObject();
                    }
                    var app = templates[0];

                    var messageTemplateObject = ModelMapper.Map<MessageTemplate, MessageTemplateObject>(app);
                    if (messageTemplateObject == null || messageTemplateObject.Id < 1)
                    {
                        return new MessageTemplateObject();
                    }

                    var userInfoId = users[0].UserInfo_Id;
                    if (userInfoId != null) messageTemplateObject.UserId = (long) userInfoId;
                    messageTemplateObject.MessageLifeSpan = exp[0].MessageLifeSpan;
                    return messageTemplateObject;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new MessageTemplateObject();
            }
        }

        public MessageTemplateObject GetMessageTempWithExpiry(int messageEventId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var templates = db.MessageTemplates.Where(m => m.EventTypeId == messageEventId).ToList();
                    var settings = db.ImportSettings.ToList();
                    if (!templates.Any() || !settings.Any())
                    {
                        return new MessageTemplateObject();
                    }
                    var app = templates[0];
                    var setting = settings[0];
                    var messageTemplateObject = ModelMapper.Map<MessageTemplate, MessageTemplateObject>(app);
                    if (messageTemplateObject == null || messageTemplateObject.Id < 1)
                    {
                        return new MessageTemplateObject();
                    }
                    messageTemplateObject.MessageLifeSpan = setting.MessageLifeSpan;
                    return messageTemplateObject;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new MessageTemplateObject();
            }
        }

        public MessageTemplateObject GetMessageTemplate(int messageEventId, string email)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var users = db.AspNetUsers.Where(a => a.Email == email).Include("UserProfile").ToList();
                    if (!users.Any())
                    {
                        return new MessageTemplateObject();
                    }
                    var templates = db.MessageTemplates.Where(m => m.EventTypeId == messageEventId).ToList();
                    var settings = db.ImportSettings.ToList();
                    if (!templates.Any() || !settings.Any())
                    {
                        return new MessageTemplateObject();
                    }
                    var app = templates[0];
                    var setting = settings[0];
                    var messageTemplateObject = ModelMapper.Map<MessageTemplate, MessageTemplateObject>(app);
                    if (messageTemplateObject == null || messageTemplateObject.Id < 1)
                    {
                        return new MessageTemplateObject();
                    }
                    var userId = users[0].UserProfile.Id;
                    messageTemplateObject.MessageLifeSpan = setting.MessageLifeSpan;
                    messageTemplateObject.UserId = userId;
                    return messageTemplateObject;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new MessageTemplateObject();
            }
        }

        public List<MessageTemplateObject> Search(string searchCriteria)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var templates = db.MessageTemplates.Where(m => m.Subject.ToLower() == searchCriteria.ToLower().Trim()).ToList();
                    if (!templates.Any())
                    {
                        return new List<MessageTemplateObject>();
                    }
                    var newList = new List<MessageTemplateObject>();
                    templates.ForEach(app =>
                    {
                        var messageTemplateObject = ModelMapper.Map<MessageTemplate, MessageTemplateObject>(app);
                        if (messageTemplateObject != null && messageTemplateObject.EventTypeId > 0)
                        {
                            var msgEvent = Enum.GetName(typeof(MessageEventEnum), messageTemplateObject.EventTypeId);
                            if (msgEvent != null)
                            {
                                messageTemplateObject.EventTypeName = msgEvent.Replace("_", " ");
                            }

                            newList.Add(messageTemplateObject);
                        }
                    });
                    return newList;
                }
            }
            catch (Exception ex)
            {
                return new List<MessageTemplateObject>();
            }
        }
        public long DeleteMessageTemplate(long messageTemplateId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var myItems =
                        db.MessageTemplates.Where(m => m.Id == messageTemplateId).ToList();
                    if (!myItems.Any())
                    {
                        return 0;
                    }

                    var item = myItems[0];
                    db.MessageTemplates.Remove(item);
                    db.SaveChanges();
                    return 5;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }
    }
}
