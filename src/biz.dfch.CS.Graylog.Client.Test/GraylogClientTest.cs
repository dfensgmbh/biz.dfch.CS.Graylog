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

        #region Streams

        [TestMethod]
        [TestCategory("SkipOnTeamCity")]
        public void GetStreamsTest()
        {
            //Test runs only if there is a file TestSettings.txt in the project folders containing the settings in the format
            //{key}={value}\n

            // Arrange
            TestSettings settings = TestSettings.Load();
            string username = settings.GetValue("Username");
            string password = settings.GetValue("Password");

            GraylogClient graylogClient = new GraylogClient();
            graylogClient.Login(Properties.Settings.Default.GraylogAPIUrl, username, password, TestEnvironment.TotalAttempts, TestEnvironment.BaseRetryIntervallMilliseconds);

            dynamic streamCollection = graylogClient.GetStreams(TestEnvironment.TotalAttempts, TestEnvironment.BaseRetryIntervallMilliseconds);

            Assert.IsNotNull(streamCollection, "No stream collection received");
            Assert.IsNotNull(streamCollection.streams, "No streams in stream collection");
            Assert.IsTrue(streamCollection.streams.Count > 0, "List of streams in stream collection is empty");
        }

        #endregion Streams

        #region Messages

        [TestMethod]
        [TestCategory("SkipOnTeamCity")]
        public void SearchMessagesTest()
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

            dynamic messageCollection = graylogClient.SearchMessages(streamName, new DateTime(2015, 1, 1), new DateTime(2016, 1, 1), TestEnvironment.TotalAttempts, TestEnvironment.BaseRetryIntervallMilliseconds);

            Assert.IsNotNull(messageCollection, "No message collection received");
            Assert.IsNotNull(messageCollection.messages, "No messages in message collection");
            Assert.IsTrue(messageCollection.messages.Count > 0, "List of messages in message collection is empty");

            object date = messageCollection.messages.FirstOrDefault().timestamp;
        }

        [TestMethod]
        [TestCategory("SkipOnTeamCity")]
        public void SearchMessagesWithSortTest()
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

            SearchParameters parameters = new SearchParameters()
            {
                SortFieldName = "hostname",
                SortOrder = SortOrder.Ascending,
            };
            dynamic messageCollection = graylogClient.SearchMessages(streamName, new DateTime(2015, 1, 1), new DateTime(2016, 1, 1), parameters, TestEnvironment.TotalAttempts, TestEnvironment.BaseRetryIntervallMilliseconds);

            Assert.IsNotNull(messageCollection, "No message collection received");
            Assert.IsNotNull(messageCollection.messages, "No messages in message collection");
            Assert.IsTrue(messageCollection.messages.Count > 0, "List of messages in message collection is empty");
        }

        [TestMethod]
        [TestCategory("SkipOnTeamCity")]
        public void SearchMessagesWithFieldListTest()
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

            SearchParameters parameters = new SearchParameters()
            {
                FieldNames = new List<string>() { "message", "hostname" },
            };
            dynamic messageCollection = graylogClient.SearchMessages(streamName, new DateTime(2015, 1, 1), new DateTime(2016, 1, 1), parameters, TestEnvironment.TotalAttempts, TestEnvironment.BaseRetryIntervallMilliseconds);

            Assert.IsNotNull(messageCollection, "No message collection received");
            Assert.IsNotNull(messageCollection.messages, "No messages in message collection");
            Assert.IsTrue(messageCollection.messages.Count > 0, "List of messages in message collection is empty");
        }

        [TestMethod]
        [TestCategory("SkipOnTeamCity")]
        public void SearchMessagesWithPagingTest()
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

            SearchParameters parameters = new SearchParameters()
            {
                Limit = 20,
                Offset = 30,
            };
            dynamic messageCollection = graylogClient.SearchMessages(streamName, new DateTime(2015, 1, 1), new DateTime(2016, 1, 1), parameters, TestEnvironment.TotalAttempts, TestEnvironment.BaseRetryIntervallMilliseconds);

            Assert.IsNotNull(messageCollection, "No message collection received");
            Assert.IsNotNull(messageCollection.messages, "No messages in message collection");
            Assert.IsTrue(messageCollection.messages.Count > 0, "List of messages in message collection is empty");
            Assert.IsTrue(messageCollection.messages.Count <= 20, "To many messages returned");
        }

        [TestMethod]
        [TestCategory("SkipOnTeamCity")]
        public void SearchMessagesWithPagingParameterDictionaryTest()
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

            object parameters = new Dictionary<string, object>()
            {
                { SearchParameters.LimitKey, 20 },
                { SearchParameters.OffsetKey, 30 },
            };
            dynamic messageCollection = graylogClient.SearchMessages(streamName, new DateTime(2015, 1, 1), new DateTime(2016, 1, 1), parameters, TestEnvironment.TotalAttempts, TestEnvironment.BaseRetryIntervallMilliseconds);

            Assert.IsNotNull(messageCollection, "No message collection received");
            Assert.IsNotNull(messageCollection.messages, "No messages in message collection");
            Assert.IsTrue(messageCollection.messages.Count > 0, "List of messages in message collection is empty");
            Assert.IsTrue(messageCollection.messages.Count <= 20, "To many messages returned");
        }

        [TestMethod]
        [TestCategory("SkipOnTeamCity")]
        public void SearchMessagesWithQueryTest()
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

            SearchParameters parameters = new SearchParameters()
            {
                Query = "message:\"PING~\"",
            };
            dynamic messageCollection = graylogClient.SearchMessages(streamName, new DateTime(2015, 1, 1), new DateTime(2016, 1, 1), parameters, TestEnvironment.TotalAttempts, TestEnvironment.BaseRetryIntervallMilliseconds);

            Assert.IsNotNull(messageCollection, "No message collection received");
            Assert.IsNotNull(messageCollection.messages, "No messages in message collection");
            Assert.IsTrue(messageCollection.messages.Count > 0, "List of messages in message collection is empty");
        }

        [TestMethod]
        [TestCategory("SkipOnTeamCity")]
        public void SearchMessagesWithFilterTest()
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

            SearchParameters parameters = new SearchParameters()
            {
                Filter = "streams:55b896c60cf20fc7dd52b273 AND message:\"PING\"",
            };
            dynamic messageCollection = graylogClient.SearchMessages(streamName, new DateTime(2015, 1, 1), new DateTime(2016, 1, 1), parameters, TestEnvironment.TotalAttempts, TestEnvironment.BaseRetryIntervallMilliseconds);

            Assert.IsNotNull(messageCollection, "No message collection received");
            Assert.IsNotNull(messageCollection.messages, "No messages in message collection");
            Assert.IsTrue(messageCollection.messages.Count > 0, "List of messages in message collection is empty");
        }

        #endregion Messages

    }
}
