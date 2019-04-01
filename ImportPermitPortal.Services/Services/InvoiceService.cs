using System;
using System.Collections.Generic;
using System.Linq;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Managers.Managers;

namespace ImportPermitPortal.Services.Services
{
	public class InvoiceServices
	{
        private readonly InvoiceManager _invoiceManager; 
        public InvoiceServices()
		{
            _invoiceManager = new InvoiceManager();
		}

        public ResponseObject AddInvoice(InvoiceObject invoice)
		{
			try
			{
                return _invoiceManager.AddInvoice(invoice);
			}
			catch (Exception ex)
			{
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
				return new ResponseObject();
			}
		}

        public ResponseObject AddExpenditionaryInvoice(InvoiceObject invoice, long notificationId)
        {
            try
            {
                return _invoiceManager.AddExpenditionaryInvoice(invoice, notificationId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new ResponseObject();
            }
        }

        public long DeleteInvoice(long invoiceId)
        {
            try
            {
                return _invoiceManager.DeleteInvoice(invoiceId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long UpdateInvoice(InvoiceObject invoice, List<InvoiceItemObject> newInvoiceItems)
        {
            try
            {
                return _invoiceManager.UpdateInvoice(invoice, newInvoiceItems);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long UpdateInvoiceRefCode(InvoiceObject invoice)
        {
            try
            {
                return _invoiceManager.UpdateInvoiceRefCode(invoice);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public ResponseObject UpdateInvoiceWithouthAttach(InvoiceObject invoice)
        {
            try
            {
                return _invoiceManager.UpdateInvoiceWithouthAttach(invoice);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new ResponseObject();
            }
        }

        public long UpdateInvoiceRrr(string refCode, string rrr)
        {
            try
            {
                return _invoiceManager.UpdateInvoiceRrr(refCode, rrr);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public string UpdateInvoiceGetRefCode(InvoiceObject invoice)
        {
            try
            {
                return _invoiceManager.UpdateInvoiceGetRefCode(invoice);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return "";
            }
        }
        
        public InvoiceObject GetInvoice(long invoiceId)
        {
            try
            {
                return _invoiceManager.GetInvoice(invoiceId);
                
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new InvoiceObject();
            }
        }

        public InvoiceObject VerifyRrr(string rrr)
        {
            try
            {
                return _invoiceManager.VerifyRrr(rrr);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new InvoiceObject();
            }
        }
        public InvoiceObject GetReceipt(long invoiceId, long importerId)
        {
            try
            {
                return _invoiceManager.GetReceipt(invoiceId, importerId);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new InvoiceObject();
            }
        }

        public InvoiceObject GetReceiptInfo(long invoiceId)
        {
            try
            {
                return _invoiceManager.GetReceiptInfo(invoiceId);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new InvoiceObject();
            }
        }

        public InvoiceObject GetInvoice(long invoiceId, long importerId)
        {
            try
            {
                return _invoiceManager.GetInvoice(invoiceId, importerId);
                
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new InvoiceObject();
            }
        }

        public List<InvoiceObject> GetInvoices(int? itemsPerPage, int? pageNumber, out int countG)
         {
             try
             {
                 var objList = _invoiceManager.GetInvoices(itemsPerPage, pageNumber, out countG);
                 if (objList == null || !objList.Any())
                 {
                     return new List<InvoiceObject>();
                 }

                 return objList;
             }
             catch (Exception ex)
             {
                 ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                 countG = 0;
                 return new List<InvoiceObject>();
             }
         }

        public List<InvoiceObject> GetInvoices(int? itemsPerPage, int? pageNumber, out int countG, long importerId)
         {
             try
             {
                 return _invoiceManager.GetInvoices(itemsPerPage, pageNumber, out countG, importerId);
             }
             catch (Exception ex)
             {
                 ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                 countG = 0;
                 return new List<InvoiceObject>();
             }
         }

        public List<InvoiceObject> GetPaidInvoices(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                return _invoiceManager.GetPaidInvoices(itemsPerPage, pageNumber, out countG);
              
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                countG = 0;
                return new List<InvoiceObject>();
            }
        }

        public List<InvoiceObject> GetMyReceiptsGetPaidInvoices(int? itemsPerPage, int? pageNumber, out int countG)
		{
			try
			{
                return _invoiceManager.GetPaidInvoices(itemsPerPage, pageNumber, out countG);
			}
			catch (Exception ex)
			{
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                countG = 0;
                return new List<InvoiceObject>();
			}
		}

        public List<InvoiceObject> Search(string searchCriteria)
        {
            try
            {
                var objList = _invoiceManager.Search(searchCriteria);
                if (objList == null || !objList.Any())
                {
                    return new List<InvoiceObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<InvoiceObject>();
            }
        }

        public List<InvoiceObject> Search(string searchCriteria, long importerId)
            
        {
            try
            {
                var objList = _invoiceManager.Search(searchCriteria, importerId);
                if (objList == null || !objList.Any())
                {
                    return new List<InvoiceObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<InvoiceObject>();
            }
        }
        public List<InvoiceObject> SearchPaidInvoices(string searchCriteria)
        {
            try
            {
                var objList = _invoiceManager.SearchPaidInvoice(searchCriteria);
                if (objList == null || !objList.Any())
                {
                    return new List<InvoiceObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<InvoiceObject>();
            }
        }
        
	}

}
