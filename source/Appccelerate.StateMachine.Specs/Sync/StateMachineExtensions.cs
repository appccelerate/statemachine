//-------------------------------------------------------------------------------
// <copyright file="StateMachineExtensions.cs" company="Appccelerate">
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
//-------------------------------------------------------------------------------

namespace Appccelerate.StateMachine.Specs.Sync
{
    using FakeItEasy;
    using Machine;
    using Xbehave;

    public class StateMachineExtensions
    {
        [Scenario]
        public void AddingExtensions(
            IStateMachine<string, int> machine,
            IExtension<string, int> extension)
        {
            "establish a state machine".x(() =>
                {
                    machine = new PassiveStateMachine<string, int>();

                    extension = A.Fake<IExtension<string, int>>();
                });

            "when adding an extension".x(() =>
                {
                    machine.AddExtension(extension);
                    machine.Initialize("initial");
                });

            "it should notify extension about internal events".x(() =>
                A.CallTo(extension).MustHaveHappened());
        }

        [Scenario]
        public void ClearingExtensions(
            IStateMachine<string, int> machine,
            IExtension<string, int> extension)
        {
            "establish a state machine with an extension".x(() =>
                {
                    machine = new PassiveStateMachine<string, int>();

                    extension = A.Fake<IExtension<string, int>>();
                    machine.AddExtension(extension);
                });

            "when clearing all extensions from the state machine".x(() =>
                {
                    machine.ClearExtensions();
                    machine.Initialize("initial");
                });

            "it should not anymore notify extension about internal events".x(() =>
                A.CallTo(extension)
                    .MustNotHaveHappened());
        }
    }
}