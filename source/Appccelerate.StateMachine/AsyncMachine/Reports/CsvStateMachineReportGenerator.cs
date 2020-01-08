//-------------------------------------------------------------------------------
// <copyright file="CsvStateMachineReportGenerator.cs" company="Appccelerate">
//   Copyright (c) 2008-2019 Appccelerate
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

namespace Appccelerate.StateMachine.AsyncMachine.Reports
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using States;

    /// <summary>
    /// Generator for csv reports of states and transitions of a state machine.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    public class CsvStateMachineReportGenerator<TState, TEvent> : IStateMachineReport<TState, TEvent>
        where TState : notnull
        where TEvent : notnull
    {
        private readonly TextWriter statesWriter;

        private readonly TextWriter transitionsWriter;

        /// <summary>
        /// Initializes a new instance of the <see cref="CsvStateMachineReportGenerator{TState, TEvent}"/> class.
        /// </summary>
        /// <param name="statesWriter">The stream where the states are written to.</param>
        /// <param name="transitionsWriter">The stream where the transitions are written to.</param>
        public CsvStateMachineReportGenerator(
            TextWriter statesWriter,
            TextWriter transitionsWriter)
        {
            this.statesWriter = statesWriter;
            this.transitionsWriter = transitionsWriter;
        }

        /// <summary>
        /// Generates a report of the state machine.
        /// </summary>
        /// <param name="name">The name of the state machine.</param>
        /// <param name="states">The states.</param>
        /// <param name="initialStateId">The initial state id.</param>
        public void Report(string name, IEnumerable<IStateDefinition<TState, TEvent>> states, TState initialStateId)
        {
            states = states.ToList();

            this.ReportStates(states);
            this.ReportTransitions(states);
        }

        private void ReportStates(IEnumerable<IStateDefinition<TState, TEvent>> states)
        {
            var writer = new CsvStatesWriter<TState, TEvent>(
                this.statesWriter);

            writer.Write(states);
        }

        private void ReportTransitions(IEnumerable<IStateDefinition<TState, TEvent>> states)
        {
            var writer = new CsvTransitionsWriter<TState, TEvent>(
                this.transitionsWriter);

            writer.Write(states);
        }
    }
}