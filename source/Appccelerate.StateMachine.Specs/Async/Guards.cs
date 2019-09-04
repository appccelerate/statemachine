//-------------------------------------------------------------------------------
// <copyright file="Guards.cs" company="Appccelerate">
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

namespace Appccelerate.StateMachine.Specs.Async
{
    using System.Threading.Tasks;
    using FluentAssertions;
    using Xbehave;

    public class Guards
    {
        private const int SourceState = 1;
        private const int DestinationState = 2;
        private const int ErrorState = 3;

        private const int Event = 2;

        [Scenario]
        public void MatchingGuard(
            AsyncPassiveStateMachine<int, int> machine,
            CurrentStateExtension currentStateExtension)
        {
            "establish a state machine with guarded transitions".x(async () =>
            {
                machine = new AsyncPassiveStateMachine<int, int>();

                currentStateExtension = new CurrentStateExtension();
                machine.AddExtension(currentStateExtension);

                machine.In(SourceState)
                    .On(Event)
                        .If(() => false).Goto(ErrorState)
                        .If(async () => await Task.FromResult(false)).Goto(ErrorState)
                        .If(async () => await Task.FromResult(true)).Goto(DestinationState)
                        .If(() => true).Goto(ErrorState)
                        .Otherwise().Goto(ErrorState);

                await machine.Initialize(SourceState);
                await machine.Start();
            });

            "when an event is fired".x(()
                => machine.Fire(Event));

            "it should take transition guarded with first matching guard".x(()
                => currentStateExtension.CurrentState.Should().Be(DestinationState));
        }

        [Scenario]
        public void NoMatchingGuard(
            AsyncPassiveStateMachine<int, int> machine)
        {
            bool declined = false;

            "establish state machine with no matching guard".x(async () =>
            {
                machine = new AsyncPassiveStateMachine<int, int>();

                var currentStateExtension = new CurrentStateExtension();
                machine.AddExtension(currentStateExtension);

                machine.In(SourceState)
                    .On(Event)
                    .If(() => Task.FromResult(false)).Goto(ErrorState);

                machine.TransitionDeclined += (sender, e) => declined = true;

                await machine.Initialize(SourceState);
                await machine.Start();
            });

            "when an event is fired".x(()
                => machine.Fire(Event));

            "it should notify about declined transition".x(()
                => declined.Should().BeTrue("TransitionDeclined event should be fired"));
        }

        [Scenario]
        public void OtherwiseGuard(
            AsyncPassiveStateMachine<int, int> machine,
            CurrentStateExtension currentStateExtension)
        {
            "establish a state machine with otherwise guard and no machting other guard".x(async () =>
                {
                    machine = new AsyncPassiveStateMachine<int, int>();

                    currentStateExtension = new CurrentStateExtension();
                    machine.AddExtension(currentStateExtension);

                    machine.In(SourceState)
                        .On(Event)
                            .If(() => Task.FromResult(false)).Goto(ErrorState)
                            .Otherwise().Goto(DestinationState);

                    await machine.Initialize(SourceState);
                    await machine.Start();
                });

            "when an event is fired".x(()
                => machine.Fire(Event));

            "it should_take_transition_guarded_with_otherwise".x(()
                => currentStateExtension.CurrentState.Should().Be(DestinationState));
        }
    }
}