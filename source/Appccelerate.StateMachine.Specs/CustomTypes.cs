//-------------------------------------------------------------------------------
// <copyright file="CustomTypes.cs" company="Appccelerate">
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
    using FluentAssertions;
    using Xbehave;

    //// see http://www.appccelerate.com/statemachinecustomtypes.html for an explanation why states and events have to be IComparable
    //// and not IEquatable.
    public class CustomTypes
    {
        [Scenario]
        public void CustomTypesForStatesAndEvents(
            PassiveStateMachine<MyState, MyEvent> machine,
            bool arrivedInStateB)
        {
            "establish a state machine with custom types for states and events"._(() =>
                {
                    machine = new PassiveStateMachine<MyState, MyEvent>();

                    machine.In(new MyState("A"))
                        .On(new MyEvent(1)).Goto(new MyState("B"));

                    machine.In(new MyState("B"))
                        .ExecuteOnEntry(() => arrivedInStateB = true);

                    machine.Initialize(new MyState("A"));

                    machine.Start();
                });

            "when using the state machine"._(() =>
                machine.Fire(new MyEvent(1)));

            "it should use equals to compare states and events"._(() =>
                arrivedInStateB.Should().BeTrue("state B should be current state"));
        }

        public class MyState : IComparable
        {
            public MyState(string name)
            {
                this.Name = name;
            }

            private string Name { get; set; }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                {
                    return false;
                }

                if (ReferenceEquals(this, obj))
                {
                    return true;
                }

                if (obj.GetType() != this.GetType())
                {
                    return false;
                }

                return this.Equals((MyState)obj);
            }

            public override int GetHashCode()
            {
                return this.Name != null ? this.Name.GetHashCode() : 0;
            }

            public int CompareTo(object obj)
            {
                throw new InvalidOperationException("state machine should not use CompareTo");
            }

            private bool Equals(MyState other)
            {
                return string.Equals(this.Name, other.Name);
            }
        }

        public class MyEvent : IComparable
        {
            public MyEvent(int identifier)
            {
                this.Identifier = identifier;
            }

            private int Identifier { get; set; }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                {
                    return false;
                }

                if (ReferenceEquals(this, obj))
                {
                    return true;
                }

                if (obj.GetType() != this.GetType())
                {
                    return false;
                }

                return this.Equals((MyEvent)obj);
            }

            public override int GetHashCode()
            {
                return this.Identifier;
            }

            public int CompareTo(object obj)
            {
                throw new InvalidOperationException("state machine should not use CompareTo");
            }

            private bool Equals(MyEvent other)
            {
                return this.Identifier == other.Identifier;
            }
        }
    }
}