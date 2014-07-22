//-------------------------------------------------------------------------------
// <copyright file="ExtensionSpecifications.cs" company="Appccelerate">
//   Copyright (c) 2008-2014
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
    using Appccelerate.StateMachine.Machine;

    using FakeItEasy;

    using global::Machine.Specifications;

    [Subject("Extensions")]
    public class When_adding_an_extension_to_the_state_machine
    {
        static IStateMachine<string, int> machine;
        static IExtension<string, int> extension;

        Establish context = () =>
            {
                machine = new PassiveStateMachine<string, int>();

                extension = A.Fake<IExtension<string, int>>();
            };

        Because of = () =>
            {
                machine.AddExtension(extension);
                machine.Initialize("initial");
            };

        It should_notify_extension_about_internal_events = () =>
            A.CallTo(extension).MustHaveHappened();
    }

    [Subject("Extensions")]
    public class When_clearing_all_extensions_from_the_state_machine
    {
        static IStateMachine<string, int> machine;
        static IExtension<string, int> extension;

        Establish context = () =>
        {
            machine = new PassiveStateMachine<string, int>();

            extension = A.Fake<IExtension<string, int>>();
            machine.AddExtension(extension);
        };

        Because of = () =>
        {
            machine.ClearExtensions();
            machine.Initialize("initial");
        };

        It should_not_anymore_notify_extension_about_internal_events = () =>
            A.CallTo(extension).MustNotHaveHappened();
    }
}