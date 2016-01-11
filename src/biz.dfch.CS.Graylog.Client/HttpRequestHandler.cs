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
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using biz.dfch.CS.Utilities.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace biz.dfch.CS.Graylog.Client
{
    internal class HttpRequestHandler
    {
        private const string AUTHORIZATION_HEADER_NAME = "Authorization";
        private const string AUTHORIZATION_BASIC_SCHEME = "Basic {0}";

        private string baseUrl;
        private string username;
        private string password;

        internal HttpRequestHandler(string baseUrl, string username, string password)
        {
            #region Contract
            Contract.Requires(!string.IsNullOrWhiteSpace(baseUrl), "No base url defined");
            Contract.Requires(!string.IsNullOrWhiteSpace(username), "No username defined");
            Contract.Requires(!string.IsNullOrWhiteSpace(password), "No password defined");
            #endregion Contract

            this.baseUrl = baseUrl;
            this.username = username;
            this.password = password;
        }

        public ResponseType MakeRequest<ResponseType>(string url, HttpMethod method, object body, int totalAttempts, int baseWaitingMilliseconds)
        {
            #region Contract
            Contract.Requires(!string.IsNullOrWhiteSpace(url), "No url defined");
            Contract.Requires(null != method, "No http method defined");
            Contract.Requires(totalAttempts > 0, "Total attempts must be bigger than 0");
            Contract.Requires(baseWaitingMilliseconds > 0, "Base waiting milliseconds must be bigger than 0");
            #endregion Contract

            ResponseType result = default(ResponseType);
            int currentWaitingMillis = baseWaitingMilliseconds;
            for (int actualAttempt = 1; actualAttempt <= totalAttempts; actualAttempt++)
            {
                try
                {
                    Trace.WriteLine(string.Format("START DoMakeRequest: {0} {1}{2} [{3}/{4}] ...", method, baseUrl, url, actualAttempt, totalAttempts));
                    result = this.DoMakeRequest<ResponseType>(url, method, body);
                    Trace.WriteLine(string.Format("END DoMakeRequest: {0} {1}{2} [{3}/{4}] COMPLETED.", method, baseUrl, url, actualAttempt, totalAttempts));
                    break;
                }
                catch (Exception ex)
                {
                    Exception logEx = ex;
                    while (logEx != null)
                    {
                        Trace.WriteLine(string.Format("ERROR MakeRequest: {0} {1}{2} [{3}/{4}] FAILED.", method, baseUrl, url, actualAttempt, totalAttempts));
                        Trace.WriteLine(logEx.Message);
                        Trace.WriteLine(logEx.StackTrace);
                        logEx = logEx.InnerException;
                    }

                    if (actualAttempt >= totalAttempts)
                    {
                        throw;
                    }
                }
                Thread.Sleep(currentWaitingMillis);
                currentWaitingMillis = currentWaitingMillis * 2;
            }
            return result;
        }

        private ResponseType DoMakeRequest<ResponseType>(string url, HttpMethod method, object body)
        {
            ResponseType result = default(ResponseType);

            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(this.baseUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                string basicAuthString = Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format("{0}:{1}", this.username, this.password)));
                client.DefaultRequestHeaders.Add(HttpRequestHandler.AUTHORIZATION_HEADER_NAME, string.Format(HttpRequestHandler.AUTHORIZATION_BASIC_SCHEME, basicAuthString));

                JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings()
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    DateFormatString = "yyyy-MM-dd'T'HH:mm:ss.fffzzz",
                };

                HttpRequestMessage message = new HttpRequestMessage(method, url);
                if (body != null)
                {
                    message.Content = new StringContent(JsonConvert.SerializeObject(body, jsonSerializerSettings), Encoding.UTF8, "application/json");
                }

                Task<HttpResponseMessage> responseTask = client.SendAsync(message);
                responseTask.Wait();

                if (responseTask.IsFaulted)
                {
                    throw responseTask.Exception;
                }
                else
                {
                    if (responseTask.Result.IsSuccessStatusCode)
                    {
                        Task<string> contentTask = responseTask.Result.Content.ReadAsStringAsync();
                        contentTask.Wait();

                        if (typeof(ResponseType) == typeof(string))
                        {
                            result = (ResponseType)(object)contentTask.Result;
                        }
                        else if (typeof(ResponseType) == typeof(DynamicJsonObject))
                        {
                            JavaScriptSerializer serializer = new JavaScriptSerializer();
                            serializer.RegisterConverters(new[] { new DynamicJsonConverter() });
                            result = (ResponseType)serializer.Deserialize(contentTask.Result, typeof(object));
                        }
                        else
                        {
                            result = JsonConvert.DeserializeObject<ResponseType>(contentTask.Result, jsonSerializerSettings);
                        }
                    }
                    else
                    {  
                        string exceptionMessage = string.Format("{0} {1}", (int)responseTask.Result.StatusCode, responseTask.Result.ReasonPhrase);
                        Trace.WriteLine(exceptionMessage);
                        switch (responseTask.Result.StatusCode)
                        {
                            case System.Net.HttpStatusCode.BadRequest:
                                throw new ArgumentException(exceptionMessage);

                            case System.Net.HttpStatusCode.Unauthorized:
                            case System.Net.HttpStatusCode.Forbidden:
                                throw new UnauthorizedAccessException(exceptionMessage);

                            case System.Net.HttpStatusCode.MethodNotAllowed:
                                throw new InvalidOperationException(exceptionMessage);

                            default:
                                throw new Exception(exceptionMessage);
                        }

                    }
                }
            }
            return result;
        }
    }
}
