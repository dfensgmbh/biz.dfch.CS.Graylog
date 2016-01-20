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
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Linq;
using System.Collections;

namespace biz.dfch.CS.Graylog.Client
{
    public class GraylogClient
    {
        #region Url Constants

        private const string MessagesAbsoluteDateRangeUrl = "search/universal/absolute";
        private const string StreamsUrl = "streams";

        #endregion Url Constants

        #region Constants

        /// <summary>
        /// Default total attempts for calls
        /// </summary>
        protected const int TOTAL_ATTEMPTS = 5;
        /// <summary>
        /// Default base retry intervall milliseconds
        /// </summary>
        private const int BASE_RETRY_INTERVAL_MILLISECONDS = 5 * 1000;
        /// <summary>
        /// Url parameter date format
        /// </summary>
        private const string URL_PARAMETER_DATE_FORMAT = "yyyy-MM-dd HH:mm:ss";
        /// <summary>
        /// The key of the field name blacklist filter
        /// </summary>
        private const string FIELD_NAME_BLACKLIST_FILTER = "biz.dfch.CS.Graylog.Client.FieldBlacklistFilter";

        #endregion Constants

        #region Properties

        /// <summary>
        /// Current total attempts that are made for a request
        /// </summary>
        public int TotalAttempts { get; set; }
        /// <summary>
        /// Current base wait intervall between request attempts in milliseconds
        /// </summary>
        public int BaseRetryIntervallMilliseconds { get; set; }
        /// <summary>
        /// Url to the redmine API
        /// </summary>
        public string GraylogUrl { get; set; }
        /// <summary>
        /// The user name for authentication
        /// </summary>
        public string Username { get; set; }
        /// <summary>
        /// The password for authentication
        /// </summary>
        public string Password { get; set; }
        /// <summary>
        /// True if the the user could succsefully be authorized on the server
        /// </summary>
        public bool IsLoggedIn { get; private set; }

        /// <summary>
        /// The list of filters to decide which fields should be returned
        /// </summary>
        private Dictionary<string, IFieldFilter> fieldFilters;
        /// <summary>
        /// The list of filters to decide which messages should be returned
        /// </summary>
        private Dictionary<string, IMessageFilter> messageFilters;

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Creates an Instance of the Class <see cref="GraylogClient"/>
        /// </summary>
        public GraylogClient()
        {
            this.TotalAttempts = GraylogClient.TOTAL_ATTEMPTS;
            this.BaseRetryIntervallMilliseconds = GraylogClient.BASE_RETRY_INTERVAL_MILLISECONDS;
            this.fieldFilters = new Dictionary<string, IFieldFilter>();
            this.fieldFilters.Add(GraylogClient.FIELD_NAME_BLACKLIST_FILTER, new FieldNameBlacklistFilter());
            this.messageFilters = new Dictionary<string, IMessageFilter>();
        }

        #endregion Constructors

        #region Login / Logout

        /// <summary>
        ///  Checks if the user can be authorized on the server
        /// </summary>
        /// <param name="redmineUrl">Url of the redmine api</param>
        /// <param name="username">The user name for authentication</param>
        /// <param name="password">The password for authentication</param>
        /// <returns>True if the user could be authorized on the server</returns>
        public bool Login(string redmineUrl, string username, string password)
        {
            return this.Login(redmineUrl, username, password, this.TotalAttempts, this.BaseRetryIntervallMilliseconds);
        }

        /// <summary>
        ///  Checks if the user can be authorized on the server
        /// </summary>
        /// <param name="graylogUrl">Url of the redmine api</param>
        /// <param name="username">The user name for authentication</param>
        /// <param name="password">The password for authentication</param>
        /// <param name="totalAttempts">Total attempts that are made for a request</param>
        /// <param name="baseRetryIntervallMilliseconds">Default base retry intervall milliseconds</param>
        /// <returns>True if the user could be authorized on the server</returns>
        public bool Login(string graylogUrl, string username, string password, int totalAttempts, int baseRetryIntervallMilliseconds)
        {
            #region Contract
            Contract.Requires(!string.IsNullOrEmpty(graylogUrl), "No graylog url defined");
            Contract.Requires(!string.IsNullOrEmpty(username), "No username defined");
            Contract.Requires(!string.IsNullOrEmpty(password), "No password defined");
            Contract.Requires(totalAttempts > 0, "TotalAttempts must be greater than 0");
            Contract.Requires(baseRetryIntervallMilliseconds > 0, "BaseRetryIntervallMilliseconds must be greater than 0");
            #endregion Contract

            Trace.WriteLine(string.Format("GraylogClient.Login({0}, {1}, {2}, {3}, {4})", graylogUrl, username, password, totalAttempts, baseRetryIntervallMilliseconds));

            this.Logout(); // Ensure old login info is removed

            HttpRequestHandler requestHandler = new HttpRequestHandler(graylogUrl, username, password);
            //ToDo: Use correct url to get some objects from graylog
            DynamicJsonObject result = requestHandler.MakeRequest<DynamicJsonObject>(GraylogClient.StreamsUrl, HttpMethod.Get, null, totalAttempts, baseRetryIntervallMilliseconds);
            this.IsLoggedIn = null != result;

            if (this.IsLoggedIn)
            {
                this.GraylogUrl = graylogUrl;
                this.Username = username;
                this.Password = password;
            }
            else
            {
                throw new Exception("User could not be authorized");
            }

            return this.IsLoggedIn;
        }

        /// <summary>
        /// Removes all login information
        /// </summary>
        public void Logout()
        {
            Trace.WriteLine(string.Format("GraylogClient.Logout()"));
            this.Username = null;
            this.Password = null;
            this.GraylogUrl = null;
            this.IsLoggedIn = false;
        }

        #endregion Login / Logout

        #region Streams

        /// <summary>
        /// Loads the streams
        /// </summary>
        /// <returns>The collection objec containing the streams</returns>
        public DynamicJsonObject GetStreams()
        {
            return this.GetStreams(this.TotalAttempts, this.BaseRetryIntervallMilliseconds);
        }

        /// <summary>
        /// Loads the streams
        /// </summary>
        /// <param name="totalAttempts">Total attempts that are made for a request</param>
        /// <param name="baseRetryIntervallMilliseconds">Default base retry intervall milliseconds in job polling</param>
        /// <returns>The collection objec containing the streams</returns>
        public DynamicJsonObject GetStreams(int totalAttempts, int baseRetryIntervallMilliseconds)
        {
            #region Contract
            Contract.Requires(this.IsLoggedIn, "Not logged in, call method login first");
            Contract.Requires(totalAttempts > 0, "TotalAttempts must be greater than 0");
            Contract.Requires(baseRetryIntervallMilliseconds > 0, "BaseRetryIntervallMilliseconds must be greater than 0");
            #endregion Contract

            HttpRequestHandler requestHandler = new HttpRequestHandler(this.GraylogUrl, this.Username, this.Password);
            DynamicJsonObject result = requestHandler.MakeRequest<DynamicJsonObject>(GraylogClient.StreamsUrl, HttpMethod.Get, null, totalAttempts, baseRetryIntervallMilliseconds);

            return result;
        }

        #endregion Streams

        #region Messages

        /// <summary>
        /// Searches for Messages
        /// </summary>
        /// <param name="streamTitle">The name of the stream in which to search messages</param>
        /// <param name="from">From date for the search range</param>
        /// <param name="to">To date for the search range</param>
        public DynamicJsonObject SearchMessages(string streamTitle, DateTime from, DateTime to)
        {
            return this.SearchMessages(streamTitle, from, to, (SearchParameters)null, this.TotalAttempts, this.BaseRetryIntervallMilliseconds);
        }

        /// <summary>
        /// Searches for Messages
        /// </summary>
        /// <param name="streamTitle">The name of the stream in which to search messages</param>
        /// <param name="from">From date for the search range</param>
        /// <param name="to">To date for the search range</param>
        /// <param name="totalAttempts">Total attempts that are made for a request</param>
        /// <param name="baseRetryIntervallMilliseconds">Default base retry intervall milliseconds in job polling</param>
        public DynamicJsonObject SearchMessages(string streamTitle, DateTime from, DateTime to, int totalAttempts, int baseRetryIntervallMilliseconds)
        {
            return this.SearchMessages(streamTitle, from, to, (SearchParameters)null, totalAttempts, baseRetryIntervallMilliseconds);
        }

        /// <summary>
        /// Searches for Messages
        /// </summary>
        /// <param name="streamTitle">The name of the stream in which to search messages</param>
        /// <param name="from">From date for the search range</param>
        /// <param name="to">To date for the search range</param>
        /// <param name="parameters">Optional query parameters</param>
        /// <returns>The collection objec containing the found messages</returns>
        public DynamicJsonObject SearchMessages(string streamTitle, DateTime from, DateTime to, object parameters)
        {
            Contract.Assert((parameters is SearchParameters) || (parameters is Dictionary<string, object>), "Parameters format is not suported (valid formats are 'SearchParameters' or 'Dictionary<string,object>'");

            DynamicJsonObject result = null;
            if (parameters is SearchParameters)
            {
                result = this.SearchMessages(streamTitle, from, to, (SearchParameters)parameters);
            }
            else if (parameters is Dictionary<string, object>)
            {
                result = this.SearchMessages(streamTitle, from, to, (Dictionary<string, object>)parameters);
            }

            return result;
        }

        /// <summary>
        /// Searches for Messages
        /// </summary>
        /// <param name="streamTitle">The name of the stream in which to search messages</param>
        /// <param name="from">From date for the search range</param>
        /// <param name="to">To date for the search range</param>
        /// <param name="parameters">Optional query parameters</param>
        /// <returns>The collection objec containing the found messages</returns>
        public DynamicJsonObject SearchMessages(string streamTitle, DateTime from, DateTime to, Dictionary<string, object> parameters)
        {
            SearchParameters searchParameters = new SearchParameters(parameters);
            return this.SearchMessages(streamTitle, from, to, searchParameters);
        }

        /// <summary>
        /// Searches for Messages
        /// </summary>
        /// <param name="streamTitle">The name of the stream in which to search messages</param>
        /// <param name="from">From date for the search range</param>
        /// <param name="to">To date for the search range</param>
        /// <param name="parameters">Optional query parameters</param>
        /// <returns>The collection objec containing the found messages</returns>
        public DynamicJsonObject SearchMessages(string streamTitle, DateTime from, DateTime to, SearchParameters parameters)
        {
            return this.SearchMessages(streamTitle, from, to, parameters, this.TotalAttempts, this.BaseRetryIntervallMilliseconds);
        }

        /// <summary>
        /// Searches for Messages
        /// </summary>
        /// <param name="streamTitle">The name of the stream in which to search messages</param>
        /// <param name="from">From date for the search range</param>
        /// <param name="to">To date for the search range</param>
        /// <param name="parameters">Optional query parameters</param>
        /// <param name="totalAttempts">Total attempts that are made for a request</param>
        /// <param name="baseRetryIntervallMilliseconds">Default base retry intervall milliseconds in job polling</param>
        /// <returns>The collection objec containing the found messages</returns>
        public DynamicJsonObject SearchMessages(string streamTitle, DateTime from, DateTime to, object parameters,
            int totalAttempts, int baseRetryIntervallMilliseconds)
        {
            Contract.Assert((parameters is SearchParameters) || (parameters is Dictionary<string, object>), "Parameters format is not suported (valid formats are 'SearchParameters' or 'Dictionary<string,object>'");

            DynamicJsonObject result = null;
            if (parameters is SearchParameters)
            {
                result = this.SearchMessages(streamTitle, from, to, (SearchParameters)parameters, totalAttempts, baseRetryIntervallMilliseconds);
            }
            else if (parameters is Dictionary<string, object>)
            {
                result = this.SearchMessages(streamTitle, from, to, (Dictionary<string, object>)parameters, totalAttempts, baseRetryIntervallMilliseconds);
            }

            return result;
        }

        /// <summary>
        /// Searches for Messages
        /// </summary>
        /// <param name="streamTitle">The name of the stream in which to search messages</param>
        /// <param name="from">From date for the search range</param>
        /// <param name="to">To date for the search range</param>
        /// <param name="parameters">Optional query parameters</param>
        /// <param name="totalAttempts">Total attempts that are made for a request</param>
        /// <param name="baseRetryIntervallMilliseconds">Default base retry intervall milliseconds in job polling</param>
        /// <returns>The collection objec containing the found messages</returns>
        public DynamicJsonObject SearchMessages(string streamTitle, DateTime from, DateTime to, Dictionary<string, object> parameters,
            int totalAttempts, int baseRetryIntervallMilliseconds)
        {
            SearchParameters searchParameters = new SearchParameters(parameters);
            return this.SearchMessages(streamTitle, from, to, searchParameters, totalAttempts, baseRetryIntervallMilliseconds);
        }

        /// <summary>
        /// Searches for Messages
        /// </summary>
        /// <param name="streamTitle">The name of the stream in which to search messages</param>
        /// <param name="from">From date for the search range</param>
        /// <param name="to">To date for the search range</param>
        /// <param name="parameters">Optional query parameters</param>
        /// <param name="totalAttempts">Total attempts that are made for a request</param>
        /// <param name="baseRetryIntervallMilliseconds">Default base retry intervall milliseconds in job polling</param>
        /// <returns>The collection objec containing the found messages</returns>
        public DynamicJsonObject SearchMessages(string streamTitle, DateTime from, DateTime to, SearchParameters parameters,
            int totalAttempts, int baseRetryIntervallMilliseconds)
        {
            #region Contract
            Contract.Requires(this.IsLoggedIn, "Not logged in, call method login first");
            Contract.Requires(!string.IsNullOrEmpty(streamTitle), "No stream title defined");
            Contract.Requires(DateTime.MinValue != from, "From date can not be min date");
            Contract.Requires(DateTime.MaxValue != from, "From date can not be max date");
            Contract.Requires(DateTime.MinValue != to, "To date can not be min date");
            Contract.Requires(DateTime.MaxValue != to, "To date can not be max date");
            Contract.Requires(totalAttempts > 0, "TotalAttempts must be greater than 0");
            Contract.Requires(baseRetryIntervallMilliseconds > 0, "BaseRetryIntervallMilliseconds must be greater than 0");
            #endregion Contract

            //Add Stream filter
            dynamic streamCollection = this.GetStreams(totalAttempts, baseRetryIntervallMilliseconds);
            List<dynamic> streams = streamCollection.streams;
            dynamic stream = streams.FirstOrDefault(s => s.title == streamTitle);
            Contract.Assert(null != (object)stream, string.Format("No stream with title '{0}' found", streamTitle));
            string streamQuery = string.Format("streams:\"{0}\"", stream.id);

            //Add optional query parameters
            string optionalParameters = "";
            if (null != parameters)
            {
                if (!string.IsNullOrEmpty(parameters.Query))
                {
                    streamQuery = string.Format("({0}) AND ({1})", streamQuery, HttpUtility.UrlEncode(parameters.Query));
                }
                if (parameters.Limit > 0)
                {
                    optionalParameters = string.Format("{0}&limit={1}", optionalParameters, parameters.Limit);
                }
                if (parameters.Offset > 0)
                {
                    optionalParameters = string.Format("{0}&offset={1}", optionalParameters, parameters.Offset);
                }
                if (!string.IsNullOrEmpty(parameters.Filter))
                {
                    optionalParameters = string.Format("{0}&filter={1}", optionalParameters, HttpUtility.UrlEncode(parameters.Filter));
                }
                if ((null != parameters.FieldNames) && (parameters.FieldNames.Count > 0))
                {
                    optionalParameters = string.Format("{0}&fields={1}", optionalParameters, HttpUtility.UrlEncode(string.Join(",", parameters.FieldNames)));
                }
                if (!string.IsNullOrEmpty(parameters.SortFieldName))
                {
                    string sortParameter = null;
                    switch (parameters.SortOrder)
                    {
                        case SortOrder.Ascending:
                            sortParameter = string.Format("{0}:asc", parameters.SortFieldName);
                            break;
                        case SortOrder.Descending:
                            sortParameter = string.Format("{0}:desc", parameters.SortFieldName);
                            break;
                    }
                    optionalParameters = string.Format("{0}&sort={1}", optionalParameters, HttpUtility.UrlEncode(sortParameter));
                }
            }
            string url = string.Format("{0}?query={1}&from={2}&to={3}{4}", GraylogClient.MessagesAbsoluteDateRangeUrl,
                HttpUtility.UrlEncode(streamQuery), HttpUtility.UrlEncode(from.ToString(GraylogClient.URL_PARAMETER_DATE_FORMAT)),
                HttpUtility.UrlEncode(to.ToString(GraylogClient.URL_PARAMETER_DATE_FORMAT)), optionalParameters);

            HttpRequestHandler requestHandler = new HttpRequestHandler(this.GraylogUrl, this.Username, this.Password);
            dynamic result = requestHandler.MakeRequest<DynamicJsonObject>(url, HttpMethod.Get, null, totalAttempts, baseRetryIntervallMilliseconds);

            result.messages = this.ApplyMessageFilter(result.messages);
            this.ApplyFieldFilter(result.messages);

            return result;
        }

        /// <summary>
        /// Applies all field filters registered
        /// </summary>
        /// <param name="objectsToFilter">The list of messages to filter</param>
        private void ApplyFieldFilter(dynamic objectsToFilter)
        {
            if ((null != objectsToFilter) &&(objectsToFilter.Count>0))
            {
                foreach (dynamic item in objectsToFilter)
                {
                    if ((item is DynamicJsonObject)&&(null!=item.message))
                    {
                        foreach (IFieldFilter filter in this.fieldFilters.Values)
                        {
                            ((DynamicJsonObject)item.message).ApplyFieldFilter(filter);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Applies all message filters registered
        /// </summary>
        /// <param name="objectsToFilter">The list of messages to filter</param>
        /// <returns>The filtered list of messages</returns>
        private dynamic ApplyMessageFilter(dynamic objectsToFilter)
        {
            dynamic messages = new ArrayList();
            if ((null != objectsToFilter) && (objectsToFilter.Count > 0))
            {
                foreach (dynamic item in objectsToFilter)
                {
                    if ((item is DynamicJsonObject) && (null != item.message))
                    {
                        bool removeMessage = false;
                        foreach (IMessageFilter filter in this.messageFilters.Values)
                        {
                            removeMessage = filter.RemoveMessage(item.message);
                            if(removeMessage)
                            {
                                break;
                            }
                        }
                        if (!removeMessage)
                        {
                            messages.Add(item);
                        }
                    }
                }
            }
            return messages;
        }

        #endregion Messages

        #region Manage Filters

        /// <summary>
        /// Adds a filter to decide which fields should be returned
        /// </summary>
        /// <param name="key">The key of the filter</param>
        /// <param name="filter">The filter</param>
        public void RegisterFieldFilter(string key, IFieldFilter filter)
        {
            #region Contracts
            Contract.Assert(!string.IsNullOrEmpty(key), "No key for the filter defined");
            Contract.Assert(null != filter, "No filter defined");
            #endregion Contracts
            
            this.fieldFilters.Add(key, filter);
        }

        /// <summary>
        /// Removes a filter to decide which fields should be returned
        /// </summary>
        /// <param name="key">The key of the filter to remove</param>
        public void UnregisterFieldFilter(string key)
        {
            #region Contracts
            Contract.Assert(GraylogClient.FIELD_NAME_BLACKLIST_FILTER != key, "The field name blacklist filter can not be removed");
            #endregion Contracts

            if ((!string.IsNullOrEmpty(key)) && (this.fieldFilters.ContainsKey(key)))
            {
                this.fieldFilters.Remove(key);
            }
        }

        /// <summary>
        /// Adds a filter to decide which messages should be returned
        /// </summary>
        /// <param name="key">The key of the filter</param>
        /// <param name="filter">The filter</param>
        public void RegisterMessageFilter(string key, IMessageFilter filter)
        {
            #region Contracts
            Contract.Assert(!string.IsNullOrEmpty(key), "No key for the filter defined");
            Contract.Assert(null != filter, "No filter defined");
            #endregion Contracts

            this.messageFilters.Add(key, filter);
        }

        /// <summary>
        /// Removes a filter to decide which messages should be returned
        /// </summary>
        /// <param name="key">The key of the filter to remove</param>
        public void UnregisterMessageFilter(string key)
        {
            if ((!string.IsNullOrEmpty(key)) && (this.messageFilters.ContainsKey(key)))
            {
                this.messageFilters.Remove(key);
            }
        }

        #endregion Manage Filters
    }
}
