//-------------------------------------------------------------------------------
// <copyright file="StateMachineReportGenerator.cs" company="Appccelerate">
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
    using System.Globalization;
    using System.Linq;
    using System.Text;

    using Appccelerate.Formatters;
    using Appccelerate.StateMachine.Machine;
    using Appccelerate.StateMachine.Machine.Transitions;

    /// <summary>
    /// Creates a textual report of a state machine.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    public class StateMachineReportGenerator<TState, TEvent> : IStateMachineReport<TState, TEvent>
        where TState : IComparable
        where TEvent : IComparable
    {
        /// <summary>
        /// Gets the resulting report.
        /// </summary>
        /// <value>The result.</value>
        public string Result { get; private set; }

        /// <summary>
        /// Generates a report of the state machine.
        /// </summary>
        /// <param name="name">The name of the state machine.</param>
        /// <param name="states">The states.</param>
        /// <param name="initialStateId">The initial state id.</param>
        public void Report(string name, IEnumerable<IState<TState, TEvent>> states, Initializable<TState> initialStateId)
        {
            states = states.ToList();

            Guard.AgainstNullArgument("states", states);
            Guard.AgainstNullArgument("initialStateId", initialStateId);

            var report = new StringBuilder();

            const string Indentation = "    ";

            report.AppendFormat("{0}: initial state = {1}{2}", name, initialStateId.IsInitialized ? initialStateId.Value.ToString() : "none", Environment.NewLine);

            // write states
            var rootStates = states.Where(state => state.SuperState == null);
            foreach (var state in rootStates)
            {
                this.ReportState(state, report, Indentation);
            }

            this.Result = report.ToString();
        }

        /// <summary>
        /// Reports the state name, initial state, history type, entry and exit action.
        /// </summary>
        /// <param name="report">The report.</param>
        /// <param name="indentation">The current indentation.</param>
        /// <param name="state">The state.</param>
        private static void ReportStateNameInitialStateHistoryTypeEntryAndExitAction(StringBuilder report, string indentation, IState<TState, TEvent> state)
        {
            report.AppendFormat(
                CultureInfo.InvariantCulture,
                "{0}{1}: initial state = {2} history type = {3}{4}",
                indentation,
                state,
                state.InitialState != null ? state.InitialState.ToString() : "None",
                state.HistoryType,
                Environment.NewLine);
            indentation += "    ";

            report.AppendFormat(
                "{0}entry action: {1}{2}", 
                indentation,
                FormatHelper.ConvertToString(state.EntryActions.Select(action => action.Describe()), ", "), 
                Environment.NewLine);
            
            report.AppendFormat(
                "{0}exit action: {1}{2}", 
                indentation,
                FormatHelper.ConvertToString(state.ExitActions.Select(action => action.Describe()), ", "), 
                Environment.NewLine);
        }

        /// <summary>
        /// Reports the transition.
        /// </summary>
        /// <param name="report">The report.</param>
        /// <param name="indentation">The indentation.</param>
        /// <param name="transition">The transition.</param>
        private static void ReportTransition(StringBuilder report, string indentation, TransitionInfo<TState, TEvent> transition)
        {
            report.AppendFormat(
                CultureInfo.InvariantCulture,
                "{0}{1} -> {2} actions: {3} guard: {4}{5}",
                indentation,
                transition.EventId,
                transition.Target != null ? transition.Target.ToString() : "internal",
                FormatHelper.ConvertToString(transition.Actions.Select(action => action.Describe()), ", "),
                transition.Guard != null ? transition.Guard.Describe() : string.Empty,
                Environment.NewLine);
        }

        /// <summary>
        /// Creates the part of the report for the specified state.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <param name="report">The report to add to.</param>
        /// <param name="indentation">The current indentation level.</param>
        private void ReportState(IState<TState, TEvent> state, StringBuilder report, string indentation)
        {
            ReportStateNameInitialStateHistoryTypeEntryAndExitAction(report, indentation, state);

            indentation += "    ";

            foreach (var transition in state.Transitions.GetTransitions())
            {
                ReportTransition(report, indentation, transition);
            }

            foreach (var subState in state.SubStates)
            {
                this.ReportState(subState, report, indentation);
            }
        }
    }
}