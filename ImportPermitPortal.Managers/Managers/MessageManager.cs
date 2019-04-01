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
    public class MessageManager
    {
        public long AddMessage(MessageObject message) 
        {
            try
            {
                if (message == null)
                {
                    return -2;
                }

                var messageEntity = ModelMapper.Map<MessageObject, Message>(message);

                if (messageEntity == null || messageEntity.UserId < 1)
                {
                    return -2;
                }

                using (var db = new ImportPermitEntities())
                {
                    var returnStatus = db.Messages.Add(messageEntity);
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


        public long AddMessagePerm(MessageObject message)
        {
            try
            {
                if (message == null)
                {
                    return -2;
                }

                var messageEntity = ModelMapper.Map<MessageObject, Message>(message);

                if (messageEntity == null)
                {
                    return -2;
                }

                using (var db = new ImportPermitEntities())
                {
                    var returnStatus = db.Messages.Add(messageEntity);
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


        public long UpdateMessage(MessageObject message)
        {
            try
            {
                if (message == null)
                {
                    return -2;
                }

                using (var db = new ImportPermitEntities())
                {
                    var msgs = db.Messages.Where(k => k.Id == message.Id).ToList();
                    if (!msgs.Any())
                    {
                        return -2;
                    }
                    var msg = msgs[0];
                    msg.MessageTemplateId = message.MessageTemplateId;
                    msg.Status = message.Status;
                    msg.DateSent = msg.DateSent;
                    msg.MessageBody = message.MessageBody;

                    db.Entry(msg).State = EntityState.Modified;
                    db.SaveChanges();
                    return msg.Id;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }
        
        public List<MessageObject> GetMessages()
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var countries = db.Messages.ToList();
                    if (!countries.Any())
                    {
                        return new List<MessageObject>();
                    }
                    var objList = new List<MessageObject>();
                    countries.ForEach(app =>
                    {
                        var messageObject = ModelMapper.Map<Message, MessageObject>(app);
                        if (messageObject != null && messageObject.Id > 0)
                        {
                            objList.Add(messageObject);
                        }
                    });

                    return !objList.Any() ? new List<MessageObject>() : objList;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return null;
            }
        }
        
        public List<MessageObject> GetMessages(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
                {
                    var tpageNumber = (int)pageNumber;
                    var tsize = (int)itemsPerPage;

                    using (var db = new ImportPermitEntities())
                    {
                        var messages = (from msg in db.Messages.OrderByDescending(g => g.DateSent).Include("MessageTemplate")
                             .Skip(tpageNumber).Take(tsize) join usr in db.UserProfiles.Include("Person").Include("AspNetUsers") on msg.UserId equals usr.Id
                             select new MessageObject
                             {
                                 EventTypeId = msg.MessageTemplate.EventTypeId,
                                 Id = msg.Id,
                                 UserId = msg.UserId,
                                 MessageTemplateId = msg.MessageTemplateId,
                                 Status = msg.Status,
                                 Receipient = usr.Person.FirstName + " " + usr.Person.LastName,
                                 Subject = msg.MessageTemplate.Subject,
                                 MessageContent = msg.MessageTemplate.MessageContent,
                                 Footer = msg.MessageTemplate.Footer

                             }).ToList();

                        if (messages.Any())
                        {
                            messages.ForEach(app =>
                            {
                                var msgEvent = Enum.GetName(typeof(MessageEventEnum), app.EventTypeId);
                                if (msgEvent != null)
                                {
                                    app.EventTypeName = msgEvent.Replace("_", " ");

                                }
                                app.StatusStr = Enum.GetName(typeof(MessageStatus), app.Status).Replace("_", " ");

                                app.DateSentStr = app.DateSent.ToString("dd/MM/yyyy"); 
                            });
                            countG = db.Messages.Count();
                            return messages;
                        }
                    }

                }
                countG = 0;
                return new List<MessageObject>();
            }
            catch (Exception ex)
            {
                countG = 0;
                return new List<MessageObject>();
            }
        }

        public List<MessageObject> GetMyMessages(int? itemsPerPage, int? pageNumber, out int countG, long userId)
        {
            try
            {
                if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
                {
                    var tpageNumber = (int)pageNumber;
                    var tsize = (int)itemsPerPage;

                    using (var db = new ImportPermitEntities())
                    {
                        var messages = (from msg in db.Messages.Where(m => m.UserId == userId).OrderByDescending(g => g.Id).Include("MessageTemplate")
                             .Skip(tpageNumber).Take(tsize)
                                       
                                        select new MessageObject
                                        {
                                            EventTypeId = msg.MessageTemplate.EventTypeId,
                                            Id = msg.Id,
                                            UserId = msg.UserId,
                                            MessageTemplateId = msg.MessageTemplateId,
                                            Status = msg.Status,
                                            Subject = msg.MessageTemplate.Subject,
                                            MessageContent = msg.MessageTemplate.MessageContent,
                                            Footer = msg.MessageTemplate.Footer,
                                            DateSent = msg.DateSent
                                        }).ToList();

                        if (messages.Any())
                        {
                            messages.ForEach(app =>
                            {
                                var msgEvent = Enum.GetName(typeof(MessageEventEnum), app.EventTypeId);
                                if (msgEvent != null)
                                {
                                    app.EventTypeName = msgEvent.Replace("_", " ");

                                }
                                app.StatusStr = Enum.GetName(typeof(MessageStatus), app.Status).Replace("_", " ");

                                app.DateSentStr = app.DateSent.ToString("dd/MM/yyyy"); 
                            });
                            countG = db.Messages.Count(m => m.UserId == userId);
                            return messages;
                        }
                    }

                }
                countG = 0;
                return new List<MessageObject>();
            }
            catch (Exception ex)
            {
                countG = 0;
                return new List<MessageObject>();
            }
        }

        public List<MessageObject> GetMyLatestMessages(long userId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var messages = (from msg in db.Messages.Where(m => m.UserId == userId).OrderByDescending(g => g.Id).Include("MessageTemplate").Take(5)
                                    select new MessageObject
                                    {
                                        EventTypeId = msg.MessageTemplate.EventTypeId,
                                        Id = msg.Id,
                                        UserId = msg.UserId,
                                        MessageTemplateId = msg.MessageTemplateId,
                                        Status = msg.Status,
                                        Subject = msg.MessageTemplate.Subject,
                                        MessageContent = msg.MessageTemplate.MessageContent,
                                        Footer = msg.MessageTemplate.Footer,
                                        DateSent = msg.DateSent
                                    }).ToList();

                    if (messages.Any())
                    {
                        messages.ForEach(app =>
                        {
                            var msgEvent = Enum.GetName(typeof(MessageEventEnum), app.EventTypeId);
                            if (msgEvent != null)
                            {
                                app.EventTypeName = msgEvent.Replace("_", " ");

                            }
                            app.StatusStr = Enum.GetName(typeof(MessageStatus), app.Status).Replace("_", " ");

                            app.DateSentStr = app.DateSent.ToString("dd/MM/yyyy");
                        });
                        return messages;
                    }
                    return new List<MessageObject>();
                }
            }
            catch (Exception ex)
            {
                return new List<MessageObject>();
            }
        }

        public List<MessageObject> GetMyMessages(long userId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var messages = (from msg in db.Messages.Where(m => m.UserId == userId).OrderByDescending(g => g.DateSent).Include("MessageTemplate")
                         
                                    join usr in db.UserProfiles.Include("Person").Include("AspNetUsers") on msg.UserId equals usr.Id
                                    select new MessageObject
                                    {
                                        EventTypeId = msg.MessageTemplate.EventTypeId,
                                        Id = msg.Id,
                                        UserId = msg.UserId,
                                        MessageTemplateId = msg.MessageTemplateId,
                                        Status = msg.Status,
                                        Receipient = usr.Person.FirstName + " " + usr.Person.LastName,
                                        Subject = msg.MessageTemplate.Subject,
                                        MessageContent = msg.MessageBody,
                                        Footer = msg.MessageTemplate.Footer, 
                                        DateSent = msg.DateSent

                                    }).ToList();

                    if (!messages.Any())
                    {
                        return new List<MessageObject>();
                    }
                    messages.ForEach(app =>
                    {
                        var msgEvent = Enum.GetName(typeof(MessageEventEnum), app.EventTypeId);
                        if (msgEvent != null)
                        {
                            app.EventTypeName = msgEvent.Replace("_", " ");

                        }
                        app.StatusStr = Enum.GetName(typeof (MessageStatus), app.Status).Replace("_", " ");

                        app.DateSentStr = app.DateSent.ToString("dd/MM/yyyy"); 
                    });
                    return messages;
                }
            }
            catch (Exception ex)
            {
                return new List<MessageObject>();
            }
        }

        public List<MessageObject> SearchMessages(string searchCriteria, long userId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var messages = (from msg in db.Messages.Where(m => m.UserId == userId && m.MessageTemplate.Subject.Contains(searchCriteria)).OrderByDescending(g => g.DateSent).Include("MessageTemplate")

                                    select new MessageObject
                                    {
                                        EventTypeId = msg.MessageTemplate.EventTypeId,
                                        Id = msg.Id,
                                        MessageTemplateId = msg.MessageTemplateId,
                                        Status = msg.Status,
                                        Subject = msg.MessageTemplate.Subject,
                                        MessageContent = msg.MessageBody,
                                        Footer = msg.MessageTemplate.Footer,
                                        DateSent = msg.DateSent

                                    }).ToList();

                    if (!messages.Any())
                    {
                        return new List<MessageObject>();
                    }
                    messages.ForEach(app =>
                    {
                        var msgEvent = Enum.GetName(typeof(MessageEventEnum), app.EventTypeId);
                        if (msgEvent != null)
                        {
                            app.EventTypeName = msgEvent.Replace("_", " ");

                        }
                        app.StatusStr = Enum.GetName(typeof(MessageStatus), app.Status).Replace("_", " ");

                        app.DateSentStr = app.DateSent.ToString("dd/MM/yyyy");
                    });
                    return messages;
                }
            }
            catch (Exception ex)
            {
                return new List<MessageObject>();
            }
        }

        public MessageObject GetMessage(long messageId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var messages = (from msg in db.Messages.Where(o => o.Id == messageId).OrderByDescending(g => g.DateSent).Include("MessageTemplate")
                                    join usr in db.UserProfiles.Include("Person").Include("AspNetUsers") on msg.UserId equals usr.Id
                                    select new MessageObject
                                    {
                                        EventTypeId = msg.MessageTemplate.EventTypeId,
                                        Id = msg.Id,
                                        UserId = msg.UserId,
                                        MessageTemplateId = msg.MessageTemplateId,
                                        Status = msg.Status,
                                        Receipient = usr.Person.FirstName + " " + usr.Person.LastName,
                                        Subject = msg.MessageTemplate.Subject,
                                        MessageContent = msg.MessageBody,
                                        Footer = msg.MessageTemplate.Footer

                                    }).ToList();

                    if (!messages.Any())
                    {
                        return new MessageObject();
                    }

                    var app = messages[0];
                    var msgEvent = Enum.GetName(typeof(MessageEventEnum), app.EventTypeId);
                    if (msgEvent != null)
                    {
                        app.EventTypeName = msgEvent.Replace("_", " ");

                    }
                    var status = Enum.GetName(typeof(MessageStatus), app.Status);
                    if (status != null)
                    {
                        app.StatusStr = status.Replace("_", " ");
                    }
                    return app;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new MessageObject();
            }
        }

        public List<MessageObject> Search(string searchCriteria)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var messages = (from msgt in db.MessageTemplates.Where(u => u.Subject.ToLower().Contains(searchCriteria.ToLower()))
                                    join msg in db.Messages.OrderByDescending(g => g.DateSent) on msgt.Id equals msg.MessageTemplateId
                                    join usr in db.UserProfiles.Include("Person").Include("AspNetUsers") on msg.UserId equals usr.Id
                                    select new MessageObject
                                    {
                                        EventTypeId = msgt.EventTypeId,
                                        Id = msg.Id,
                                        UserId = msg.UserId,
                                        MessageTemplateId = msgt.Id,
                                        Status = msg.Status,
                                        Receipient = usr.Person.FirstName + " " + usr.Person.LastName,
                                        Subject = msgt.Subject,
                                        MessageContent = msgt.MessageContent,
                                        Footer = msgt.Footer

                                    }).ToList();

                    if (!messages.Any())
                    {
                        return new List<MessageObject>();
                    }

                    messages.ForEach(app =>
                    {
                        var msgEvent = Enum.GetName(typeof(MessageEventEnum), app.EventTypeId);
                        if (msgEvent != null)
                        {
                            app.EventTypeName = msgEvent.Replace("_", " ");

                        }
                        var status = Enum.GetName(typeof(MessageStatus), app.Status);
                        if (status != null)
                        {
                            app.EventTypeName = status.Replace("_", " ");
                        }
                    });
                    return messages;
                }
            }
            catch (Exception ex)
            {
                return new List<MessageObject>();
            }
        }
        public long DeleteMessage(long messageId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var myItems =
                        db.Messages.Where(m => m.Id == messageId).ToList();
                    if (!myItems.Any())
                    {
                        return 0;
                    }

                    var item = myItems[0];
                    db.Messages.Remove(item);
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
