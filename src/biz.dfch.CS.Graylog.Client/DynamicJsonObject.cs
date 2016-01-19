/**
 * Copyright 2015 d-fens GmbH
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
 
﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace biz.dfch.CS.Graylog.Client
{
    public class DynamicJsonObject : DynamicObject
    {
        private const string CSVFieldSepparator = ";";
        private static readonly string CSVLineSepparator = Environment.NewLine;

        private readonly IDictionary<string, object> values;

        public DynamicJsonObject(IDictionary<string, object> values)
        {
            #region Contract
            Contract.Assert(null != values, "No values defined");
            #endregion Contract

            this.values = values;
        }
        public string ToJson()
        {
            StringBuilder sb = new StringBuilder("");
            this.ToJson(sb);
            return sb.ToString();
        }

        private void ToJson(StringBuilder sb)
        {
            sb.Append("{");
            bool firstInDictionary = true;
            foreach (KeyValuePair<string, object> pair in values)
            {
                object value = pair.Value;
                if (null != value)
                {
                    if (!firstInDictionary)
                        sb.Append(",");
                    firstInDictionary = false;
                    string name = pair.Key;
                    if (value is string)
                    {
                        string escapedString = (string)value;
                        escapedString = escapedString.Replace("\\", "\\\\");
                        escapedString = escapedString.Replace("\"", "\\\"");
                        sb.AppendFormat("\"{0}\":\"{1}\"", name, escapedString);
                    }
                    else if (value is bool)
                    {
                        sb.AppendFormat("\"{0}\":{1}", name, value.ToString().ToLower());
                    }
                    else if (value is IDictionary<string, object>)
                    {
                        sb.AppendFormat("\"{0}\":", name);
                        new DynamicJsonObject((IDictionary<string, object>)value).ToJson(sb);
                    }
                    else if (value is ArrayList)
                    {
                        sb.AppendFormat("\"{0}\":[", name);
                        bool firstInArray = true;
                        foreach (object arrayValue in (ArrayList)value)
                        {
                            if (!firstInArray)
                            {
                                sb.Append(",");
                            }
                            firstInArray = false;
                            if (arrayValue is IDictionary<string, object>)
                            {
                                new DynamicJsonObject((IDictionary<string, object>)arrayValue).ToJson(sb);
                            }
                            else if (arrayValue is string)
                            {
                                sb.AppendFormat("\"{0}\"", arrayValue);
                            }
                            else
                            {
                                sb.AppendFormat("{0}", arrayValue);
                            }
                        }
                        sb.Append("]");
                    }
                    else
                    {
                        sb.AppendFormat("\"{0}\":{1}", name, value);
                    }
                }
            }
            sb.Append("}");
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (!this.values.TryGetValue(binder.Name, out result))
            {
                // return null to avoid exception.  caller can check for null this way...
                result = null;
                return true;
            }

            result = DynamicJsonObject.WrapResultObject(result);
            return true;
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            if (indexes.Length == 1 && indexes[0] != null)
            {
                if (!this.values.TryGetValue(indexes[0].ToString(), out result))
                {
                    // return null to avoid exception.  caller can check for null this way...
                    result = null;
                    return true;
                }

                result = DynamicJsonObject.WrapResultObject(result);
                return true;
            }

            return base.TryGetIndex(binder, indexes, out result);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            bool valueSet = false;
            if (!string.IsNullOrEmpty(binder.Name))
            {
                if(!this.values.ContainsKey(binder.Name))
                {
                    this.values.Add(binder.Name, null);
                }
                this.values[binder.Name] = value;
                valueSet = true;
            }
            return valueSet;
        }

        private static object WrapResultObject(object result)
        {
            IDictionary<string, object> dictionary = result as IDictionary<string, object>;
            if (dictionary != null)
                return new DynamicJsonObject(dictionary);

            ArrayList arrayList = result as ArrayList;
            if (arrayList != null && arrayList.Count > 0)
            {
                if (arrayList[0] is IDictionary<string, object>)
                {
                    result = new List<object>(arrayList.Cast<IDictionary<string, object>>().Select(x => new DynamicJsonObject(x)));
                }
                else
                {
                    result = new List<object>(arrayList.Cast<object>());
                }
            }

            return result;
        }

        public string ToCSV()
        {
            StringBuilder sb = new StringBuilder("");
            this.ToCSV(sb, null, true, 0);
            return sb.ToString();
        }

        private Dictionary<string, int> ToCSV(StringBuilder sb, Dictionary<string, int> fieldNameColumnMapping, bool addHeader, int currentLevel)
        {
            if (null == fieldNameColumnMapping)
            {
                fieldNameColumnMapping = new Dictionary<string, int>();
            }
            int lastUsedColumnIndex = 0;
            List<CSVValue> csvValues = new List<CSVValue>();
            StringBuilder listSb = new StringBuilder();
            StringBuilder subObjectSb = new StringBuilder();
            foreach (KeyValuePair<string, object> pair in this.values)
            {
                if (pair.Value is Dictionary<string, object>)
                {
                    Dictionary<string, object> subObject = (Dictionary<string, object>)pair.Value;
                    if (subObject.Count > 0)
                    {
                        DynamicJsonObject.Indent(subObjectSb, currentLevel);
                        subObjectSb.Append(pair.Key);
                        subObjectSb.Append(DynamicJsonObject.CSVLineSepparator);
                        new DynamicJsonObject((IDictionary<string, object>)pair.Value).ToCSV(subObjectSb, null, true, currentLevel + 1);
                    }
                }
                else if (pair.Value is ArrayList)
                {
                    ArrayList list = (ArrayList)pair.Value;
                    if (list.Count > 0)
                    {
                        Dictionary<string, int> listFieldNameColumnMapping = new Dictionary<string, int>();
                        StringBuilder listItemSb = new StringBuilder();
                        foreach (object listItem in list)
                        {
                            if (listItem is Dictionary<string, object>)
                            {
                                listFieldNameColumnMapping = new DynamicJsonObject((IDictionary<string, object>)listItem).ToCSV(listItemSb, listFieldNameColumnMapping, false, currentLevel + 1);
                            }
                            else
                            {
                                DynamicJsonObject.Indent(listItemSb, currentLevel);
                                listItemSb.Append(listItem.ToString());
                                listItemSb.Append(DynamicJsonObject.CSVLineSepparator);
                            }
                        }
                        DynamicJsonObject.Indent(listSb, currentLevel);
                        listSb.Append(pair.Key);
                        listSb.Append(DynamicJsonObject.CSVLineSepparator);
                        if (listFieldNameColumnMapping.Count > 0)
                        {
                            DynamicJsonObject.Indent(listSb, currentLevel + 1);
                            listSb.Append(string.Join(DynamicJsonObject.CSVFieldSepparator, listFieldNameColumnMapping.Keys.ToArray()));
                            listSb.Append(DynamicJsonObject.CSVLineSepparator);
                        }
                        listSb.Append(listItemSb);
                    }
                }
                else
                {
                    if (!fieldNameColumnMapping.ContainsKey(pair.Key))
                    {
                        lastUsedColumnIndex++;
                        fieldNameColumnMapping.Add(pair.Key, lastUsedColumnIndex);
                    }
                    csvValues.Add(new CSVValue(fieldNameColumnMapping[pair.Key], pair.Value));
                }
            }
            if (addHeader)
            {
                DynamicJsonObject.Indent(sb, currentLevel);   
                sb.Append(string.Join(DynamicJsonObject.CSVFieldSepparator, fieldNameColumnMapping.Keys.ToArray()));
                sb.Append(DynamicJsonObject.CSVLineSepparator);
            }
            DynamicJsonObject.Indent(sb, currentLevel); 
            sb.Append(string.Join(DynamicJsonObject.CSVFieldSepparator, csvValues.OrderBy(csvv => csvv.Position)
                .Select(csvv => (null == csvv.Value) ? "" : csvv.Value.ToString()).ToArray()));
            sb.Append(DynamicJsonObject.CSVLineSepparator);
            if (!string.IsNullOrWhiteSpace(listSb.ToString()))
            {
                sb.Append(listSb);
            }
            if (!string.IsNullOrWhiteSpace(subObjectSb.ToString()))
            {
                sb.Append(subObjectSb);
            }
            return fieldNameColumnMapping;
        }

        private static void Indent(StringBuilder sb,int numberOfFields)
        {
            for(int i=0;i<numberOfFields;i++)
            {
                sb.Append(DynamicJsonObject.CSVFieldSepparator);
            }
        }

        internal void ApplyFieldFilter(IFieldFilter filter)
        {
            IDictionary<string, object> originalValues = this.values.ToDictionary(kvp=>kvp.Key, kvp=>kvp.Value);
            this.values.Clear();
            foreach(KeyValuePair<string, object> value in originalValues)
            {
                if (!filter.RemoveField(value.Key, value.Value))
                {
                    this.values.Add(value.Key, value.Value);
                }
            }
        }
    }
}
