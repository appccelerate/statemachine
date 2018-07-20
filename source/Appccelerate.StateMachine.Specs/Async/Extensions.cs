﻿// <copyright file="Extensions.cs" company="Appccelerate">
//   Copyright (c) 2008-2017 Appccelerate
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

namespace Appccelerate.StateMachine.Async
{
    using Appccelerate.StateMachine.AsyncMachine;
    using FakeItEasy;
    using Xbehave;

    public class Extensions
    {
        private const string Name = "machine";

        [Scenario]
        public void BeforeExecutingEntryActions(
            AsyncPassiveStateMachine<string, int> machine,
            IExtension<string, int> extension)
        {
            "establish an extension".x(()
                => extension = A.Fake<IExtension<string, int>>());

            "establish a state machine using the extension"._(async () =>
            {
                machine = new AsyncPassiveStateMachine<string, int>(Name);

                machine.AddExtension(extension);

                machine.In("0")
                    .On(1).Goto("1");

                await machine.Initialize("0");
                await machine.Start();
            });

            "when firing an event onto the state machine"._(()
                => machine.Fire(1));

            "it should call EnteringState on registered extensions for target state".x(()
                => A.CallTo(() => extension.EnteringState(
                        A<IStateMachineInformation<string, int>>.That.Matches(x => x.Name == Name && x.CurrentStateId == "1"),
                        A<IState<string, int>>.That.Matches(x => x.Id == "1"),
                        A<ITransitionContext<string, int>>.That.Matches(x => x.EventId.Value == 1)))
                    .MustHaveHappened());
        }

        [Scenario]
        public void BeforeExecutingEntryActionsHierarchical(
            AsyncPassiveStateMachine<string, string> machine,
            IExtension<string, string> extension)
        {
            "establish an extension".x(()
                => extension = A.Fake<IExtension<string, string>>());

            "establish a hierarchical state machine using the extension"._(async () =>
            {
                machine = new AsyncPassiveStateMachine<string, string>(Name);

                machine.AddExtension(extension);

                machine.DefineHierarchyOn("A")
                    .WithHistoryType(HistoryType.None)
                    .WithInitialSubState("A0");

                machine.In("0")
                    .On("A0").Goto("A0");

                await machine.Initialize("0");
                await machine.Start();
            });

            "when firing an event onto the state machine"._(()
                => machine.Fire("A0"));

            "it should call EnteringState on registered extensions for entered super states of target state".x(()
                => A.CallTo(() => extension.EnteringState(
                        A<IStateMachineInformation<string, string>>.That.Matches(x => x.Name == Name && x.CurrentStateId == "A0"),
                        A<IState<string, string>>.That.Matches(x => x.Id == "A"),
                        A<ITransitionContext<string, string>>.That.Matches(x => x.EventId.Value == "A0")))
                    .MustHaveHappened());

            "it should call EnteringState on registered extensions for entered leaf target state".x(()
                => A.CallTo(() => extension.EnteringState(
                        A<IStateMachineInformation<string, string>>.That.Matches(x => x.Name == Name && x.CurrentStateId == "A0"),
                        A<IState<string, string>>.That.Matches(x => x.Id == "A0"),
                        A<ITransitionContext<string, string>>.That.Matches(x => x.EventId.Value == "A0")))
                    .MustHaveHappened());
        }
    }
}