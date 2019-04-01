using System;
using System.Collections.Generic;
using System.Linq;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Managers.Managers;

namespace ImportPermitPortal.Services.Services
{
	public class MessageServices
	{
        private readonly MessageManager _messageManager;
        public MessageServices()
		{
            _messageManager = new MessageManager();
		}

        public long AddMessage(MessageObject message)
		{
			try
			{
                return _messageManager.AddMessage(message);
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
                return _messageManager.AddMessagePerm(message);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long DeleteMessage(long messageId)
        {
            try
            {
                return _messageManager.DeleteMessage(messageId);
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
                return _messageManager.UpdateMessage(message);
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
                var objList = _messageManager.GetMessages();
                if (objList == null || !objList.Any())
			    {
                    return new List<MessageObject>();
			    }
				return objList;
			}
			catch (Exception ex)
			{
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<MessageObject>();
			}
		}

        public MessageObject GetMessage(long messageId)
        {
            try
            {
                return _messageManager.GetMessage(messageId);
                
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new MessageObject();
            }
        }

        public List<MessageObject> GetMessages(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                var objList = _messageManager.GetMessages(itemsPerPage, pageNumber, out countG);
                if (objList == null || !objList.Any())
                {
                    return new List<MessageObject>();
			    }
                
                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                countG = 0;
                return new List<MessageObject>();
            }
        }

        public List<MessageObject> GetMyMessages(int? itemsPerPage, int? pageNumber, out int countG, long userId)
        {
            try
            {
                return _messageManager.GetMyMessages(itemsPerPage, pageNumber, out countG, userId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                countG = 0;
                return new List<MessageObject>();
            }
        }

        public List<MessageObject> GetMyLatestMessages(long userId)
        {
            try
            {
                return _messageManager.GetMyLatestMessages(userId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<MessageObject>();
            }
        }

        public List<MessageObject> SearchMessages(string searchCriteria, long userId)
        {
            try
            {
                return _messageManager.SearchMessages(searchCriteria, userId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<MessageObject>();
            }
        }

        public List<MessageObject> GetMyMessages(long userId)
        {
            try
            {
                return _messageManager.GetMyMessages(userId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<MessageObject>();
            }
        }

        public List<MessageObject> Search(string searchCriteria)
        {
            try
            {
                var objList = _messageManager.Search(searchCriteria);
                if (objList == null || !objList.Any())
                {
                    return new List<MessageObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<MessageObject>();
            }
        }
	}

}
