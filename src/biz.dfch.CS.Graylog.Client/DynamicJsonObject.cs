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
                        sb.AppendFormat("\"{0}\":\"{1}\"", name, value);
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
    }
}
