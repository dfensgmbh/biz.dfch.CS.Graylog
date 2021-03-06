﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using biz.dfch.CS.Graylog.Client;

namespace biz.dfch.CS.Graylog.Api.Controllers
{
    public class StreamsController : ApiController
    {
        public HttpResponseMessage Get()
        {
            GraylogClient graylogClient = new GraylogClient();
            //ToDo: Get user name and password from header
            graylogClient.Login(Properties.Settings.Default.GraylogAPIUrl, "", "");
            DynamicJsonObject streamsCollection = graylogClient.GetStreams();
            return this.Request.CreateResponse(HttpStatusCode.OK, streamsCollection);
        }
    }
}