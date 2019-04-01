using System.Collections.Generic;
namespace ImportPermitPortal.DataObjects
{
    public class PObject
    {
       
        public string SmallPath { get; set; }
        public string BigPath { get; set; }

        public bool NoEmployee { get; set; }
        public bool NoCert { get; set; }
        public bool Error { get; set; }
    }
}
