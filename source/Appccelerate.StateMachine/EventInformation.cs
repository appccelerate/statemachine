//-------------------------------------------------------------------------------
// <copyright file="EventInformation.cs" company="Appccelerate">
//   Copyright (c) 2008-2015
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
// </copyright>
//-------------------------------------------------------------------------------

namespace Appccelerate.StateMachine
{
    using System;

    public class EventInformation<TEvent>
        where TEvent : IComparable
    {
        public EventInformation(TEvent eventId, object eventArgument)
        {
            this.EventId = eventId;
            this.EventArgument = eventArgument;
        }

        public TEvent EventId { get; private set; }

        public object EventArgument { get; private set; }
    }
}