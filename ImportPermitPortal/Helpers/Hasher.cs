using System;
using System.Security.Cryptography;
using System.Text;
using ImportPermitPortal.DataObjects.Helpers;

namespace ImportPermitPortal.Helpers
{
    public static class Hasher
    {
        public static string GenerateHash(string rrr)
        {
            //merchantId+rrr+api_key

            //var x = "2547916" + rrr + "1946";

            var x = ((long)RemitaServiceType.MecharntId) + rrr + ((long)RemitaServiceType.ApiKey);

            var hash = CreateHash(x);
            return hash;
        }

         public static string GenerateHashStatus(string rrr)
        {
            // SHA512 (RRR+api_key+merchantId)

            var x = rrr + ((long)RemitaServiceType.ApiKey) + ((long)RemitaServiceType.MecharntId);

            //var x = rrr + "1946" + "2547916";
           
            var hash = CreateHash(x);
            return hash;
        }

        public static string CreateHash(string req)
        {
            return BitConverter.ToString(new SHA512CryptoServiceProvider().ComputeHash(Encoding.Default.GetBytes(req))).Replace("-", string.Empty).ToLower();
        }

    }
}
