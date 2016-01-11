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
using System.Web.Script.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace biz.dfch.CS.Graylog.Client.Test
{
    [TestClass]
    public class DynamicJsonConverterTest
    {
        [TestMethod]
        public void DeserializeTest()
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            serializer.RegisterConverters(new[] { new DynamicJsonConverter() });

            string json = "{\"Name\":\"Hello\",\"Description\":\"World\",\"Count\":3, \"Factor\":4.5,\"Subobject\":{\"Name\":\"Inner object name\",\"Description\":\"Inner object description\"},\"List\":[{\"Name\":\"list object 1\",\"Description\":\"list object 1 description\"},{\"Name\":\"list object 2\",\"Description\":\"list object 2 description\"}]}";

            dynamic obj = (DynamicJsonObject)serializer.Deserialize(json, typeof(object));
            Assert.IsNotNull(obj);
            Assert.AreEqual("Hello", obj.Name);
            Assert.AreEqual(3, obj.Count);
            Assert.AreEqual(4.5m, obj.Factor); //non integer numbers are deserialized as decimal
            Assert.IsTrue(obj.Subobject is DynamicJsonObject);
            Assert.IsTrue(obj.List is List<object>);
            Assert.IsTrue(((List<object>)obj.List).FirstOrDefault() is DynamicJsonObject);
        }
    }
}
