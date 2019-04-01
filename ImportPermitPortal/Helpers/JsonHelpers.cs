using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Helpers;
using Newtonsoft.Json;

namespace ImportPermitPortal.Helpers
{
    public static class JsonHelpers
    {
        public static T CreateFromJsonStream<T>(this Stream stream)
        {
            var serializer = new JsonSerializer();
            T data;
            using (var streamReader = new StreamReader(stream))
            {
                data = (T)serializer.Deserialize(streamReader, typeof(T));
            }
            return data;
        }
        public static T CreateFromJsonString<T>(this String json)
        {
            T data;
            using (var stream = new MemoryStream(System.Text.Encoding.Default.GetBytes(json)))
            {
                data = CreateFromJsonStream<T>(stream);
            }
            return data;
        }

        public static T CreateFromJsonFile<T>(this String fileName)
        {
            T data;
            using (var fileStream = new FileStream(fileName, FileMode.Open))
            {
                data = CreateFromJsonStream<T>(fileStream);
            }
            return data;
        }

        public static string ConvertToStreamString(HttpPostedFileBase file)
        {
            try
            {
                 //byte[] image = new byte[file.ContentLength];
                 //file.InputStream.Read(image, 0, image.Length); 
                //var rdr = new BinaryReader(file.InputStream);
                //var imageByte = rdr.ReadBytes(file.ContentLength);
                 var streamString = JsonConvert.SerializeObject(file.InputStream);
                return streamString;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static T ConvertFromString<T>(byte[] json) 
        {
            var jsonStr = Encoding.UTF8.GetString(json);
            return JsonConvert.DeserializeObject<T>(jsonStr);
        }
    }

}