using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;
using biz.dfch.CS.Graylog.Client;

namespace biz.dfch.CS.Graylog.Api.Formatters
{
    public class DynamicObjectJsonFormatter : JsonMediaTypeFormatter
    {
        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content, TransportContext transportContext)
        {
            Task writeTask = null;
            if (value is DynamicJsonObject)
            {
                writeTask = new Task(() =>
                {
                    DynamicJsonObject dynamicObject = (DynamicJsonObject)value;
                    string json = dynamicObject.ToJson();
                    using (StreamWriter writer = new StreamWriter(writeStream))
                    {
                        writer.Write(json);
                    }
                });
                writeTask.Start();
            }
            else
            {
                writeTask = base.WriteToStreamAsync(type, value, writeStream, content, transportContext);
            }
            return writeTask;
        }

        public override Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
        {
            Task<object> readTask = null;
            if (type == typeof(DynamicJsonObject))
            {
                readTask = new Task<object>(() =>
                {
                    JavaScriptSerializer serializer = new JavaScriptSerializer();
                    serializer.RegisterConverters(new[] { new DynamicJsonConverter() });
                    string json = null;
                    using (StreamReader reader = new StreamReader(readStream))
                    {
                        json = reader.ReadToEnd();
                    }
                    return serializer.Deserialize(json, typeof(object));
                });
                readTask.Start();
            }
            else
            {
                readTask = base.ReadFromStreamAsync(type, readStream, content, formatterLogger);
            }
            return readTask;
        }
    }
}