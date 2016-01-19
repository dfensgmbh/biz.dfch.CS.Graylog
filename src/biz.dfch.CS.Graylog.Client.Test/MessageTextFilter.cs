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

namespace biz.dfch.CS.Graylog.Client.Test
{
    public class MessageTextFilter: IMessageFilter
    {
        private string filterText;

        public MessageTextFilter(string filterText)
        {
            this.filterText = filterText;
        }

        public bool RemoveMessage(dynamic message)
        {
            bool removeMessage = true;
            if ((null != message) && (null != message.message))
            {
                removeMessage = !message.message.Contains(this.filterText);
            }
            return removeMessage;
        }
    }
}
