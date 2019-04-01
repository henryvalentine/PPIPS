using System;
using System.Collections.Generic;
using System.Globalization;

namespace ImportPermitPortal.Helpers
{
   public static class EnumToObjList
    {
       public static List<GenericObject> ConvertEnumToList(Type enumType)
        {
            if (enumType == null)
                return new List<GenericObject>();
            var numArray = (int[])Enum.GetValues(enumType);
            var names = Enum.GetNames(enumType);
            var arrayList = new List<GenericObject>();
            try
            {
                for (int index = 0; index < numArray.GetLength(0); ++index)
                {
                    var doc = new GenericObject
                    {
                        Id = numArray[index]
                    };
                    if (names[index].IndexOf("_", StringComparison.Ordinal) > -1)
                        names[index] = names[index].Replace("_", " ");
                    doc.Name = names[index];
                    arrayList.Add(doc);
                }
            }
            catch (Exception ex)
            {
                return new List<GenericObject>();
            }
            return arrayList;
        }

       public static List<GenericObject> GeneratYearList(int startYear, int stopYear)
        {
            try
            {
                if (startYear < 1 || stopYear < 1 || (stopYear < startYear))
                {
                    return new List<GenericObject>();
                }

                var yearList = new List<GenericObject>();

                for (long i = startYear; i < stopYear + 1; i++)
                {
                    var doc = new GenericObject
                    {
                        Id = i,
                        Name = i.ToString(CultureInfo.InvariantCulture)
                    };
                    yearList.Add(doc); 
                } 
                return yearList;
            }
            catch (Exception ex)
            {
                return new List<GenericObject>();
            }
        }
    }
    
}
