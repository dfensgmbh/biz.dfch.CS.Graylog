using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using biz.dfch.CS.Graylog.Client;

namespace biz.dfch.CS.Graylog.Api.Controllers
{
    public class MessagesController : ApiController
    {
        public HttpResponseMessage Get(string streamTitle, DateTime from, DateTime to)
        {
            GraylogClient graylogClient = new GraylogClient();
            //ToDo: Get user name and password from header
            graylogClient.Login(Properties.Settings.Default.GraylogAPIUrl, "", "");
            DynamicJsonObject messageCollection = graylogClient.SearchMessages(streamTitle, from, to);
            return this.Request.CreateResponse(HttpStatusCode.OK, messageCollection);
        }
   }
}