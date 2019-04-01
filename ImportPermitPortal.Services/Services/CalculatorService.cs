using System;
using System.Collections.Generic;
using System.Linq;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.Managers.Managers;

namespace ImportPermitPortal.Services.Services
{
    public class CalculatorServices
    {
        private readonly CalculatorManager _calculatorManager;
        public CalculatorServices()
        {
            _calculatorManager = new CalculatorManager();
        }

        public CalculatorObject AddCalculator(CalculatorObject calculator)
        {
            try
            {
                return _calculatorManager.AddCalculator(calculator);
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new CalculatorObject();
            }
        }

        //public long DeleteCalculator(long calculatorId)
        //{
        //    try
        //    {
        //        return _calculatorManager.DeleteCalculator(calculatorId);
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
        //        return 0;
        //    }
        //}

        //public long UpdateCalculator(CalculatorObject calculator)
        //{
        //    try
        //    {
        //        return _calculatorManager.UpdateCalculator(calculator);
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
        //        return 0;
        //    }
        //}

        //public CalculatorObject GetCalculator(long calculatorId)
        //{
        //    try
        //    {
        //        return _calculatorManager.GetCalculator(calculatorId);

        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
        //        return new CalculatorObject();
        //    }
        //}

        //public List<CalculatorObject> GetCalculators()
        //{
        //    try
        //    {
        //        return _calculatorManager.GetCalculators();

        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
        //        return new List<CalculatorObject>();
        //    }
        //}
        //public List<CalculatorObject> GetCalculators(long userProfileId)
        //{
        //    try
        //    {
        //        return _calculatorManager.GetCalculators(userProfileId);

        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
        //        return new List<CalculatorObject>();
        //    }
        //}
        
        //public UserProfileObject GetCalculatorAdmin(int calculatorId)
        //{
        //    try
        //    {
        //        return _calculatorManager.GetCalculatorAdmin(calculatorId);

        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
        //        return new UserProfileObject();
        //    }
        //}

        //public List<CalculatorObject> GetLocalCalculators()
        //{
        //    try
        //    {
        //        return _calculatorManager.GetLocalCalculators();
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
        //        return new List<CalculatorObject>();
        //    }
        //}

        //public List<CalculatorObject> GetCalculators(int? itemsPerPage, int? pageNumber, out int countG)
        //{
        //    try
        //    {
        //        var objList = _calculatorManager.GetCalculators(itemsPerPage, pageNumber, out countG);
        //        if (objList == null || !objList.Any())
        //        {
        //            return new List<CalculatorObject>();
        //        }

        //        return objList;
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
        //        countG = 0;
        //        return new List<CalculatorObject>();
        //    }
        //}

        //public List<CalculatorObject> Search(string searchCriteria)
        //{
        //    try
        //    {
        //        var objList = _calculatorManager.Search(searchCriteria);
        //        if (objList == null || !objList.Any())
        //        {
        //            return new List<CalculatorObject>();
        //        }

        //        return objList;
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
        //        return new List<CalculatorObject>();
        //    }
        //}

    }

}
