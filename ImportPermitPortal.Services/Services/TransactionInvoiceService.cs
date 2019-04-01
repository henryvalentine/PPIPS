using System;
using System.Collections.Generic;
using System.Linq;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Managers.BusinessTransactions;
namespace ImportPermitPortal.Services.Services
{
	public class TransactionInvoiceServices
	{
        private readonly TransactionManager _transactionManager;
        public TransactionInvoiceServices()
		{
            _transactionManager = new TransactionManager();
		}

        public long AddTransactionInvoice(TransactionInvoiceObject invoice)
		{
			try
			{
                return _transactionManager.AddTransactionInvoice(invoice);
			}
			catch (Exception ex)
			{
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
				return 0;
			}
		}


        public InvoiceObject AddBankPayment(PaymentLogObject payLog)  
        {
            try
            {
                return _transactionManager.AddBankPayment(payLog);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new InvoiceObject();
            }
        }

        public long UpdatePaymentDetails(string rrr)
        {
            try
            {
                return _transactionManager.UpdatePaymentDetails(rrr);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public ApplicationObject AddWebPayment(string code)
        {
            try
            {
                return _transactionManager.AddApplicationWebPayment(code);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new ApplicationObject();
            }
        }

        public InvoiceObject InsertPayment(string rrr, string orderId)
        {
            try
            {
                return _transactionManager.InsertPayment(rrr, orderId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new InvoiceObject();
            }
        }

        public NotificationObject AddNotificationWebPayment(string code)
        {
            try
            {
                return _transactionManager.AddNotificationWebPayment(code);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new NotificationObject();
            }
        }

        public PaymentReceiptObject GetMyReceipt(long receiptId, long importerId)
        {
            try
            {
                return _transactionManager.GetMyReceipt(receiptId, importerId);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new PaymentReceiptObject();
            }
        }

        public long CheckRrrValidity(string rrrCode, string paymentDate)
        {
            try
            {
                return _transactionManager.CheckRrrValidity(rrrCode, paymentDate);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public List<PaymentReceiptObject> GetMyReceipts(int? itemsPerPage, int? pageNumber, out int countG, long importerId)
        {
            try
            {
                return _transactionManager.GetMyReceipts(itemsPerPage, pageNumber, out countG, importerId);
             
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                countG = 0;
                return new List<PaymentReceiptObject>();
            }
        }

        public List<PaymentReceiptObject> GetReceipts(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                return _transactionManager.GetReceipts(itemsPerPage, pageNumber, out countG);

            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                countG = 0;
                return new List<PaymentReceiptObject>();
            }
        }

       public List<PaymentReceiptObject> SearchMyReceipts(string searchCriteria, long importerId)
        {
            try
            {
                var objList = _transactionManager.SearchMyReceipts(searchCriteria, importerId);
                if (objList == null || !objList.Any())
                {
                    return new List<PaymentReceiptObject>();
                }

                return objList;
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<PaymentReceiptObject>();
            }
        }

       public List<PaymentReceiptObject> SearchReceipts(string searchCriteria)
       {
           try
           {
               var objList = _transactionManager.SearchReceipts(searchCriteria);
               if (objList == null || !objList.Any())
               {
                   return new List<PaymentReceiptObject>();
               }

               return objList;
           }
           catch (Exception ex)
           {
               ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
               return new List<PaymentReceiptObject>();
           }
       }
	}

}
