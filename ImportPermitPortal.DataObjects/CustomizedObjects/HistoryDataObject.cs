using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ImportPermitPortal.DataObjects
{
    public class HistoryDataObject
    {
        public string ReferenceCode { get; set; }


        public string StepName { get; set; }
        public string EmployeeName { get; set; }
        public string StatusName { get; set; }
        public string AssignedTimeStr { get; set; }
        public string DueTimeStr { get; set; }
        public string FinishedTimeStr { get; set; }
        public string ProcessName { get; set; }
        public string OutComeCodeStr { get; set; }
       
    }
}