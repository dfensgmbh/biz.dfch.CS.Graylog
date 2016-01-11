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
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace biz.dfch.CS.Graylog.Client.Test
{
    [TestClass]
    public class GraylogClientTest
    {
        [TestMethod]
        public void DummyTestForTeamCity()
        {
        }

        #region Messages

        [TestMethod]
        [TestCategory("SkipOnTeamCity")]
        public void InvokeCoreNodeTemplateWithTenantIDSucceeds()
        {
            //Test runs only if there is a file TestSettings.txt in the project folders containing the settings in the format
            //{key}={value}\n

            // Arrange
            TestSettings settings = TestSettings.Load();
            string username = settings.GetValue("Username");
            string password = settings.GetValue("Password");
            string streamName = settings.GetValue("StreamName");

            GraylogClient graylogClient = new GraylogClient();
            graylogClient.Login(Properties.Settings.Default.GraylogAPIUrl, username, password, TestEnvironment.TotalAttempts, TestEnvironment.BaseRetryIntervallMilliseconds);

            dynamic messageCollection = graylogClient.SearchMessages(streamName, null, new DateTime(2015, 1, 1), new DateTime(2016, 1, 1), 0, 0, TestEnvironment.TotalAttempts, TestEnvironment.BaseRetryIntervallMilliseconds);

            Assert.IsNotNull(messageCollection, "No message collection received");
            Assert.IsNotNull(messageCollection.messages, "No messages in message collection");
            Assert.IsTrue(messageCollection.messages.Count > 0, "List of messages in message collection is empty");
        }

        #endregion Messages

    }
}
