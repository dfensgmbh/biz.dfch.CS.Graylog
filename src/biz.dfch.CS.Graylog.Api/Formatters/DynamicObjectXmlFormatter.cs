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
using System.Xml;
using biz.dfch.CS.Graylog.Client;
using Newtonsoft.Json;

namespace biz.dfch.CS.Graylog.Api.Formatters
{
    public class DynamicObjectXmlFormatter : XmlMediaTypeFormatter
    {
        public override bool CanWriteType(Type type)
        {
            bool canWrite = base.CanWriteType(type);
            if (type == typeof(DynamicJsonObject))
            {
                canWrite = true;
            }
            return canWrite;
        }

        public override bool CanReadType(Type type)
        {
            bool canRead = base.CanWriteType(type);
            if (type == typeof(DynamicJsonObject))
            {
                canRead = true;
            }
            return canRead;
        }

        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content, TransportContext transportContext)
        {
            Task writeTask = null;
            if (value is DynamicJsonObject)
            {
                writeTask = new Task(() =>
                    {
                        DynamicJsonObject dynamicObject = (DynamicJsonObject)value;
                        string json = dynamicObject.ToJson();
                        string xml = JsonConvert.DeserializeXmlNode(json, typeof(DynamicJsonObject).Name).OuterXml;
                        using (StreamWriter writer = new StreamWriter(writeStream))
                        {
                            writer.Write(xml);
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
                    string xml = null;
                    using (StreamReader reader = new StreamReader(readStream))
                    {
                        xml = reader.ReadToEnd();
                    }
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(xml);
                    string json = JsonConvert.SerializeXmlNode(xmlDoc, Newtonsoft.Json.Formatting.None, true);
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