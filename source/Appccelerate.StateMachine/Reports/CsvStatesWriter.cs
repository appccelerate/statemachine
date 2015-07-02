//-------------------------------------------------------------------------------
// <copyright file="CsvStatesWriter.cs" company="Appccelerate">
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

namespace Appccelerate.StateMachine.Reports
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using Appccelerate.Formatters;
    using Appccelerate.StateMachine.Machine;

    /// <summary>
    /// Writes the states of a state machine to a stream as csv.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    public class CsvStatesWriter<TState, TEvent>
        where TState : IComparable
        where TEvent : IComparable
    {
        private readonly StreamWriter writer;

        /// <summary>
        /// Initializes a new instance of the <see cref="CsvStatesWriter&lt;TState, TEvent&gt;"/> class.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public CsvStatesWriter(StreamWriter writer)
        {
            this.writer = writer;
        }

        /// <summary>
        /// Writes the specified states.
        /// </summary>
        /// <param name="states">The states.</param>
        public void Write(IEnumerable<IState<TState, TEvent>> states)
        {
            states = states.ToList();

            Guard.AgainstNullArgument("states", states);

            this.WriteStatesHeader();

            foreach (var state in states)
            {
                this.ReportState(state);
            }
        }

        private void WriteStatesHeader()
        {
            this.writer.WriteLine("Source;Entry;Exit;Children");
        }

        private void ReportState(IState<TState, TEvent> state)
        {
            string entry = FormatHelper.ConvertToString(state.EntryActions.Select(action => action.Describe()), ", ");
            string exit = FormatHelper.ConvertToString(state.ExitActions.Select(action => action.Describe()), ", ");
            string children = FormatHelper.ConvertToString(state.SubStates.Select(s => s.Id.ToString()), ", ");

            this.writer.WriteLine(
                "{0};{1};{2};{3}",
                state.Id,
                entry,
                exit,
                children);
        }
    }
}