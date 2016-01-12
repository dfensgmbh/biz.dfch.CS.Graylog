using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Reflection;
using System.Web;
using biz.dfch.CS.Graylog.Client;

namespace biz.dfch.CS.Graylog.Api.Formatters
{
    public class CSVFormatter : BufferedMediaTypeFormatter
    {
        public CSVFormatter()
        {
            this.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/csv"));
        }

        public override bool CanWriteType(Type type)
        {
            return true;
        }

        public override bool CanReadType(Type type)
        {
            return false;
        }

        public override void WriteToStream(Type type, object value, System.IO.Stream writeStream, System.Net.Http.HttpContent content)
        {
            string csv = null;
            if (value is DynamicJsonObject)
            {
                csv = ((DynamicJsonObject)value).ToCSV();
            }
            else if (null != value)
            {
                Dictionary<string, object> values = CSVFormatter.ToDictionary(value);
                DynamicJsonObject dynamicObject = new DynamicJsonObject(values);
                csv = dynamicObject.ToCSV();
            }
            using (StreamWriter writer = new StreamWriter(writeStream))
            {
                writer.Write(csv);
            }
        }

        private static Dictionary<string, object> ToDictionary(object obj)
        {
            Dictionary<string, object> values = new Dictionary<string, object>();
            foreach (PropertyInfo property in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                object propertyValue = property.GetValue(obj);
                if (null != propertyValue)
                {
                    if ((propertyValue.GetType().IsValueType) || (propertyValue is string))
                    {
                        values.Add(property.Name, propertyValue);
                    }
                    else
                    {
                        if (propertyValue is IEnumerable)
                        {
                            ArrayList list = new ArrayList();
                            foreach (object item in (IEnumerable)propertyValue)
                            {
                                if ((item.GetType().IsValueType) || (item is string))
                                {
                                    list.Add(item);
                                }
                                else
                                {
                                    Dictionary<string, object> subObjectValues = CSVFormatter.ToDictionary(item);
                                    list.Add(subObjectValues);
                                }
                                
                            }
                            values.Add(property.Name, list);
                        }
                        else
                        {
                            Dictionary<string, object> subObjectValues = CSVFormatter.ToDictionary(propertyValue);
                            values.Add(property.Name, subObjectValues);
                        }
                    }
                }
            }
            return values;
        }
    }
}