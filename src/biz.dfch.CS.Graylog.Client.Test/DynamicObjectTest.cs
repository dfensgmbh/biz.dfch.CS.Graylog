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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace biz.dfch.CS.Graylog.Client.Test
{
    [TestClass]
    public class DynamicObjectTest
    {
        [TestMethod]
        public void ToJsonTest()
        {
            Dictionary<string, object> values = new Dictionary<string, object>()
            {
                {"Name", "Hello"},
                {"Description", "World"},
                {"Count", 3},
                {"Subobject", new Dictionary<string,object>()
                    {
                        {"Name", "Inner object name"},
                        {"Description", "Inner object description"},
                    }
                },
                {"List", new ArrayList()
                    {
                        new Dictionary<string,object>()
                        {
                            {"Name", "list object 1"},
                            {"Description", "list object 1 description"},
                        },
                        new Dictionary<string,object>()
                        {
                            {"Name", "list object 2"},
                            {"Description", "list object 2 description"},
                        },
                    }
                }
            };

            DynamicJsonObject dynamicObject = new DynamicJsonObject(values);
            string json = dynamicObject.ToJson();

            string expected = "{\"Name\":\"Hello\",\"Description\":\"World\",\"Count\":3,\"Subobject\":{\"Name\":\"Inner object name\",\"Description\":\"Inner object description\"},\"List\":[{\"Name\":\"list object 1\",\"Description\":\"list object 1 description\"},{\"Name\":\"list object 2\",\"Description\":\"list object 2 description\"}]}";
            Assert.AreEqual(expected, json);
        }
    }
}
