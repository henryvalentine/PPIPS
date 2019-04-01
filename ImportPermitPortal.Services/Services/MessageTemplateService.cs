using System;
using System.Collections.Generic;
using System.Linq;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Managers.Managers;

namespace ImportPermitPortal.Services.Services
{
	public class MessageTemplateServices
	{
        private readonly MessageTemplateManager _messageTemplateManager;
        public MessageTemplateServices()
		{
            _messageTemplateManager = new MessageTemplateManager();
		}

        public long AddMessageTemplate(MessageTemplateObject messageTemplate)
		{
			try
			{
                return _messageTemplateManager.AddMessageTemplate(messageTemplate);
			}
			catch (Exception ex)
			{
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
				return 0;
			}
		}

        public long DeleteMessageTemplate(long messageTemplateId)
        {
            try
            {
                return _messageTemplateManager.DeleteMessageTemplate(messageTemplateId);
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
                return _messageTemplateManager.UpdateMessageTemplate(messageTemplate);
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
                var objList = _messageTemplateManager.GetMessageTemplates();
                if (objList == null || !objList.Any())
			    {
                    return new List<MessageTemplateObject>();
			    }
				return objList;
			}
			catch (Exception ex)
			{
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<MessageTemplateObject>();
			}
		}

        public MessageTemplateObject GetMessageTemplate(long messageTemplateId)
        {
            try
            {
                return _messageTemplateManager.GetMessageTemplate(messageTemplateId);
                
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
                return _messageTemplateManager.GetMessageTemp(messageEventId);

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
                return _messageTemplateManager.GetMessageTemp(messageEventId, email);

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
                return _messageTemplateManager.GetMessageTempWithExpiry(messageEventId);

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
                return _messageTemplateManager.GetMessageTemplate(messageEventId, email);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new MessageTemplateObject();
            }
        }

        public List<MessageTemplateObject> GetMessageTemplates(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                var objList = _messageTemplateManager.GetMessageTemplates(itemsPerPage, pageNumber, out countG);
                if (objList == null || !objList.Any())
                {
                    return new List<MessageTemplateObject>();
			    }
                
                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                countG = 0;
                return new List<MessageTemplateObject>();
            }
        }

        public List<MessageTemplateObject> Search(string searchCriteria)
        {
            try
            {
                var objList = _messageTemplateManager.Search(searchCriteria);
                if (objList == null || !objList.Any())
                {
                    return new List<MessageTemplateObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<MessageTemplateObject>();
            }
        }
	}

}
