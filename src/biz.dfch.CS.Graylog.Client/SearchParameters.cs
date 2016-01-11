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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace biz.dfch.CS.Graylog.Client
{
    public class SearchParameters
    {
        #region Constants

        public static string QueryKey = "Query";
        public static string LimitKey = "Limit";
        public static string OffsetKey = "Offset";
        public static string SortFieldNameKey = "SortFieldName";
        public static string SortOrderKey = "SortOrder";
        public static string FieldNamesKey = "FieldNames";
        public static string FilterKey = "Filter";

        #endregion Constants

        /// <summary>
        /// Lucene query for finding the objects
        /// </summary>
        public string Query { get; set; }
        /// <summary>
        /// Maximal number of returned objects
        /// </summary>
        public int Limit { get; set; }
        /// <summary>
        /// Number of objects to skip
        /// </summary>
        public int Offset { get; set; }
        /// <summary>
        /// Fieldname after which should be sorted
        /// </summary>
        public string SortFieldName { get; set; }
        /// <summary>
        /// Sort order (ascending or descending)
        /// </summary>
        public SortOrder SortOrder { get; set; }
        /// <summary>
        /// The names of the fields to return 
        /// </summary>
        public List<string> FieldNames { get; set; }
        /// <summary>
        /// Filter for the objects
        /// </summary>
        public string Filter { get; set; }

        public SearchParameters()
        {
        }

        public SearchParameters(Dictionary<string, object> values)
        {
            this.Query = SearchParameters.GetValue<string>(values, SearchParameters.QueryKey);
            this.Limit = SearchParameters.GetValue<int>(values, SearchParameters.LimitKey);
            this.Offset = SearchParameters.GetValue<int>(values, SearchParameters.OffsetKey);
            this.SortFieldName = SearchParameters.GetValue<string>(values, SearchParameters.SortFieldNameKey);
            this.SortOrder = SearchParameters.GetValue<SortOrder>(values, SearchParameters.SortOrderKey);
            this.FieldNames = SearchParameters.GetValue<List<string>>(values, SearchParameters.FieldNamesKey);
            this.Filter = SearchParameters.GetValue<string>(values, SearchParameters.FilterKey);
        }

        private static PropertyType GetValue<PropertyType>(Dictionary<string, object> values, string key)
        {
            PropertyType value = default(PropertyType);
            if ((!string.IsNullOrEmpty(key)) && (null != values) && (values.ContainsKey(key)) && (values[key] is PropertyType))
            {
                value = (PropertyType)values[key];
            }
            return value;
        }
    }
}
